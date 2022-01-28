using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;


/// Serializables version of a few unserializable struct defined in DualSenseDefine.cs
/// And some custom inspector and property drawer Attributes to display them correctly
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
            this = triggerState;
        }
        public DualSenseSerializableTriggerState(DualSenseTriggerState triggerState) : this()
        {
            this.EffectType = triggerState.EffectType;

            if (EffectType != DualSenseTriggerEffectType.NoResistance
                && EffectType != DualSenseTriggerEffectType.ResetResistance)
            {
                FieldInfo thisEffectParameters = typeof(DualSenseSerializableTriggerState).GetField(EffectType.ToString());
                FieldInfo otherEffectParameters = typeof(DualSenseTriggerState).GetField(EffectType.ToString());
                object BoxedTriggerState = this; //need boxing otherwise 'this' is a value and is copied, not refered to SetValue()
                thisEffectParameters.SetValue(this, otherEffectParameters.GetValue(triggerState));
                this = (DualSenseSerializableTriggerState)BoxedTriggerState;
            }
        }
        public static explicit operator DualSenseSerializableTriggerState(DualSenseTriggerState TS) =>
           new DualSenseSerializableTriggerState(TS);
    }

    #region DualSenseSerializableTriggerState custom inspector display
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DualSenseSerializableTriggerState))]
    [CanEditMultipleObjects]
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

            if ((DualSenseTriggerEffectType)effectType.intValue == DualSenseTriggerEffectType.NoResistance
                || (DualSenseTriggerEffectType)effectType.intValue == DualSenseTriggerEffectType.ResetResistance)
            {
                effectParameters = null; 
            }
            else
            {
                effectParameters = property.FindPropertyRelative(((DualSenseTriggerEffectType)effectType.intValue).ToString());
                if (effectParameters == null)
                    Debug.LogError("Unimplemented Effect Type Value");
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
#endif
    #endregion

    #region Custom additionnal display attributes

    /// <summary>
    /// An attribute to display a <see cref="byte"/> type value as 8 boolean bits in the inspector.
    /// Custom property drawer is <see cref="ByteDisplayPropertyDrawer"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ByteDisplay : PropertyAttribute { }

    /// <summary>
    /// An attribute to make a more customisable discrete range slider 
    /// (with the possibility to reference properties as min or max for example).
    /// Custom property drawer is <see cref="DynamicDiscreteRangePropertyDrawer"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DynamicDiscreteRange : PropertyAttribute
    {
        public int defaultMinValue;
        public int defaultMaxValue;
        public int minValuePropertyOffset = 0;
        public int maxValuePropertyOffset = 0;
        public string minValueProperty = null;
        public string maxValueProperty = null;
        public bool showInaccessibleValue = false;

        public DynamicDiscreteRange(int defaultMinValue, int defaultMaxValue)
        {
            this.defaultMinValue = defaultMinValue;
            this.defaultMaxValue = defaultMaxValue;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ByteDisplay))]
    public class ByteDisplayPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type != "byte")
            {
                Debug.LogError("The ByteDisplay attribute is only suited for byte type! " +
                    "It is not suitable for the property " + label.text + " which detected type is \"" + property.type + "\"");
                EditorGUI.PropertyField(position, property);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            position.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(position, label);

            position.x += position.width - (15 * EditorGUI.indentLevel);
            position.width = 15 * (EditorGUI.indentLevel + 1); // ??? No idea why the width needs to be bigger if the content is indented, but otherwise the toggles are not hoverable nor clickable.

            byte value = (byte)property.intValue;

            for (int i = 7; i >= 0; i--)
            {
                bool bitValue = (value & (1 << i)) == (1 << i);
                bitValue = EditorGUI.Toggle(position, bitValue);
                if (bitValue)
                    value |= (byte)(1 << i);
                else
                    value &= (byte)~(1 << i);

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

    [CustomPropertyDrawer(typeof(DynamicDiscreteRange))]
    public class DynamicDiscreteRangePropertyDrawer : PropertyDrawer
    {
        SerializedProperty findSiblingProperty(SerializedProperty baseProperty, string siblingPropertyName)
        {
            string baseParentPath = baseProperty.propertyPath.Substring(0, baseProperty.propertyPath.LastIndexOf('.')+1);
            return baseProperty.serializedObject.FindProperty(baseParentPath + siblingPropertyName);
        }

        Vector2Int GetDynamicMinMax(DynamicDiscreteRange dynamicDiscreteRange, SerializedProperty property)
        {
            Vector2Int minMax = new Vector2Int(dynamicDiscreteRange.defaultMinValue, dynamicDiscreteRange.defaultMaxValue);

            if (!(dynamicDiscreteRange.minValueProperty is null || dynamicDiscreteRange.minValueProperty == ""))
            {
                SerializedProperty minProp = findSiblingProperty(property, dynamicDiscreteRange.minValueProperty);
                if (minProp != null)
                    minMax.x = minProp.intValue + dynamicDiscreteRange.minValuePropertyOffset;
                else
                    Debug.LogError("DynamicDiscreteRangePropertyDrawer : couldn't find property : " + dynamicDiscreteRange.minValueProperty);
            }

            if (!(dynamicDiscreteRange.maxValueProperty is null || dynamicDiscreteRange.maxValueProperty == ""))
            {
                SerializedProperty maxProp = findSiblingProperty(property, dynamicDiscreteRange.maxValueProperty);
                if (maxProp != null)
                    minMax.y = maxProp.intValue + dynamicDiscreteRange.maxValuePropertyOffset;
                else
                    Debug.LogError("DynamicDiscreteRangePropertyDrawer : couldn't find property : " + dynamicDiscreteRange.maxValueProperty);
            }

            return minMax;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {    
            if (property.type != "int"
                && property.type != "uint"
                && property.type != "byte" 
                && property.type != "sbyte"
                && property.type != "byte"
                && property.type != "short"
                && property.type != "ushort")
            {
                Debug.LogError("The DynamicDiscreteRange attribute is only suited for non-long discrete type! " +
                    "It is not suitable for the property " + label.text + " which detected type is \"" + property.type + "\"");
                EditorGUI.PropertyField(position, property);
                return;
            }

            DynamicDiscreteRange dynamicDiscreteRangeAttribute = attribute as DynamicDiscreteRange;

            EditorGUI.BeginProperty(position, label, property);
            
            Vector2Int minMax = GetDynamicMinMax(dynamicDiscreteRangeAttribute, property);

            if (dynamicDiscreteRangeAttribute.showInaccessibleValue)
            {
                EditorGUI.IntSlider(position, property, dynamicDiscreteRangeAttribute.defaultMinValue, dynamicDiscreteRangeAttribute.defaultMaxValue);
            }
            else
            {
                EditorGUI.IntSlider(position, property, minMax.x, minMax.y);
            }

            if (property.intValue < minMax.x)
                property.intValue = minMax.x;
            if (property.intValue > minMax.y)
                property.intValue = minMax.y;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
    #endregion
}