using System;
using UnityEditor;
using UnityEngine;

namespace UniSense
{
    // Having an explicit layout with different interpretation of the same data is incompatible with unity inspector
    /// <summary>
    /// A Serializable (and thus unity inspector displayable) version of <see cref="DualSenseTriggerState"/>.
    /// This uses a CustomPropertyDrawer (mainly show only the currently related effect parameter)
    /// </summary>
    [Serializable]
    public struct DualSenseSerializableTriggerState
    {
        public DualSenseTriggerEffectType EffectType;

        public DualSenseContinuousResistanceProperties ContinuousResistance;
        public DualSenseSectionResistanceProperties SectionResistance;
        public DualSenseVibratingResistanceProperties VibratingResistance;
        public DualSenseCrunchProperties Crunch;
        public DualSenseSnapBackProperties SnapBack;
        public DualSenseEffectExProperties EffectEx;
        public DualSenseAmplitudeVibrationProperties AmplitudeVibration;


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
                    break;

                case DualSenseTriggerEffectType.ContinuousResistance:
                    this.ContinuousResistance = triggerState.ContinuousResistance;
                    break;

                case DualSenseTriggerEffectType.SectionResistance:
                    this.SectionResistance = triggerState.SectionResistance;
                    break;

                case DualSenseTriggerEffectType.VibratingResistance:
                    this.VibratingResistance = triggerState.VibratingResistance;
                    break;

                case DualSenseTriggerEffectType.Crunch:
                    this.Crunch = triggerState.Crunch;
                    break;

                case DualSenseTriggerEffectType.SnapBack:
                    this.SnapBack = triggerState.SnapBack;
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    this.EffectEx = triggerState.EffectEx;
                    break;

                case DualSenseTriggerEffectType.AmplitudeVibration:
                    this.AmplitudeVibration = triggerState.AmplitudeVibration;
                    break;

                default:
                    Debug.LogError("Unimplemented EffectType !");
                    break;
            }
        }
        public static implicit operator DualSenseSerializableTriggerState(DualSenseTriggerState TS) =>
           new DualSenseSerializableTriggerState(TS);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ByteDisplay : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DualSenseSerializableTriggerState))]
    public class UniqueObjectLogicProperty : PropertyDrawer
    {
        bool isFoldout;
        bool showWarningMessage;
        SerializedProperty effectType = null;
        SerializedProperty effectParameters;

        void Init(SerializedProperty property)
        {
            if (effectType is null)
                effectType = property.FindPropertyRelative("EffectType");

            showWarningMessage = false;

            switch ((DualSenseTriggerEffectType) effectType.intValue)
            {
                case DualSenseTriggerEffectType.NoResistance:
                case DualSenseTriggerEffectType.ResetResistance:
                    effectParameters = null;
                    break;

                case DualSenseTriggerEffectType.ContinuousResistance:
                    effectParameters = property.FindPropertyRelative("ContinuousResistance");
                    break;

                case DualSenseTriggerEffectType.SectionResistance:
                    effectParameters = property.FindPropertyRelative("SectionResistance");
                    break;

                case DualSenseTriggerEffectType.VibratingResistance:
                    effectParameters = property.FindPropertyRelative("VibratingResistance");
                    break;

                case DualSenseTriggerEffectType.Crunch:
                    effectParameters = property.FindPropertyRelative("Crunch");
                    showWarningMessage = true;
                    break;

                case DualSenseTriggerEffectType.SnapBack:
                    effectParameters = property.FindPropertyRelative("SnapBack");
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    effectParameters = property.FindPropertyRelative("EffectEx");
                    showWarningMessage = true;
                    break;

                case DualSenseTriggerEffectType.AmplitudeVibration:
                    effectParameters = property.FindPropertyRelative("AmplitudeVibration");
                    break;

                default:
                    Debug.LogError("Unimplemented Effect Type Value");
                    break;
            }
        }

        void ShowIndentedWarningBox(ref Rect subPos, string message)
        {
            float indentValue = EditorGUI.indentLevel * 15;
            subPos.height = EditorGUIUtility.singleLineHeight * 2;
            subPos.x += indentValue; subPos.width -= indentValue;
            EditorGUI.HelpBox(subPos, message, MessageType.Warning);
            subPos.x -= indentValue; subPos.width += indentValue;
            subPos.y += subPos.height + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            Rect subPos = new Rect(position);
            EditorGUI.BeginProperty(position, label, property);
            subPos.height = EditorGUIUtility.singleLineHeight;
            isFoldout = EditorGUI.BeginFoldoutHeaderGroup(subPos, isFoldout, label); 
            subPos.y += subPos.height + EditorGUIUtility.standardVerticalSpacing;
            if (isFoldout)
            {
                EditorGUI.indentLevel++;
                
                EditorGUI.PropertyField(subPos, effectType, new GUIContent("Effect Type"), true);
                subPos.y += subPos.height + EditorGUIUtility.standardVerticalSpacing; 
                
                if (showWarningMessage)
                {
                    ShowIndentedWarningBox(ref subPos, "This Effect Type is not completely understood yet...\nBe careful!");
                }

                if (effectType.hasMultipleDifferentValues)
                {
                    subPos.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(subPos, "Cannot edit mutiple parameters when Effect Type is different");
                }
                else if (effectParameters != null)
                {
                    subPos.height = EditorGUI.GetPropertyHeight(effectParameters);
                    EditorGUI.PropertyField(subPos, effectParameters, 
                        new GUIContent("Effect Parameters", "Parameters of the effect, hover over them for more details"), true);
                }
                subPos.y += subPos.height + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel--;
            }
            else if (showWarningMessage)
            {
                EditorGUI.indentLevel++;
                ShowIndentedWarningBox(ref subPos, "The selected Effect Type is not completely understood yet...\nBe careful!");
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
            if (showWarningMessage)
                total += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight * 2;
            return total;
        }
    }

    [CustomPropertyDrawer(typeof(ByteDisplay))]
    public class ByteDisplayPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type != "byte")
            {
                Debug.LogError("The ByteDisplay attribute is only suited for byte type! " +
                    "It is not suitable for the property " + label.text);
                EditorGUI.PropertyField(position, property);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, label);

            position.x = EditorGUIUtility.labelWidth - 10;
            position.width = 45;
           
            byte value = (byte)property.intValue;

            for (int i = 7; i >= 0; i--)
            {
                bool bitValue = (value & (1 << i)) == (1 << i);
                bitValue = EditorGUI.Toggle(position, bitValue);
                if (bitValue)
                    value |= (byte)(1 << i);
                else
                    value &= (byte)(255 ^ (1 << i));

                position.x += EditorGUIUtility.singleLineHeight;
            }

            property.intValue = value;


            position.width = 150;
            EditorGUI.LabelField(position, "= " + value.ToString() + " = 0x" + value.ToString("x"));

            //EditorGUI.PropertyField(position, property, label, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

#endif
}