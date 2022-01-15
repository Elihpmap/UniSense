using System;
using System.Collections;
using System.Reflection;
using UniSense.LowLevel;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UniSense
{
    /// <summary>
    /// A class to implement the Dualsense controller to Unity Input System and which provides base 
    /// methods to create and send instructions for the outputs of the controller (Haptics, lights, mic sensibility, etc.)
    /// </summary>
    [InputControlLayout(
        stateType = typeof(DualSenseHIDInputReport),
        displayName = "PS5 Controller")]
    [Preserve]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class DualSenseGamepadHID : DualShockGamepad
    {
        #region Attributes

        public ButtonControl leftTriggerButton { get; protected set; }
        public ButtonControl rightTriggerButton { get; protected set; }
        public ButtonControl playStationButton { get; protected set; }

        public ButtonControl micMuteButton { get; protected set; }


        /// <summary>
        /// Was the lattest update successfully send to the Dualsense
        /// </summary>
        public bool UpdateSucceeded { get; private set; }
        /// <summary>
        /// The last time the Dualsense was successfully updated, in seconds (corresponding to <see cref="Time.realtimeSinceStartup"/>)
        /// </summary>
        public float LastGamepadUpdateTime { get; private set; } = 0;
        /// <summary>
        /// Seconds since the Dualsense was last updated;
        /// </summary>
        public float TimeSinceLastGamepadUpdate => Time.realtimeSinceStartup - LastGamepadUpdateTime;

        private DualSenseGamepadState _newState;
        private bool stateModified;
        public DualSenseGamepadState NewState
        {
            get { return _newState; } // TODO Data out protection
            set
            {
                stateModified = true;
                CheckLegacyRumbleValidity(ref value.Motor);
                _newState = value; // TODO data in protection
            }
        }

        //TODO initialise to known gamepad startup default values
        private DualSenseGamepadState _currentGamepadState;
        public DualSenseGamepadState CurrentGamepadState
        {
            get { return _currentGamepadState; } // TODO Data out protection
            private set
            {
                _currentGamepadState = value;
            }
        }

        private bool _useLegacyHaptics = true;
        public bool UseLegacyHaptics
        {
            get { return _useLegacyHaptics; }
            set
            {
                if (!value)
                    ResetMotorSpeeds(true);
                _useLegacyHaptics = value;
            }
        }

        public delegate void HapticsControls(DualSenseGamepadHID dualSenseGamepad);

        //events invoked when the corresponding method is called and UseLegacyRumbles == false, use them to mute/resume HD rumble on your integration!
        //Due to the current implementation PauseHaptic() method will call both ShouldPauseHDHaptics and ShouldResetHDHaptics (in this order).
        public event HapticsControls ShouldPauseHDHaptics;
        public event HapticsControls ShouldResetHDHaptics;
        public event HapticsControls ShouldResumeHDHaptics;

        // Save values to allow pause and resume haptics functions
        /// <summary>
        /// A simple bool to flag the next gamepad update as one where the given haptic values 
        /// should not be saved (as they are reset values) and were the HapticPaused bool should be set to true afterwards.
        /// </summary>
        private bool shouldPauseHapticAtNextUpdate = false;
        public bool IsHapticPaused { get; private set; } = false;
        /// <summary>
        /// m_motorSpeeds == currentState.Motor except when haptic are paused, then it is the value currentState.Motor ould be if haptics weren't paused.
        /// </summary>
        private DualSenseMotorSpeed? m_motorSpeeds;
        private DualSenseTriggerState? m_rightTriggerState;
        private DualSenseTriggerState? m_leftTriggerState;

        #endregion


        #region Methods

#if UNITY_EDITOR
        static DualSenseGamepadHID()
        {
            Initialize();
        }
#endif

        /// <summary>
        /// Finds the first DualSense connected by the player or <c>null</c> if 
        /// there is no one connected to the system.
        /// </summary>
        /// <returns>A DualSenseGamepadHID instance or <c>null</c>.</returns>
        public static DualSenseGamepadHID FindFirst()
        {
            foreach (var gamepad in all)
            {
                var isDualSenseGamepad = gamepad is DualSenseGamepadHID;
                if (isDualSenseGamepad) return gamepad as DualSenseGamepadHID;
            }

            return null;
        }

        /// <summary>
        /// Finds the DualSense last used/connected by the player or <c>null</c> if 
        /// the current gamepad is not a DualSense or if there is no DualSense connected.
        /// </summary>
        /// <returns>A DualSenseGamepadHID instance or <c>null</c>.</returns>
        public static DualSenseGamepadHID FindCurrent() => Gamepad.current as DualSenseGamepadHID;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            InputSystem.RegisterLayout<DualSenseGamepadHID>(
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithManufacturer("Sony.+Entertainment")
                    .WithCapability("vendorId", 0x54C)
                    .WithCapability("productId", 0xCE6));
        }

        protected override void FinishSetup()
        {
            leftTriggerButton = GetChildControl<ButtonControl>("leftTriggerButton");
            rightTriggerButton = GetChildControl<ButtonControl>("rightTriggerButton");
            playStationButton = GetChildControl<ButtonControl>("systemButton");
            micMuteButton = GetChildControl<ButtonControl>("micMuteButton");

            base.FinishSetup();
        }

        public void Reset()
        {
            ResetHaptics(false);
            ResetLightBarColor(false);

            UpdateGamepad(false);
        }

        #region QuickSets 
        // TODO Need Data In protection
        #region Light Bar Color
        //----------------------------------------------------------------------------------------------------
        public override void SetLightBarColor(Color color) => SetLightBarColor(color, true);
        public void SetLightBarColor(Color color, bool updateGamepad = true)
        {
            DualSenseGamepadState gamepadState = new DualSenseGamepadState();
            gamepadState.LightBarColor = color;

            UpdateState(gamepadState, updateGamepad);
        }
        public void ResetLightBarColor(bool updateGamepad = true) => SetLightBarColor(Color.black, updateGamepad);

        //TODO add SetUserLight
        //TODO add SetMicLIght
        #endregion

        #region Haptics Feedbacks
        //----------------------------------------------------------------------------------------------------
        public override void PauseHaptics() => PauseHaptics(true);
        /// <summary>
        /// Pauses the haptic feedback on the controller at next UpdateGamepad() call 
        /// (or immediatly if parameter updateGamepad is left to true).
        /// </summary>
        /// <param name="updateGamepad">Should the gamepad be updated immediatly. If set to false 
        /// "UpdateGamepad()" should be called afterwards for the pause to take place and haptics 
        /// changes set between pause and update will not be saved</param>
        public void PauseHaptics(bool updateGamepad = true)
        {
            if (!UseLegacyHaptics)
                ShouldPauseHDHaptics?.Invoke(this);

            shouldPauseHapticAtNextUpdate = true;
            ResetHaptics(updateGamepad);
        }

        public override void ResumeHaptics() => ResumeHaptics(true);
        /// <summary>
        /// Resumes the haptics on the controller and reapply saved values to the state.
        /// Values are saved at each UpdateGamepad() call (even if haptics are paused).
        /// </summary>
        /// <param name="updateGamepad">Should the gamepad be updated immediatly. If set to false 
        /// "UpdateGamepad()" should be called afterwards to apply the resumed values</param>
        public void ResumeHaptics(bool updateGamepad = true)
        {
            IsHapticPaused = false;
            shouldPauseHapticAtNextUpdate = false;

            if (!UseLegacyHaptics)
                ShouldResumeHDHaptics?.Invoke(this);
            else if (m_motorSpeeds.HasValue)
                SetMotorSpeeds(m_motorSpeeds.Value, false);

            if (m_leftTriggerState.HasValue || m_rightTriggerState.HasValue)
                SetTriggerState(m_leftTriggerState ?? new DualSenseTriggerState(), m_rightTriggerState ?? new DualSenseTriggerState(), false);

            if (updateGamepad)
                UpdateGamepad(false);
        }

        public override void ResetHaptics() => ResetHaptics(true);
        /// <summary>
        /// Resets the haptics values back to zero.
        /// </summary>
        /// <param name="updateGamepad">Should the gamepad be updated immediatly. If set to false 
        /// "UpdateGamepad()" should be called afterwards to apply the reset values</param>
        public void ResetHaptics(bool updateGamepad = true)
        {
            if (!UseLegacyHaptics)
                ShouldResetHDHaptics?.Invoke(this);
            else
                ResetMotorSpeeds(false);

            ResetTriggersState(false);

            if (updateGamepad)
                UpdateGamepad(false);
        }

        #region Haptics : Legacy rumbles
        //----------------------------------------------------------------------------------------------------

        public override void SetMotorSpeeds(float lowFrequency, float highFrequency)
            => SetMotorSpeeds(new DualSenseMotorSpeed(lowFrequency, highFrequency), true);
        public void SetMotorSpeeds(float lowFrequency, float highFrequency, bool updateGamepad = true)
            => SetMotorSpeeds(new DualSenseMotorSpeed(lowFrequency, highFrequency), updateGamepad);
        /// <summary>
        /// Sets the emulated motors speeds value for legacy rumble haptics (this won't work if UseLegacyHaptics is set to false)
        /// </summary>
        /// <param name="motorSpeeds">the speed each of the low and high frequency motor should go (between 0 and 1)</param>
        /// <param name="updateGamepad">Should the gamepad be updated immediatly. If set to false 
        /// "UpdateGamepad()" should be called afterwards to apply the value</param>
        public void SetMotorSpeeds(DualSenseMotorSpeed motorSpeeds, bool updateGamepad = true)
        {
            DualSenseGamepadState gamepadState = new DualSenseGamepadState();
            gamepadState.Motor = motorSpeeds;

            UpdateState(gamepadState, updateGamepad);
        }

        public void ResetMotorSpeeds(bool updateGamepad = true) => SetMotorSpeeds(0f, 0f, updateGamepad);
        #endregion

        #region Haptics : Triggers force feedbacks
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the force feedback on the trigger of the gamepad
        /// </summary>
        /// <param name="leftTriggerState">Left trigger state to set, null value leave it unchanged</param>
        /// <param name="rightTriggerState">Right trigger state to set, null value leave it unchanged</param>
        /// <param name="updateGamepad">Should the gamepad be updated immediatly. If set to false 
        /// "UpdateGamepad()" should be called afterwards to apply the value</param>
        public void SetTriggerState(DualSenseTriggerState? leftTriggerState, DualSenseTriggerState? rightTriggerState, bool updateGamepad = true)
        {
            if (leftTriggerState is null && rightTriggerState is null)
                return;

            DualSenseGamepadState gamepadState = new DualSenseGamepadState();
            if (leftTriggerState.HasValue)
                gamepadState.LeftTrigger = leftTriggerState.Value;
            if (rightTriggerState.HasValue)
                gamepadState.RightTrigger = rightTriggerState.Value;

            UpdateState(gamepadState, updateGamepad);
        }

        public void ResetTriggersState(bool updateGamepad = true)
            => SetTriggerState(new DualSenseTriggerState(), new DualSenseTriggerState(), updateGamepad);
        #endregion

        #endregion

        #region Audio Volume
        public void SetAudioVolume(float volume, DualSenseTargetAudioDevice target = DualSenseTargetAudioDevice.InternalSpeaker , bool updateGamepad = true)
        {
            DualSenseGamepadState gamepadState = new DualSenseGamepadState();

            switch(target)
            {
                case DualSenseTargetAudioDevice.InternalSpeaker:
                    gamepadState.internalVolume = volume;
                    break;

                case DualSenseTargetAudioDevice.ExternalPluggedInDevice:
                    gamepadState.externalDeviceVolume = volume;
                    break;

                case DualSenseTargetAudioDevice.Both:
                    gamepadState.internalVolume = volume;
                    gamepadState.externalDeviceVolume = volume;
                    break;

                default:
                    Debug.LogError("Unknown value or not implemented!");
                    break;
            }
            Debug.Log("received " + volume + "  for " + target.ToString());

            UpdateState(gamepadState, updateGamepad);
        }
        public void ResetAudioVolume(bool updateGamepad = true) => SetAudioVolume(0, DualSenseTargetAudioDevice.Both, updateGamepad);
        #endregion

        #endregion

        bool CheckLegacyRumbleValidity(ref DualSenseMotorSpeed? motorSpeeds)
        {
            bool isInvalid = !UseLegacyHaptics && !motorSpeeds.Equals(new DualSenseMotorSpeed(0, 0));
            if (isInvalid)
            {
                Debug.LogWarning("The property \'UseLegacyHaptics\' is set to false but a motor speed was given : this value will be ignored !");
                motorSpeeds = null;
            }

            return !isInvalid;
        }

        /// <summary>
        /// Updates the currently modified gamepadState internally with the newer state. 
        /// This doesn't overwrite the previous state, it combines them considering the one given here as the newer one
        /// (to overwrite it you can access the NewState property directly).
        /// By default this doesn't update the gamepad in itself until UpdateGamepad() is called.
        /// </summary>
        /// <param name="state">The state values to add, if a field is left to null, it will not overwrite the corresponding one</param>
        /// <param name="updateGamepad">Should the gamepad be updated afterward (default is false)</param>
        public void UpdateState(DualSenseGamepadState state, bool updateGamepad = false)
        {
            NewState = Combine(NewState, state);
            if (updateGamepad)
                UpdateGamepad(false);
        }

        [Obsolete("This method is deprecated, please use UpdateState or set NewState parameter directly followed by UpdateGamepad if needed")]
        public void SetGamepadState(DualSenseGamepadState state)
        {
            NewState = state;
            UpdateGamepad(false);
        }

        /// <summary>
        /// Updates the gamepad outputs state accordingly to the actual value of the NewState property.
        /// </summary>
        /// <param name="forceUpdate">Should an update be sent even if the state has not been changed since last update. 
        /// Setting this parameter to true also ensure that the update sent is explicit (all values are explicitly set)</param>
        /// <returns>True if the update was succesfully send, false otherwise. This value for the last update is also set 
        /// to the UpdateSuccessed parameter if you need to check it later</returns>
        public bool UpdateGamepad(bool forceUpdate = false)
        {
            DualSenseHIDOutputReport outputReport;
            if (//(stateModified || forceUpdate || UseLegacyHaptics) //this is a basic equivalent of the tests perfomed in CreateOutputReport() without the full creation thanks to lazy evaluation
                 CreateOutputReport(CurrentGamepadState, NewState, out outputReport, !forceUpdate))
            {
                /*TODO suppre
                if (dirtyscript.instance != null )
                {
                    dirtyscript.instance.prevFlag1 = (byte)outputReport.flags1;
                    outputReport.flags1 = (DualSenseHIDOutputReport.Flags1)dirtyscript.instance.flag1;
                    dirtyscript.instance.prevFlag2 = (byte)outputReport.outputReportContent2;
                    outputReport.outputReportContent2 = (DualSenseHIDOutputReport.Flags2)dirtyscript.instance.flag2;

                    outputReport.externalVolume = dirtyscript.instance.externalVolume;
                    outputReport.internalVolume = dirtyscript.instance.internalVolume;
                    outputReport.micVolume = dirtyscript.instance.internalMicVolume;
                    outputReport.audioFlags = (DualSenseHIDOutputReport.AudioFlags)dirtyscript.instance.audioFlags;

                    outputReport.secondInternalVolume = dirtyscript.instance.secondInternalVolume;
                }
                //end suprre*/

                long executeResult = ExecuteCommand(ref outputReport);
                if (executeResult >= 0)
                {
                    CurrentGamepadState = Combine(CurrentGamepadState, NewState);
                    NewState = new DualSenseGamepadState();
                    stateModified = false;
                    UpdateSucceeded = true;
                    LastGamepadUpdateTime = Time.realtimeSinceStartup;
                }
                else
                {
                    Debug.LogWarning("Output report could not be send (error returned : " + executeResult + ").\n" +
                        "States values have been modified so they will be sent next update.\n" +
                        "This is often due to the latest update still being sent to the controller. " +
                        "If this message comes up a lot, you are probably calling UpdateGamepad too often. " +
                        "Think to check TimeSinceLastGamepadUpdate property if that is the case");
                    UpdateSucceeded = false;
                }
            }
            return UpdateSucceeded;
        }

        /// <summary>
        /// Computes the differences between the current state of the gamepad outputs 
        /// and the state wanted and creates an output report to be sent to the gamepad to update it accordingly.
        /// </summary>
        /// <param name="currentState">The current state of the gamepad outputs (as set by the lattest OutputReport sent to the gamepad).
        /// leave to null if unknown (similar to setting sendOnlyChangedValues to false)</param>
        /// <param name="newState">The new state the gamepad outputs should be updated to</param>
        /// <param name="outputReport">The resulting output report</param>
        /// <param name="sendOnlyChangedValues">Should the output report be only updating the gamepadOutputs value or should 
        /// all of them be set explicitly</param>
        /// <returns>true if the new report would change values on the controller (correspond to "Is it usefull to send it?").</returns>
        bool CreateOutputReport(DualSenseGamepadState? currentState, DualSenseGamepadState newState, out DualSenseHIDOutputReport outputReport, bool sendOnlyChangedValues = true)
        {
            outputReport = DualSenseHIDOutputReport.Create();
            if (!sendOnlyChangedValues && currentState.HasValue)
                newState = Combine(currentState.Value, newState);

            if (currentState == null)
                sendOnlyChangedValues = false;

            bool updatedValues = false;

            //Test for updated values between states and fill the outputReport if necessary 
            //TODO updates struct declaration to test equals with underlying values (default behaviour is checking references)
            //Lights and leds
            if (newState.LightBarColor.HasValue && (!sendOnlyChangedValues || !newState.LightBarColor.Value.Equals(currentState?.LightBarColor)))
            {
                outputReport.SetLightBarColor(newState.LightBarColor.Value);
                updatedValues = true;
            }
            if (newState.MicLed.HasValue && (!sendOnlyChangedValues || !newState.MicLed.Value.Equals(currentState?.MicLed)))
            {
                outputReport.SetMicMuteLedMode(newState.MicLed.Value);
                updatedValues = true;
            }
            if (newState.PlayerLed.HasValue && (!sendOnlyChangedValues || !newState.PlayerLed.Value.Equals(currentState?.PlayerLed)))
            {
                outputReport.SetPlayerLedState(newState.PlayerLed.Value);
                updatedValues = true;
            }
            if (newState.PlayerLedBrightness.HasValue && (!sendOnlyChangedValues || !newState.PlayerLedBrightness.Value.Equals(currentState?.PlayerLedBrightness)))
            {
                outputReport.SetPlayerLedBrightness(newState.PlayerLedBrightness.Value);
                updatedValues = true;
            }

            //Haptic feedbacks
            //Legacy rumbles different than 0 needs to be actualised every time (they will stop after a while otherwise)
            if (UseLegacyHaptics)
            {
                //TODO there is probably a better way to write this but its late and at least this is reasonably readable
                bool rumbleIsNewVal = newState.Motor.HasValue && (!newState.Motor.Value.Equals(new DualSenseMotorSpeed(0, 0)) || !newState.Motor.Value.Equals(m_motorSpeeds));
                bool oldValIsNonZero = currentState.HasValue && currentState.Value.Motor.HasValue && !currentState.Value.Motor.Equals(new DualSenseMotorSpeed(0, 0));

                DualSenseMotorSpeed? motorValue = rumbleIsNewVal ? newState.Motor : (oldValIsNonZero ? currentState.Value.Motor : null);

                if (motorValue.HasValue)
                {
                    if (!IsHapticPaused)
                    {
                        outputReport.SetMotorSpeeds(motorValue.Value.LowFrequencyMotorSpeed, motorValue.Value.HighFrequencyMotorSpeed);
                        updatedValues = true;
                    }
                    if (!shouldPauseHapticAtNextUpdate)
                    {
                        m_motorSpeeds = motorValue;
                    }
                }
            }
            if (newState.LeftTrigger.HasValue && (!sendOnlyChangedValues || !newState.LeftTrigger.Equals(currentState?.LeftTrigger)))
            {
                if (!IsHapticPaused)
                {
                    outputReport.SetLeftTriggerState(newState.LeftTrigger.Value);
                    updatedValues = true;
                }
                if (!shouldPauseHapticAtNextUpdate)
                    m_leftTriggerState = newState.LeftTrigger.Value;
            }
            if (newState.RightTrigger.HasValue && (!sendOnlyChangedValues || !newState.RightTrigger.Equals(currentState?.RightTrigger)))
            {
                if (!IsHapticPaused)
                {
                    outputReport.SetRightTriggerState(newState.RightTrigger.Value);
                    updatedValues = true;
                }
                if (!shouldPauseHapticAtNextUpdate)
                    m_rightTriggerState = newState.RightTrigger.Value;
            }

            if (newState.internalVolume.HasValue && (!sendOnlyChangedValues || !newState.internalVolume.Equals(currentState?.internalVolume)))
            {
                outputReport.SetInternalVolume(newState.internalVolume.Value);
                updatedValues = true;
            }
            if (newState.externalDeviceVolume.HasValue && (!sendOnlyChangedValues || !newState.externalDeviceVolume.Equals(currentState?.externalDeviceVolume)))
            {
                outputReport.SetExternalDeviceVolume(newState.externalDeviceVolume.Value);
                updatedValues = true;
            }


                if (shouldPauseHapticAtNextUpdate)
            {
                IsHapticPaused = true;
                shouldPauseHapticAtNextUpdate = false;
            }

            return true;// updatedValues;
        }

        /// <summary>
        /// Combines two states into one by overwritting baseState values with values that are non-null in newState
        /// </summary>
        /// <param name="baseState">The state to use as a base for the combination, some of the contained value might get overwritten</param>
        /// <param name="newState">The state which defined values are overwritten on the baseState to create the combination</param>
        /// <returns>The combination of the two states</returns>
        DualSenseGamepadState Combine(DualSenseGamepadState baseState, DualSenseGamepadState newState)
        {
            FieldInfo[] fields = typeof(DualSenseGamepadState).GetFields();

            object boxedState = baseState;

            for (int i = 0; i < fields.Length; i++)
            {
                //Debug.Log(i + " actual value : " + fields[i].GetValue(boxedState));
                //Debug.Log(i + "    new value : " + fields[i].GetValue(newState));

                if (fields[i].GetValue(newState) != null)
                {
                    fields[i].SetValue(boxedState, fields[i].GetValue(newState));
                }
                //Debug.Log(i + "  newer value : " + fields[i].GetValue(boxedState));
            }

            return (DualSenseGamepadState)boxedState;
        }
        #endregion
    }
}