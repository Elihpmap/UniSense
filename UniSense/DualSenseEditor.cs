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

                case DualSenseTriggerEffectType.EffectEx:
                    this.EffectEx = triggerState.EffectEx;
                    break;

                default:
                case DualSenseTriggerEffectType.Crunch:             //TODO Add complete implementation
                case DualSenseTriggerEffectType.SnapBack:           //TODO Add complete implementation
                case DualSenseTriggerEffectType.AmplitudeVibration: //TODO Add complete implementation
                    Debug.LogError("Unimplemented EffectType !");
                    break;
            }
        }
        public static implicit operator DualSenseSerializableTriggerState(DualSenseTriggerState TS) =>
           new DualSenseSerializableTriggerState(TS);
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

                case DualSenseTriggerEffectType.VibratingResistance://TODO Add complete implementation
                    effectParameters = property.FindPropertyRelative("VibratingResistance");
                    break;

                case DualSenseTriggerEffectType.EffectEx:
                    effectParameters = property.FindPropertyRelative("EffectEx");
                    break;

                default:
                case DualSenseTriggerEffectType.Crunch:             //TODO Add complete implementation
                case DualSenseTriggerEffectType.SnapBack:           //TODO Add complete implementation
                case DualSenseTriggerEffectType.AmplitudeVibration: //TODO Add complete implementation
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