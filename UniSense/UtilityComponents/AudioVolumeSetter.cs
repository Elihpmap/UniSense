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
        //[Range(0, 1)]
        //public float pluggedInDeviceVolume;
        public bool updateOnSet = true;

        public void ChangeValue(float integratedSpeakerVolume)
            => this.integratedSpeakerVolume = integratedSpeakerVolume;

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
                dualSenseGamepad.SetAudioVolume(integratedSpeakerVolume, DualSenseTargetAudioDevice.InternalSpeaker, updateOnSet);
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AudioVolumeSetter))]
    [CanEditMultipleObjects]
    public class AudioVolumeSetterEditor : Editor
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
