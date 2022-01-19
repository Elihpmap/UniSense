﻿using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UniSense.LowLevel
{
    // helping sources : https://controllers.fandom.com/wiki/Sony_DualSense
    // https://gist.github.com/stealth-alex/10a8e7cc6027b78fa18a7f48a0d3d1e4

    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    internal unsafe struct DualSenseHIDOutputReport : IInputDeviceCommandInfo
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');
        public FourCC typeStatic => Type;

        internal const int kSize = InputDeviceCommand.BaseCommandSize + 48;
        internal const int kTriggerParamSize = 9;
        internal const int kReportId = 2;

        [FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 00)] public byte reportId;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 01)] public OutputReportContent outputReportContent;
                                                        //02 is the end of outputReportContent (because OutputReportContent is Uint16 so 2 byte long)
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 03)] public byte rumbleEmulationLowRight; // 0-255, Emulated Low frequency rumbles by the Right VoiceCoil
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 04)] public byte rumbleEmulationHighLeft; // 0-255, Emulated High frequency rumbles by the Left VoiceCoil
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 05)] public byte externalVolume; // volume of external device plugged in the controller jack (max 0x7f = 127)
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 06)] public byte internalVolume; // volume of internal speaker of the controller (PS5 appears to only use the range 0x3d-0x64 = 61-100)
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 07)] public byte micVolume; // (internal mic only?) microphone volume (not linear, maxes out at 0x40 = 64, 0x00 is not fully muted);
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 08)] public AudioControl audioControl; // SpeakerCompPreGain also present at [FieldOffset(K + 38)]
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 09)] public MicMuteLedMode micMuteLedMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 10)] public MuteControl muteControl;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 11)] public byte rightTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 12)] public fixed byte rightTriggerParams[kTriggerParamSize];
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 22)] public byte leftTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 23)] public fixed byte leftTriggerParams[kTriggerParamSize];
                                                        //33 1st byte of 4 of data mirrored back in the InputReport
                                                        //34 2nd byte of 4 of data mirrored back in the InputReport
                                                        //35 3rd byte of 4 of data mirrored back in the InputReport
                                                        //36 4th byte of 4 of data mirrored back in the InputReport
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 37)] public byte powerReduction; // (lower nibble: main motor; upper nibble trigger effects) 0x00 to 0x07 - reduce overall power of the respective motors/effects by 12.5% per increment (this does not affect the regular trigger motor settings, just the automatically repeating trigger effects)
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 38)] public byte audioControl2; // 3 lower bits are SpeakerCompPreGain : additional speaker volume boost. The other bits actions are still unknown.
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 39)] public LedFlags ledFlags; // only defined on the two lower bits the rest is unused
                                                        //40 Unknown
                                                        //41 Unknown
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 42)] public LedFadeAnimation ledFadeAnimation;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 43)] public PlayerLedBrightness playerLedBrightness;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 44)] public byte playerLedState;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 45)] public byte lightBarRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 46)] public byte lightBarGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 47)] public byte lightBarBlue;

        #region Flags and structs for data formats
        [StructLayout(LayoutKind.Explicit, Size = 2)] internal struct OutputReportContent
        {
            [FieldOffset(0)] public UInt16 outputReportContent;
            public enum RumbleContent : UInt16 // 2 lower bits (0b_0000_0000_0000_00xx)
            {
                AudioHaptics = 0x0000,            // Stops emulated rumbles and switch back instantly to audio haptics
                FadeBackToAudioHaptics = 0x0001,  // Allow rumble to gracefully terminate and then re-enable audio haptics
                EmulatedRumbles = 0x0002,         // Emulated rumbles are allowed to time out without re-enabling audio haptics
                NewEmulatedRumbles = 0x0003,      // Allow to set new values for lowFrequencyMotorSpeed and highFrequencyMotorSpeed
            }
            [Flags] public enum ContentFlags : UInt16 // 8+6 higher bits (0b_xxxx_xxxx_xxxx_xx00)
            {
                AllowRightTriggerFFB = 0x0004,  // Enable setting RightTriggerFFB section
                AllowLeftTriggerFFB = 0x0008,   // Enable setting LeftTriggerFFB section
                AllowExternalVolume = 0x0010,   // Enable setting externalVolume
                AllowInternalVolume = 0x0020,   // Enable setting internalVolume
                AllowMicVolume = 0x0040,        // Enable setting micVolume
                AllowAudioControl = 0x0080,     // Enable setting AudioControl section

                AllowMicMuteLedMode = 0x0100,   // Enable setting micMuteLedMode
                AllowMuteControl = 0x0200,      // Enable setting MuteControl section
                AllowLightBarColor = 0x0400,    // Enable setting LightBarColor section
                //ResetLights = 0x0800,           // Release the LEDs from Wireless firmware control see Wiki for more info
                AllowPlayerLed = 0x1000,        // Enable setting PlayerLedIndicators section
                //Unknown = 0x2000,               // 
                AllowPowerReduction = 0x4000,   // Enable setting powerReduction
                AllowAudioControl2 = 0x8000     // Enable setting AudioControl2 section
            }

            public RumbleContent rumbleContent
            {
                get { return (RumbleContent)(outputReportContent & 0b_0000_0000_0000_0011); }
                set { outputReportContent = (UInt16)((outputReportContent & 0b_1111_1111_1111_1100) | (UInt16)value); }
            }
            public ContentFlags contentFlags
            {
                get { return (ContentFlags)(outputReportContent & 0b_1111_1111_1111_1100); }
                set { outputReportContent = (UInt16)((outputReportContent & 0b_0000_0000_0000_0011) | (UInt16)value); }
            }
        }
        [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 1)] internal struct AudioControl
        {
            [FieldOffset(0)] public byte audioControl;

            public enum MicSelect : byte //2 lower bits (0b_000_00xx)
            {
                Auto = 0x00,            // an attached headset will take precedence, otherwise the internal one is used
                InternalMic = 0x01,     // force use of internal controller mic
                ExternalMic = 0x02,     // force use of mic attached to the controller (headset) 
            }
            [Flags] public enum MicEffect : byte // 2 higher bits of the lower nible (0b_0000_xx00)
            {
                EchoCancelEnable = 0x04,
                NoiseCancelEnable = 0x08,
            }
            public enum OutputPathSelect : byte // 2 lower bits of the higher nible (0b_00xx_0000)
            {
                L_R_X = 0x00, // headphones has Left and Right channel, nothing on internal speaker
                L_L_X = 0x10, // headphones has stereo upmix of Left channel, nothing on internal speaker
                L_L_R = 0x20, // headphones has stereo upmix of Left channel, internal speaker has Right channel
                X_X_R = 0x30  // nothing on headphones, internal speaker has Right channel
            }
            public enum InputPathSelect : byte // 2 higher bits (0b_xx00_0000)
            { //TODO what's that ?
                CHAT_ASR = 0x00,
                CHAT_CHAT = 0x40,
                ASR_ASR = 0x80,
                //InvalidValue = 0xc0  // Does Nothing, invalid
            }

            public MicSelect micSelect
            {
                get { return (MicSelect)(audioControl & 0b_0000_0011); }
                set { audioControl = (byte)((audioControl & 0b_1111_1100) | (byte)value); }
            }
            public MicEffect micEffect
            {
                get { return (MicEffect)(audioControl & 0b_0000_1100); }
                set { audioControl = (byte)((audioControl & 0b_1111_0011) | (byte)value); }
            }
            public OutputPathSelect outputPathSelect
            { 
                get { return (OutputPathSelect) (audioControl & 0b_0011_0000); }
                set { audioControl = (byte)((audioControl & 0b_1100_1111) | (byte)value); }
            }
            public InputPathSelect inputPathSelect
            {
                get { return (InputPathSelect)(audioControl & 0b_1100_0000); }
                set { audioControl = (byte)((audioControl & 0b_0011_1111) | (byte)value); }
            }
        }
        internal enum MicMuteLedMode : byte
        {
            Off = 0x00,
            On = 0x01,
            Breathing = 0x02,
        }
        [Flags] internal enum MuteControl : byte
        {
            TouchPowerSave = 0x01,
            MotionPowerSave = 0x02,
            TriggersPowerSave = 0x04,
            AudioPowerSave = 0x08,
            MicMute = 0x10,
            SpeakerMute = 0x20,
            HeadphoneMute = 0X40,
            TriggersMute = 0x80
        }
        [Flags] internal enum LedFlags : byte
        {
            PlayerLedBrightness = 0x01,
            LightBarFade = 0x02,
        }
        internal enum LedFadeAnimation : byte
        {
            None = 0x00,
            FadeIn = 0x01,
            FadeOut = 0x02
        }
        internal enum PlayerLedBrightness : byte
        {
            High = 0x0,
            Medium = 0x1,
            Low = 0x2,
        }
        #endregion


        public static DualSenseHIDOutputReport Create()
        {
            return new DualSenseHIDOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
                reportId = kReportId,
            };
        }

        #region Data formation methods

        #region Haptics
        #region Rumbles
        public void SetMotorSpeeds(float lowFreq, float highFreq)
        {
            outputReportContent.rumbleContent = OutputReportContent.RumbleContent.NewEmulatedRumbles;
            rumbleEmulationLowRight = (byte)Mathf.Clamp(lowFreq * 255, 0, 255);
            rumbleEmulationHighLeft = (byte)Mathf.Clamp(highFreq * 255, 0, 255);
        }

        public void ResetMotorSpeeds(bool resetImmediately = false)
        {
            if (resetImmediately)
            {
                outputReportContent.rumbleContent = OutputReportContent.RumbleContent.AudioHaptics;
            }
            else
            {
                outputReportContent.rumbleContent = OutputReportContent.RumbleContent.FadeBackToAudioHaptics;
            }
        }
        #endregion
        #region Trigger Force Feedback
        public void SetLeftTriggerState(DualSenseTriggerState state)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowLeftTriggerFFB;
            fixed (byte* p = leftTriggerParams)
            {
                SetTriggerState(state, ref leftTriggerMode, p);
            }
        }

        public void SetRightTriggerState(DualSenseTriggerState state)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowRightTriggerFFB;
            fixed (byte* p = rightTriggerParams)
            {
                SetTriggerState(state, ref rightTriggerMode, p);
            }
        }

        private void SetTriggerState(DualSenseTriggerState state, ref byte triggerMode, byte* triggerParams)
        {
            triggerMode = (byte)state.EffectType;
            ClearTriggerParams(triggerParams);

            if (state.EffectType != DualSenseTriggerEffectType.NoResistance
                && state.EffectType != DualSenseTriggerEffectType.ResetResistance)
            {
                FieldInfo effectParameters = state.GetType().GetField(state.EffectType.ToString());
                ((EffectParameters)effectParameters.GetValue(state)).GetFormatedParameters(triggerParams);
            }
        }

        private void ClearTriggerParams(byte* triggerParams)
        {
            for (int i = 0; i < kTriggerParamSize; i++)
            {
                triggerParams[i] = 0;
            }
        }
        #endregion
        #endregion

        #region Lights and Leds
        public void SetMicMuteLedMode(DualSenseMicLedState state)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowMicMuteLedMode;
            switch (state)
            {
                case DualSenseMicLedState.Off:
                    micMuteLedMode = MicMuteLedMode.Off;
                    break;
                case DualSenseMicLedState.On:
                    micMuteLedMode = MicMuteLedMode.On;
                    break;
                case DualSenseMicLedState.Pulsating:
                    micMuteLedMode = MicMuteLedMode.Breathing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void SetLightBarColor(Color color)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowLightBarColor;
            ledFlags |= LedFlags.LightBarFade;
            ledFadeAnimation = LedFadeAnimation.FadeOut;
            lightBarRed = (byte) Mathf.Clamp(color.r * 255, 0, 255);
            lightBarGreen = (byte) Mathf.Clamp(color.g * 255, 0, 255);
            lightBarBlue = (byte) Mathf.Clamp(color.b * 255, 0, 255);
        }

        public void SetPlayerLedBrightness(UniSense.PlayerLedBrightness brightness)
        {
            ledFlags |= LedFlags.PlayerLedBrightness;
            ledFadeAnimation = LedFadeAnimation.FadeOut;
            switch (brightness)
            {
                case UniSense.PlayerLedBrightness.High:
                    playerLedBrightness = PlayerLedBrightness.High;
                    break;
                case UniSense.PlayerLedBrightness.Medium:
                    playerLedBrightness = PlayerLedBrightness.Medium;
                    break;
                case UniSense.PlayerLedBrightness.Low:
                    playerLedBrightness = PlayerLedBrightness.Low;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(brightness), brightness, null);
            }
        }

        public void SetPlayerLedState(PlayerLedState state)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowPlayerLed;
            ledFadeAnimation = LedFadeAnimation.FadeOut;
            playerLedState = state.Value;
        }

        public void DisableLightBarAndPlayerLed()
        {
            ledFlags |= LedFlags.LightBarFade;
            ledFadeAnimation = LedFadeAnimation.FadeIn;
        }
        #endregion

        #region Audio Control
        public void SetInternalVolume(float volume)
        {
            // TODO some of these are probably not needed each time, a cleanup might be good;

            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowInternalVolume
                                            | OutputReportContent.ContentFlags.AllowAudioControl
                                            | OutputReportContent.ContentFlags.AllowAudioControl2;

            internalVolume = (byte)Mathf.Clamp(volume * 255, 0, 255);

            audioControl.micSelect = AudioControl.MicSelect.Auto;
            audioControl.micEffect = AudioControl.MicEffect.EchoCancelEnable | AudioControl.MicEffect.NoiseCancelEnable;
            audioControl.outputPathSelect = AudioControl.OutputPathSelect.X_X_R;
            audioControl.inputPathSelect = AudioControl.InputPathSelect.CHAT_ASR;

            audioControl2 = 0x05;
        }

        public void SetExternalDeviceVolume(float volume)
        {
            // TODO some of these are probably not needed each time, being able to set them separatly might be good;

            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowExternalVolume
                                            | OutputReportContent.ContentFlags.AllowAudioControl;

            audioControl.micSelect = AudioControl.MicSelect.Auto;
            audioControl.micEffect = AudioControl.MicEffect.EchoCancelEnable | AudioControl.MicEffect.NoiseCancelEnable;
            audioControl.outputPathSelect = AudioControl.OutputPathSelect.L_R_X;
            audioControl.inputPathSelect = AudioControl.InputPathSelect.CHAT_ASR;

            externalVolume = (byte)Mathf.Clamp(volume * 255, 0, 255);
        }
        #endregion

        #endregion
    }
}