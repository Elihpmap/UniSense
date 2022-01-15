using System;
using System.Collections.Specialized;
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


        [FieldOffset(0)] public InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 0)] public byte reportId;

        //Globals flags
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 1)] public OutputReportContent outputReportContent;
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
                set { outputReportContent = (UInt16)value; }
            }
            public ContentFlags contentFlags
            {
                get { return (ContentFlags)(outputReportContent & 0b_1111_1111_1111_1100); }
                set { outputReportContent = (UInt16)value; }
            }
        }

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 3)] public byte lowFrequencyMotorSpeed; // 0-255, Emulated by the Right VoiceCoil
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 4)] public byte highFrequencyMotorSpeed; // 0-255, Emulated by the Left VoiceCoil

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 5)] public byte externalVolume; // volume of external device plugged in the controller jack
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 6)] public byte internalVolume; // volume of internal speaker of the controller
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 7)] public byte micVolume; // (internal mic only?) microphone volume (not linear, maxes out at 0x40, 0x00 is not fully muted);
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 8)] public AudioControl audioControl; // SpeakerCompPreGain also present at + 38 
        [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 1)] internal struct AudioControl
        {
            [FieldOffset(0)] public byte audioControl;

            public enum MicSelect : byte //2 lower bits (0b_000_00xx)
            {
                Auto = 0x00,
                InternalMic = 0x01,     // force use of internal controller mic (if neither 0x01 and 0x02 are set, an attached headset will take precedence)
                ExternalMic = 0x02,     // force use of mic attached to the controller (headset) (if neither 0x01 and 0x02 are set, an attached headset will take precedence
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
                set { audioControl = (byte) value; }
            }
            public MicEffect micControl
            {
                get { return (MicEffect)(audioControl & 0b_0000_1100); }
                set { audioControl = (byte) value; }
            }
            public OutputPathSelect outputPathSelect
            { 
                get { return (OutputPathSelect) (audioControl & 0b_0011_0000); }
                set { audioControl = (byte) value;}
            }
            public InputPathSelect inputPathSelect
            {
                get { return (InputPathSelect)(audioControl & 0b_1100_0000); }
                set { audioControl = (byte) value; }
            }
        }
        
        

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 9)] public MicMuteLedMode micMuteLedMode;
        internal enum MicMuteLedMode : byte
        {
            Off = 0x00,
            On = 0x01,
            Breathing = 0x02,
        }

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 10)] public MuteControl muteControl;
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

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 11)] public byte rightTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 12)] public fixed byte rightTriggerParams[kTriggerParamSize];
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 22)] public byte leftTriggerMode;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 23)] public fixed byte leftTriggerParams[kTriggerParamSize];
        
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 37)] public byte powerReduction; // (lower nibble: main motor; upper nibble trigger effects) 0x00 to 0x07 - reduce overall power of the respective motors/effects by 12.5% per increment (this does not affect the regular trigger motor settings, just the automatically repeating trigger effects)

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 38)] public byte audioControl2; // 3 lower bits are SpeakerCompPreGain : additional speaker volume boost. The other bits actions are still unknown.

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 39)] public LedFlags ledFlags;
        [Flags] internal enum LedFlags : byte
        {
            PlayerLedBrightness = 0x01,
            LightBarFade = 0x02,
        }
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 42)] public LedFadeAnimation ledFadeAnimation;
        internal enum LedFadeAnimation : byte
        {
            None = 0x00,
            FadeIn = 0x01, // from black to blue
            FadeOut = 0x02 // from blue to black 
        }
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 43)] public PlayerLedBrightness playerLedBrightness; 
        internal enum PlayerLedBrightness : byte
        {
            High = 0x0,
            Medium = 0x1,
            Low = 0x2,
        }
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 44)] public byte playerLedState;

        [FieldOffset(InputDeviceCommand.BaseCommandSize + 45)] public byte lightBarRed;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 46)] public byte lightBarGreen;
        [FieldOffset(InputDeviceCommand.BaseCommandSize + 47)] public byte lightBarBlue;

        public FourCC typeStatic => Type;

        public void SetMotorSpeeds(float lowFreq, float highFreq)
        {
            outputReportContent.rumbleContent = OutputReportContent.RumbleContent.NewEmulatedRumbles;
            lowFrequencyMotorSpeed = (byte)Mathf.Clamp(lowFreq * 255, 0, 255);
            highFrequencyMotorSpeed = (byte)Mathf.Clamp(highFreq * 255, 0, 255);
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

        /// <summary>
        /// not working yet, I couldn't figure out which flags and value would work to enable this,
        /// the only thing that work for now is that it stop audio to external device when set 
        /// (until SetExternalDeviceVolume is called again)
        /// </summary>
        /// <param name="volume"></param>
        public void SetInternalVolume(float volume)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowInternalVolume;
            //AudioControl.audioFlags |= AudioFlags.enableInternalSpeaker;
            internalVolume = (byte)Mathf.Clamp(volume * 255, 0, 255);
            audioControl2 = (byte)Mathf.Clamp(volume * 0, 0, 7);
        }

        public void SetExternalDeviceVolume(float volume)
        {
            outputReportContent.contentFlags |= OutputReportContent.ContentFlags.AllowExternalVolume;
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