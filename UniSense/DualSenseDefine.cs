using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UniSense
{
    /// <summary>
    /// A struct containing nullables values describing a gamepad state. A null value means 
    /// "no changes since last state for this parameter"
    /// </summary>
    public struct DualSenseGamepadState
    {
        //TODO implement deep copy and deep Equals() comparison to detect and reproduce changes between states
        //TODO implement custom editor to display gamepadStates in the inspector

        public DualSenseTriggerState? RightTrigger;
        public DualSenseTriggerState? LeftTrigger;

        public bool? UseLegacyRumbles;
        public DualSenseMotorSpeed? Motor;

        public Color? LightBarColor;
        public DualSenseMicLedState? MicLed;
        public PlayerLedBrightness? PlayerLedBrightness;
        public PlayerLedState? PlayerLed;

        public float? internalVolume;
        public float? externalDeviceVolume;
    }

    public struct DualSenseMotorSpeed
    {
        public float LowFrequencyMotorSpeed;
        public float HighFrequencyMotorSpeed;

        public DualSenseMotorSpeed(float lowFrequencyMotorSpeed, float highFrequenceyMotorSpeed)
        {
            LowFrequencyMotorSpeed = lowFrequencyMotorSpeed;
            HighFrequencyMotorSpeed = highFrequenceyMotorSpeed;
        }

        public override bool Equals(object obj)
        {
            return obj is DualSenseMotorSpeed speed &&
                   LowFrequencyMotorSpeed == speed.LowFrequencyMotorSpeed &&
                   HighFrequencyMotorSpeed == speed.HighFrequencyMotorSpeed;
        }

        public override int GetHashCode()
        {
            int hashCode = -344924298;
            hashCode = hashCode * -1521134295 + LowFrequencyMotorSpeed.GetHashCode();
            hashCode = hashCode * -1521134295 + HighFrequencyMotorSpeed.GetHashCode();
            return hashCode;
        }
    }

    public enum DualSenseTargetAudioDevice
    {
        InternalSpeaker,
        ExternalPluggedInDevice,
        Both
    }

    public enum DualSenseMicLedState
    {
        Off,
        On,
        Pulsating,
    }

    /// <summary>
    /// A struct to hold trigger state information : this struct is axed toward memory save, 
    /// to have an inspector displayable version use <see cref="DualSenseSerializableTriggerState"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct DualSenseTriggerState
    {
        [FieldOffset(0)]
        public DualSenseTriggerEffectType EffectType;

        [FieldOffset(1)] 
        public DualSenseContinuousResistanceProperties ContinuousResistance;
        [FieldOffset(1)] 
        public DualSenseSectionResistanceProperties SectionResistance;
        [FieldOffset(1)] 
        public DualSenseVibratingResistanceProperties VibratingResistance;
        [FieldOffset(1)]
        public DualSenseCrunchProperties Crunch;
        [FieldOffset(1)]
        public DualSenseSnapBackProperties SnapBack;
        [FieldOffset(1)]
        public DualSenseEffectExProperties EffectEx;
        [FieldOffset(1)]
        public DualSenseAmplitudeVibrationProperties AmplitudeVibration;


        public DualSenseTriggerState(DualSenseTriggerState triggerState) : this()
        {
            this = triggerState;
        }
        public DualSenseTriggerState(DualSenseSerializableTriggerState triggerState) : this()
        {
            this.EffectType = triggerState.EffectType;

            if (EffectType != DualSenseTriggerEffectType.NoResistance
                && EffectType != DualSenseTriggerEffectType.ResetResistance)
            {
                FieldInfo thisEffectParameters = typeof(DualSenseTriggerState).GetField(EffectType.ToString());
                FieldInfo otherEffectParameters = typeof(DualSenseSerializableTriggerState).GetField(EffectType.ToString());
                object BoxedTriggerState = this; //need boxing otherwise 'this' is a value and is copied, not refered to SetValue()
                thisEffectParameters.SetValue(BoxedTriggerState, otherEffectParameters.GetValue(triggerState));
                this = (DualSenseTriggerState)BoxedTriggerState;
            }

            this = new DualSenseTriggerState(this);
        }
        public static implicit operator DualSenseTriggerState(DualSenseSerializableTriggerState TS) =>
           new DualSenseTriggerState(TS);
    }

    /// <summary>
    /// An interface to be implemented by the different structs detailing trigger effect parameter 
    /// to provide a common way to format them to be sent to the gamepad via the HIDOutputReport
    /// </summary>
    public unsafe interface EffectParameters
    {
        void GetFormatedParameters(byte* triggerParams);
    }

    #region DualSenseTriggerState and DualSenseSerializableTriggerState sub-structures
    /// <summary>
    /// The type of effect currently parametered
    /// Carefull : Names corresponds to fields name of <see cref="DualSenseTriggerState"/> and <see cref="DualSenseSerializableTriggerState"/>
    /// </summary>
    public enum DualSenseTriggerEffectType : byte
    {
        // bit details : '>' means fully integrated and 'x' that it is not listed in this enum (yet?) 

        // > 00 00 0 000 = 0x00 => NoResistance (other non attributed values have the same effect)

        // > 00 00 0 001 = 0x01 => ContinuousResistance
        // > 00 00 0 010 = 0x02 => SectionResistance
        // > 00 00 0 101 = 0x05 => Reset (with actuator withdraw)
        // > 00 00 0 110 = 0x06 => VibratingResistance

        // x 00 01 0 001 = 0x11 => ? (only referenced in Nielk1 trigger effect generators)
        // x 00 01 0 010 = 0x12 => ? (only referenced in Nielk1 trigger effect generators)

        //   00 10 0 001 = 0x21 => Crunch (work in progress / poorly understood)
        // > 00 10 0 010 = 0x22 => SnapBack
        // x 00 10 0 011 = 0x23 => (work in progress / poorly understood) ??? 
        // x 00 10 0 101 = 0x25 => (work in progress / poorly understood) ???
        //   00 10 0 110 = 0x26 => EffectEx (pre-fork integration is more complete than the documentation I could find but the parameters are not contiguous so it is probably still incomplete)
        //   00 10 0 111 = 0x27 => AmplitudeVibration (the parameters doesn't seem to respond correctly...)

        // x 11 11 1 100 = 0xFC => Debug/Calibration value ?
        // x 11 11 1 101 = 0xFD => Debug/Calibration value ?
        // x 11 11 1 110 = 0xFE => Debug/Calibration value ?


        [Tooltip("[0x00] Stop the current currently programmed effect but (as opposed to ResetResistance) " +
            "do not withdraw the actuator.")]
        NoResistance = 0x00,

        [Tooltip("[0x05] Used to fully disengage the effect AND withdraw the actuator.")]
        ResetResistance = 0x05,

        [Tooltip("[0x01] Uniform resistance with programmable starting position.")]
        ContinuousResistance = 0x01,

        [Tooltip("[0x02] Resistance section; after overcoming the resistance section, it will be " +
            "re-engaged when reaching the configured resistance start P0.")]
        SectionResistance = 0x02,

        [Tooltip("[0x06] Vibration after entering high resistance region")]
        VibratingResistance = 0x06,

        [Tooltip("[0x21] (work in progress / poorly understood) " +
            "Resistance with slow recovery and optional resistance bumps")]
        Crunch = 0x21,

        [Tooltip("[0x22] Resistance section with \"snap back\"")]
        SnapBack = 0x22,

        [Tooltip("[0x26] (work in progress / poorly understood) Cycling effect")]
        EffectEx = 0x26,

        [Tooltip("[0x27] Fixed resistance with vibration section including amplitude cycling.")]
        AmplitudeVibration = 0x27
    }

    [Serializable]
    public struct DualSenseContinuousResistanceProperties : EffectParameters
    {
        [Range(0, 255), Tooltip("P0: start of resistance (0=released state; 238 fully pressed) " +
            "Trigger input report value range 0 - 255 corresponds to p0 values 30 - 172.")]
        public byte StartPosition;
        [Range(0, 255), Tooltip("P1: Resistance force (0-255) This cannot be used to completely " +
            "disable the effect as 0 represents a low force.")]
        public byte Force;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            triggerParams[0] = StartPosition;
            triggerParams[1] = Force;
        }
    }

    [Serializable]
    public struct DualSenseSectionResistanceProperties : EffectParameters
    {
        [Range(0, 255), Tooltip("P0: resistance starting position (0=released state; 238 fully pressed) " +
            "Trigger input report value 0 - 255 corresponds to P0 values 30 - 167. If starting position(P0) " +
            "is higher than the end position(P1), only a brief impulse event is observed at P1, which is " +
            "unaffected by the force parameter (P2), but rather by how fast the trigger is pressed.")]
        public byte StartPosition;
        [Range(0, 255), Tooltip("P1: resistance end position (0=released state; 238 fully pressed) " +
            "Trigger input report value 0 - 255 corresponds to p1 values 30 - 167.")]
        public byte EndPosition;
        [Range(0, 255), Tooltip("P2: Resistance force (0-255)")]
        public byte Force;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            triggerParams[0] = StartPosition;
            triggerParams[1] = EndPosition;
            triggerParams[2] = Force;
        }
    }

    [Serializable]
    public struct DualSenseVibratingResistanceProperties : EffectParameters
    {
        [Range(0, 255), Tooltip("P0: Vibration frequency in Hz (/!\\ increasingly spotty granularity when exceeding 36Hz).")]
        public byte Frequency;
        [Range(0, 255), Tooltip("P1: Vibration strength (0-63 with 0 being off)")]
        public byte VibrationStrength;
        [Range(0, 255), Tooltip("P2: Effect starting point (0=released; 137=resistance ramp starts at trigger value " +
            "0xd1(209).This is also the highest value that still allows the vibration section to be reached. Trigger " +
            "value range 0 - 255 corresponds to 26 - 168 for resistance. With a P2 value of 0, vibration section " +
            "starts at trigger value of ~0x10. At a P2 value of 255 the resistance ramp can still be felt.")]
        public byte StartPosition;
        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            triggerParams[0] = Frequency;
            triggerParams[1] = VibrationStrength;
            triggerParams[3] = StartPosition;
        }
    }

    [Serializable]
    public struct DualSenseCrunchProperties : EffectParameters
    {
        //TODO find more obvious names
        [ByteDisplay, Tooltip("P0: bit 1 = resistance; bit masks used up to 31?")]
        public byte P0;
        [Range(0, 255), Tooltip("P1: bit 1 to enable resistance bumps, bit 2 something else (can be combined)")]
        public byte P1;
        [Range(0, 255), Tooltip("P2: resistance buildup speed (0=slow; 255=fast) Only used when at least two " +
            "bits are set in P0")]
        public byte P2;
        [Range(0, 255), Tooltip("P3: bit 1 adds two additional, smaller bumps before the one at ~80%")]
        public byte P3;
        [Range(0, 255), Tooltip("P4: some \"snappiness\" characteristic in the lower 6 bits, at least when " +
            "everything else is 255.")]
        public byte P4;
        [ByteDisplay, Tooltip("P5: bits 1-3 force of resistance bumps (some other bits do also have some " +
            "effect); bits 4-6 changes tactile feel for p1 bit 2")]
        public byte P5;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            triggerParams[0] = P0;
            triggerParams[1] = P1;
            triggerParams[2] = P2;
            triggerParams[3] = P3;
            triggerParams[4] = P4;
            triggerParams[5] = P5;
        }
    }

    [Serializable]
    public struct DualSenseSnapBackProperties : EffectParameters
    {
        [DynamicDiscreteRange(0, 8), Tooltip("Position of the start of the resistance")]
        public byte Start;
        [DynamicDiscreteRange(1, 9, minValueProperty = "Start", minValuePropertyOffset = 1, showInaccessibleValue = true), 
            Tooltip("position of the SnapBack : 9 is not reachable")]
        public byte End;
        [Range(0, 7), Tooltip("Resistance Force (3 bit)")]
        public byte ResistanceForce;
        [Range(0, 7), Tooltip("Snap back force (3 bit)")]
        public byte SnapBackForce;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            UInt16 StartEndValue = (UInt16)((1 << Start) | (1 << End));

            // P0+P1 first ten bits : two extreme set bits define the effect section.

            triggerParams[0] = (byte)((StartEndValue >> 0) & 0xff);
            triggerParams[1] = (byte)((StartEndValue >> 8) & 0xff);
            triggerParams[2] = (byte)(((ResistanceForce & 0x07) << (3 * 0))
                                    | ((SnapBackForce   & 0x07) << (3 * 1)));
        }
    }

    [Serializable]
    public struct DualSenseEffectExProperties : EffectParameters
    {
        //TODO complete Tooltips
        [Range(0, 255), Tooltip("")]
        public byte StartPosition;
        [Tooltip("")]
        public bool KeepEffect;
        [Range(0, 255), Tooltip("")]
        public byte BeginForce;
        [Range(0, 255), Tooltip("")]
        public byte MiddleForce;
        [Range(0, 255), Tooltip("")]
        public byte EndForce;
        [Range(0, 255), Tooltip("")]
        public byte Frequency;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            triggerParams[0] = (byte)(0xff - StartPosition);
            triggerParams[1] = (byte)(KeepEffect ? 0x02 : 0x00);
            triggerParams[3] = BeginForce;
            triggerParams[4] = MiddleForce;
            triggerParams[5] = EndForce;
            triggerParams[8] = Frequency;
        }
    }

    [Serializable]
    public struct DualSenseAmplitudeVibrationProperties : EffectParameters
    {
        [DynamicDiscreteRange(0, 8), Tooltip("Position of the start of the effect")]
        public byte Start;
        [DynamicDiscreteRange(1, 9, minValueProperty = "Start", minValuePropertyOffset = 1, showInaccessibleValue = true),
            Tooltip("Position of the end of the effect")]
        public byte End;
        [Range(0, 7), Tooltip("Cycle starting force (3 bit)")]
        public byte StrengthA;
        [Range(0, 7), Tooltip("Cycle peak force (3 bit)")]
        public byte StrengthB;
        [Range(0, 255), Tooltip("P3: Vibration frequency in Hz")]
        public byte Frequency;
        [Range(0, 255), Tooltip("P4: Wave period in 100ms steps. The wave period position appears to be " +
            "advanced as soon as the trigger is engaged.This means there is little to no control over the " +
            "timing.\nDuring this wave period all the strength levels from Strength A to Strength B " +
            "and back to Strength A are cycled through for equally long periods.")]
        public byte WavePeriod;

        public unsafe void GetFormatedParameters(byte* triggerParams)
        {
            UInt16 StartEndValue = (UInt16)((1 << Start) | (1 << End));

            // P0+P1 first ten bits : two extreme set bits define the effect section.

            triggerParams[0] = (byte)((StartEndValue >> 0) & 0xff);
            triggerParams[1] = (byte)((StartEndValue >> 8) & 0xff);
            triggerParams[2] = (byte)(((StrengthA & 0x07) << (3 * 0))
                                    | ((StrengthB & 0x07) << (3 * 1)));
        }
    }
    #endregion

    public enum PlayerLedBrightness
    {
        High,
        Medium,
        Low,
    }

    public struct PlayerLedState
    {
        private const byte LED1 = 0x10;
        private const byte LED2 = 0x08;
        private const byte LED3 = 0x04;
        private const byte LED4 = 0x02;
        private const byte LED5 = 0x01;
        private const byte LED_MASK = LED1 | LED2 | LED3 | LED4 | LED5;

        private const byte FADE = 0x40;

        public byte Value { get; private set; }

        public PlayerLedState(bool led1, bool led2, bool led3, bool led4, bool led5, bool fade = true)
        {
            Value = 0;
            if (led1) Value |= LED1;
            if (led2) Value |= LED2;
            if (led3) Value |= LED3;
            if (led4) Value |= LED4;
            if (led5) Value |= LED5;
            if (fade) Value |= FADE;
        }

        public PlayerLedState(byte led, bool fade = false)
        {
            Value = (byte)(led & LED_MASK);
            if (fade) Value |= FADE;
        }
    }
}