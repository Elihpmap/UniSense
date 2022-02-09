using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniSense
{
    public class AudioVolumeSetter : MonoBehaviour
    {
        [Range(0, 1)]
        public float integratedSpeakerVolume;
        [Range(0, 1)]
        public float pluggedInDeviceVolume;

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
                dualSenseGamepad.SetAudioVolume(integratedSpeakerVolume);
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AudioVolumeSetter))]
    [CanEditMultipleObjects]
    public class AudioVolumeSetterEditor : Editor
    {
        SerializedProperty integratedSpeakerVolumeProperty;
        SerializedProperty pluggedInDeviceVolumeProperty;

        bool autoUpdate = false;

        private void OnEnable()
        {
            integratedSpeakerVolumeProperty = serializedObject.FindProperty("integratedSpeakerVolume");
            pluggedInDeviceVolumeProperty = serializedObject.FindProperty("pluggedInDeviceVolume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(integratedSpeakerVolumeProperty);
            EditorGUILayout.PropertyField(pluggedInDeviceVolumeProperty);


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
                (serializedObject.targetObject as AudioVolumeSetter).Set(DualSenseGamepadHID.FindFirst());
            }

            //Button
            if (GUILayout.Button(new GUIContent("Call Set(First_Dualsense)",
                "Sets the DualSenseGamepadHID.FindFirst() TriggerStates as specified by this component")))
            {
                foreach (AudioVolumeSetter setter in targets)
                {
                    setter.Set(DualSenseGamepadHID.FindFirst());
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
