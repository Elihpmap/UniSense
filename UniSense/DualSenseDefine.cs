using System;
using System.Runtime.InteropServices;
using UnityEditor;
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

    // 00 00 0 000 = 0x00 => NoResistance (other non attributed values have the same effect)

    // 00 00 0 001 = 0x01 => ContinuousResistance
    // 00 00 0 010 = 0x02 => SectionResistance
    // 00 00 0 101 = 0x05 => Reset
    // 00 00 0 110 = 0x06 => VibratingResistance

    // 00 01 0 001 = 0x11 => ? (only referenced in Nielk1 trigger effect generators)
    // 00 01 0 010 = 0x12 => ? (only referenced in Nielk1 trigger effect generators)

    // 00 10 0 001 = 0x21 => Crunch (work in progress / poorly understood)
    // 00 10 0 010 = 0x22 => SnapBack
    // 00 10 0 011 = 0x23 => (work in progress / poorly understood) ??? 
    // 00 10 0 100 = 0x25 => (work in progress / poorly understood) ???
    // 00 10 0 101 = 0x26 => EffectEx (pre-fork integration is more complete than the documentation I could find but the parameters are not contiguous so it is probably still incomplete)
    // 00 10 0 111 = 0x27 => AmplitudeVibration

    // 11 11 1 100 = 0xFC => Debug/Calibration value ?
    // 11 11 1 101 = 0xFD => Debug/Calibration value ?
    // 11 11 1 110 = 0xFE => Debug/Calibration value ?

    public enum DualSenseTriggerEffectType : byte
    {
        [Tooltip("[0x00] Stop the current currently programmed effect but (as opposed to ResetResistance), " +
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
        VibratingResistance = 0x06, // temporary name //TODO

        [Tooltip("[0x21] (work in progress / poorly understood) " +
            "Resistance with slow recovery and optional resistance bumps")]
        Crunch = 0x21, // temporary name //TODO

        [Tooltip("[0x22] Resistance section with \"snap back\"")]
        SnapBack = 0x22, // temporary name //TODO

        [Tooltip("[0x26] (work in progress / poorly understood) Cycling effect")]
        EffectEx = 0x26,

        [Tooltip("[0x27] Fixed resistance with vibration section including amplitude cycling.")]
        AmplitudeVibration = 0x27 // temporary name //TODO
    }

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
        public DualSenseEffectExProperties EffectEx;


        public DualSenseTriggerState(DualSenseTriggerState triggerState) : this()
        {
            this = (DualSenseTriggerState)triggerState.MemberwiseClone();
        }
        public DualSenseTriggerState(DualSenseSerializableTriggerState triggerState) : this()
        {
            this.EffectType = triggerState.EffectType;
            switch (this.EffectType)
            {
                case DualSenseTriggerEffectType.NoResistance:
                case DualSenseTriggerEffectType.ResetResistance:
                case DualSenseTriggerEffectType.VibratingResistance://TODO Add complete implementation
                case DualSenseTriggerEffectType.Crunch:             //TODO Add complete implementation
                case DualSenseTriggerEffectType.SnapBack:           //TODO Add complete implementation
                case DualSenseTriggerEffectType.AmplitudeVibration: //TODO Add complete implementation
                    break;

                case DualSenseTriggerEffectType.ContinuousResistance:
                    this.ContinuousResistance = triggerState.ContinuousResistance;
                    break;

                case DualSenseTriggerEffectType.SectionResistance:
                    this.SectionResistance = triggerState.SectionResistance;
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    this.EffectEx = triggerState.EffectEx;
                    break;

                default:
                    Debug.LogError("Unimplemented EffectType !");
                    break;
            }
        }
        public static implicit operator DualSenseTriggerState(DualSenseSerializableTriggerState TS) =>
           new DualSenseTriggerState(TS);
    }

    // Having an explicit layout with different interpretation of the same data is incompatible with unity inspector
    [Serializable]
    public struct DualSenseSerializableTriggerState
    {
        public DualSenseTriggerEffectType EffectType;

        public DualSenseContinuousResistanceProperties ContinuousResistance;
        public DualSenseSectionResistanceProperties SectionResistance;
        public DualSenseEffectExProperties EffectEx;


        public DualSenseSerializableTriggerState(DualSenseSerializableTriggerState triggerState) : this()
        {
            this = (DualSenseSerializableTriggerState)triggerState.MemberwiseClone();
        }
        public DualSenseSerializableTriggerState(DualSenseTriggerState triggerState) : this()
        {
            this.EffectType = triggerState.EffectType;
            switch (this.EffectType)
            {
                case DualSenseTriggerEffectType.NoResistance:
                case DualSenseTriggerEffectType.ResetResistance:
                case DualSenseTriggerEffectType.VibratingResistance://TODO Add complete implementation
                case DualSenseTriggerEffectType.Crunch:             //TODO Add complete implementation
                case DualSenseTriggerEffectType.SnapBack:           //TODO Add complete implementation
                case DualSenseTriggerEffectType.AmplitudeVibration: //TODO Add complete implementation
                    break;

                case DualSenseTriggerEffectType.ContinuousResistance:
                    this.ContinuousResistance = triggerState.ContinuousResistance;
                    break;

                case DualSenseTriggerEffectType.SectionResistance:
                    this.SectionResistance = triggerState.SectionResistance;
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    this.EffectEx = triggerState.EffectEx;
                    break;

                default:
                    Debug.LogError("Unimplemented EffectType !");
                    break;
            }
        }
        public static implicit operator DualSenseSerializableTriggerState(DualSenseTriggerState TS) =>
           new DualSenseSerializableTriggerState(TS);
    }

    [Serializable]
    public struct DualSenseContinuousResistanceProperties
    {
        [Range(0, 255), Tooltip("P0: start of resistance (0=released state; 238 fully pressed) " +
            "Trigger input report value range 0 - 255 corresponds to p0 values 30 - 172.")]
        public byte StartPosition;
        [Range(0, 255), Tooltip("P1: Resistance force (0-255) This cannot be used to completely " +
            "disable the effect as 0 represents a low force.")]
        public byte Force;
    }

    [Serializable]
    public struct DualSenseSectionResistanceProperties
    {
        [Range(0, 255), Tooltip("P0: resistance starting position (0=released state; 238 fully pressed) " +
            "Trigger input report value 0 - 255 corresponds to P0 values 30 - 167. If starting position(P0) " +
            "is higher than the end position(P1), only a brief impulse event is observed at P1, which is " +
            "unaffected by the force parameter (P2), but rather by how fast the trigger is pressed.")]
        public byte StartPosition;
        [Range(0, 255), Tooltip("resistance end position (0=released state; 238 fully pressed) " +
            "Trigger input report value 0 - 255 corresponds to p1 values 30 - 167.")]
        public byte EndPosition;
        [Range(0, 255), Tooltip("Resistance force (0-255)")]
        public byte Force;
    }

    [Serializable]
    public struct DualSenseEffectExProperties
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
    }

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
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DualSenseSerializableTriggerState))]
    public class UniqueObjectLogicProperty : PropertyDrawer
    {
        bool isFoldout;
        SerializedProperty effectType = null;
        SerializedProperty effectParameters;

        void Init(SerializedProperty property)
        {
            if (effectType is null)
                effectType = property.FindPropertyRelative("EffectType");
            
            switch ((DualSenseTriggerEffectType) effectType.intValue)
            {
                case DualSenseTriggerEffectType.ContinuousResistance:
                    effectParameters = property.FindPropertyRelative("ContinuousResistance");
                    break;

                case DualSenseTriggerEffectType.SectionResistance:
                    effectParameters = property.FindPropertyRelative("SectionResistance");
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    effectParameters = property.FindPropertyRelative("EffectEx");
                    break;

                case DualSenseTriggerEffectType.NoResistance:
                case DualSenseTriggerEffectType.ResetResistance:
                case DualSenseTriggerEffectType.VibratingResistance://TODO Add complete implementation
                case DualSenseTriggerEffectType.Crunch:             //TODO Add complete implementation
                case DualSenseTriggerEffectType.SnapBack:           //TODO Add complete implementation
                case DualSenseTriggerEffectType.AmplitudeVibration: //TODO Add complete implementation
                    effectParameters = null;
                    break;

                default:
                    Debug.LogError("Unimplemented Effect Type Value");
                    break;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            Rect subPos = new Rect(position);
            EditorGUI.BeginProperty(position, label, property);
            subPos.height = EditorGUIUtility.singleLineHeight;
            isFoldout = EditorGUI.BeginFoldoutHeaderGroup(subPos, isFoldout, label);
            if (isFoldout)
            {
                EditorGUI.indentLevel++;
                
                subPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                subPos.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(subPos, effectType, new GUIContent("Effect Type"), true);
                
                if (effectType.hasMultipleDifferentValues)
                {
                    subPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.LabelField(subPos, "Cannot edit mutiple parameters when Effect Type is different");
                }
                else if (effectParameters != null)
                {
                    subPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    subPos.height = EditorGUI.GetPropertyHeight(effectParameters);
                    EditorGUI.PropertyField(subPos, effectParameters, 
                        new GUIContent("Effect Parameters", "Parameters of the effect, hover over them for more details"), true);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            float total = EditorGUIUtility.singleLineHeight;
            if (isFoldout)
            {
                total += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight; 
                if (effectType.hasMultipleDifferentValues)
                    total += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                else if (effectParameters != null)
                    total += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(effectParameters);
            }
            return total;
        }
    }
#endif
}