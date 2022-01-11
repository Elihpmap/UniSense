using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UniSense.LowLevel
{
    // helping source : https://gist.github.com/stealth-alex/10a8e7cc6027b78fa18a7f48a0d3d1e4

    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    internal unsafe struct DualSenseHIDOutputReport : IInputDeviceCommandInfo
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');

        internal const int kSize = InputDeviceCommand.BaseCommandSize + 48;
        internal const int kTriggerParamSize = 9;
        internal const int kReportId = 2;

        [Flags]
        internal enum Flags1 : byte
        {
            MainMotors1 = 0x01,
            MainMotors2 = 0x02,
            RightTrigger = 0x04,
            LeftTrigger = 0x08,
            AudioVolume = 0x10,
            InternalSpeaker = 0x20,   // 0x20 toggling of internal speaker while headset is connected
            //MicVolume = 0x40,         // 0x40 modification of microphone volume
            //InternalMic = 0x80        // 0x80 toggling of internal mic or external speaker while headset is connected
        }

        [Flags]
        internal enum Flags2 : byte
        {
            MicLed = 0x01,
            SetLightBarColor = 0x04,
            PlayerLed = 0x10,
        }

        [Flags]
        internal enum AudioFlags : byte
        {
            // 0x01 = force use of internal controller mic (if neither 0x01 and 0x02 are set, an attached headset will take precedence)
            // 0x02 = force use of mic attached to the controller (headset)
            // 0x04 = pads left channel of external mic (~1/3rd of the volume? maybe the amount can be controlled?)
            // 0x08 = pads left channel of internal mic (~1/3rd of the volume? maybe the amount can be controlled?)
            disableExternalDevice = 0x10,   // 0x10 = disable attached headphones (only if 0x20 to enable internal speakers is provided as well)
            enableInternalSpeaker = 0x20    // 0x20 = enable audio on internal speaker (in addition to a connected headset; headset will use a stereo upmix of the left channel, internal speaker will play the right channel)
        }

        internal enum InternalMicLedState : byte
        {
            Off = 0x00,
            On = 0x01,
            Pulsating = 0x02,
        }

        [Flags]
        internal enum LedFlags : byte
        {
            PlayerLedBrightness = 0x01,
            LightBarFade        = 0x02,
        }

        internal enum InternalPlayerLedBrightness : byte
        {
            High = 0x0,
            Medium = 0x1,
            Low = 0x2,
        }

        [FieldOffset(0)] public InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public Flags1 flags1;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public Flags2 flags2;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte lowFrequencyMotorSpeed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte highFrequencyMotorSpeed;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public byte externalVolume; // volume of external device plugged in the controller jack
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 6)] public byte internalVolume; // volume of internal speaker of the controller
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 7)] public byte internalMicVolume; // internal microphone volume (not at all linear; 0-255, maxes out at 0x40, all values above are treated like 0x40;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 8)] public AudioFlags audioFlags;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 9)] public InternalMicLedState micLedState;
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 11)] public byte rightTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 12)] public fixed byte rightTriggerParams[kTriggerParamSize];
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 22)] public byte leftTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 23)] public fixed byte leftTriggerParams[kTriggerParamSize];
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 37)] public byte powerReduction;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 37)] public byte secondInternalVolume;
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 39)] public LedFlags ledFlags;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 42)] public byte ledPulseOption;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 43)] public InternalPlayerLedBrightness playerLedBrightness;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 44)] public byte playerLedState;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 45)] public byte lightBarRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 46)] public byte lightBarGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 47)] public byte lightBarBlue;

        public FourCC typeStatic => Type;

        public void SetMotorSpeeds(float lowFreq, float highFreq)
        {
            flags1 |= Flags1.MainMotors1 | Flags1.MainMotors2;
            lowFrequencyMotorSpeed = (byte)Mathf.Clamp(lowFreq * 255, 0, 255);
            highFrequencyMotorSpeed = (byte)Mathf.Clamp(highFreq * 255, 0, 255);
        }

        public void ResetMotorSpeeds(bool resetImmediately = false)
        {
            if (resetImmediately)
            {
                flags1 &= ~(Flags1.MainMotors1 | Flags1.MainMotors2);
            }
            else
            {
                flags1 &= ~Flags1.MainMotors2;
            }
        }

        public void SetMicLedState(DualSenseMicLedState state)
        {
            flags2 |= Flags2.MicLed;
            switch (state)
            {
                case DualSenseMicLedState.Off:
                    micLedState = InternalMicLedState.Off;
                    break;
                case DualSenseMicLedState.On:
                    micLedState = InternalMicLedState.On;
                    break;
                case DualSenseMicLedState.Pulsating:
                    micLedState = InternalMicLedState.Pulsating;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        public void SetLeftTriggerState(DualSenseTriggerState state)
        {
            flags1 |= Flags1.LeftTrigger;
            fixed (byte* p = leftTriggerParams)
            {
                SetTriggerState(state, ref leftTriggerMode, p);
            }
        }

        public void SetRightTriggerState(DualSenseTriggerState state)
        {
            flags1 |= Flags1.RightTrigger;
            fixed (byte* p = rightTriggerParams)
            {
                SetTriggerState(state, ref rightTriggerMode, p);
            }
        }

        private void SetTriggerState(DualSenseTriggerState state, ref byte triggerMode, byte* triggerParams)
        {

            triggerMode = (byte)state.EffectType;
            ClearTriggerParams(triggerParams);

            // TODO : a better way must exist (especially since they all implement the same EffectParameter
            // interface and their data is stored at the same adress thanks to [FieldOffset(1)])
            switch (state.EffectType)
            {
                case DualSenseTriggerEffectType.NoResistance:
                case DualSenseTriggerEffectType.ResetResistance:
                    break;

                case DualSenseTriggerEffectType.ContinuousResistance:
                    ((EffectParameters)state.ContinuousResistance).GetFormatedParameters(triggerParams);
                    break;
                case DualSenseTriggerEffectType.SectionResistance:
                    ((EffectParameters)state.SectionResistance).GetFormatedParameters(triggerParams);
                    break;
                case DualSenseTriggerEffectType.VibratingResistance:
                    ((EffectParameters)state.VibratingResistance).GetFormatedParameters(triggerParams);
                    break;

                case DualSenseTriggerEffectType.Crunch:
                    ((EffectParameters)state.Crunch).GetFormatedParameters(triggerParams);
                    break;
                case DualSenseTriggerEffectType.SnapBack:
                    ((EffectParameters)state.SnapBack).GetFormatedParameters(triggerParams);
                    break;
                case DualSenseTriggerEffectType.EffectEx:
                    ((EffectParameters)state.EffectEx).GetFormatedParameters(triggerParams);
                    break;
                case DualSenseTriggerEffectType.AmplitudeVibration:
                    ((EffectParameters)state.AmplitudeVibration).GetFormatedParameters(triggerParams);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        private void ClearTriggerParams(byte* triggerParams)
        {
            for (int i = 0; i < kTriggerParamSize; i++)
            {
                triggerParams[i] = 0;
            }
        }

        public void SetLightBarColor(Color color)
        {
            flags2 |= Flags2.SetLightBarColor;
            ledFlags |= LedFlags.LightBarFade;
            ledPulseOption = 0x02;
            lightBarRed = (byte) Mathf.Clamp(color.r * 255, 0, 255);
            lightBarGreen = (byte) Mathf.Clamp(color.g * 255, 0, 255);
            lightBarBlue = (byte) Mathf.Clamp(color.b * 255, 0, 255);
        }

        public void SetPlayerLedBrightness(PlayerLedBrightness brightness)
        {
            ledFlags |= LedFlags.PlayerLedBrightness;
            ledPulseOption = 0x02;
            switch (brightness)
            {
                case PlayerLedBrightness.High:
                    playerLedBrightness = InternalPlayerLedBrightness.High;
                    break;
                case PlayerLedBrightness.Medium:
                    playerLedBrightness = InternalPlayerLedBrightness.Medium;
                    break;
                case PlayerLedBrightness.Low:
                    playerLedBrightness = InternalPlayerLedBrightness.Low;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(brightness), brightness, null);
            }
        }

        public void SetPlayerLedState(PlayerLedState state)
        {
            flags2 |= Flags2.PlayerLed;
            ledPulseOption = 0x02;
            playerLedState = state.Value;
        }

        public void DisableLightBarAndPlayerLed()
        {
            ledFlags |= LedFlags.LightBarFade;
            ledPulseOption = 0x01;
        }

        /// <summary>
        /// not working yet, I couldn't figure out which flags and value would work to enable this,
        /// the only thing that work for now is that it stop audio to external device when set 
        /// (until SetExternalDeviceVolume is called again)
        /// </summary>
        /// <param name="volume"></param>
        public void SetInternalVolume(float volume)
        {
            flags1 |= Flags1.AudioVolume | Flags1.InternalSpeaker;
            audioFlags |= AudioFlags.enableInternalSpeaker;
            internalVolume = (byte)Mathf.Clamp(volume * 255, 0, 255);
            secondInternalVolume = (byte)Mathf.Clamp(volume * 0, 0, 7);
        }

        public void SetExternalDeviceVolume(float volume)
        {
            flags1 |= Flags1.AudioVolume;
            externalVolume = (byte)Mathf.Clamp(volume * 255, 0, 255);
        }

        public static DualSenseHIDOutputReport Create()
        {
            return new DualSenseHIDOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
                reportId = kReportId,
            };
        }
    }
}