using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniSense
{
    public class LedsColorSetter : MonoBehaviour
    {
        public Color lightBarColor;
        public bool updateOnSet = true;

        public void Set(DualSenseGamepadHID dualSenseGamepad = null)
        {
            if (dualSenseGamepad is null)
                dualSenseGamepad = DualSenseGamepadHID.FindCurrent();

            if (dualSenseGamepad is null)
            {
                Debug.LogWarning("Current gamepad (default value) is not a dualsense");
                return;
            }
            else
            {
                dualSenseGamepad.SetLightBarColor(lightBarColor, updateOnSet);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LedsColorSetter))]
    [CanEditMultipleObjects]
    public class LedsColorSetterEditor : Editor
    {
        bool autoUpdate = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.DrawDefaultInspector();

            EditorGUILayout.Space();

            //AutoUpdate
            if (!Application.isPlaying)
            {
                if (!serializedObject.isEditingMultipleObjects)
                    autoUpdate = EditorGUILayout.Toggle(new GUIContent("Auto Update",
                        "Automaticaly update the gamepad each time the component values are modified." +
                        "\nThis is for test purposes and not available while in Play mode."), autoUpdate);
                else
                    EditorGUILayout.LabelField(new GUIContent("Auto Update",
                        "Automaticaly update the gamepad each time the component values are modified." +
                        "\nThis is for test purposes and not available while in Play mode."),
                        new GUIContent("Cannot use AutoUpdate while editing multiple object"));
            }

            if (autoUpdate && !Application.isPlaying && serializedObject.hasModifiedProperties)
            {
                (serializedObject.targetObject as LedsColorSetter).Set(DualSenseGamepadHID.FindFirst());
            }

            //Button
            if (GUILayout.Button(new GUIContent("Call Set(First_Dualsense)",
                "Sets the DualSenseGamepadHID.FindFirst() TriggerStates as specified by this component")))
            {
                foreach (LedsColorSetter setter in targets)
                {
                    setter.Set(DualSenseGamepadHID.FindFirst());
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}