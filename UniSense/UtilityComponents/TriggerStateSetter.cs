using UnityEngine;
using UnityEditor;

namespace UniSense
{
    /// <summary>
    /// A simple component to setup and update triggerStates from the unity editor
    /// </summary>
    public class TriggerStateSetter : MonoBehaviour
    {
        public enum TriggerType
        {
            Left,
            Right,
            Both
        }

        public TriggerType triggerToSet = TriggerType.Both;
        public DualSenseSerializableTriggerState triggerState;
        public bool updateOnSet = true;

        void Start()
        {

        }

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
                switch (triggerToSet)
                {
                    case TriggerType.Left:
                        dualSenseGamepad.SetTriggerState(triggerState, null, updateOnSet);
                        break;

                    case TriggerType.Right:
                        dualSenseGamepad.SetTriggerState(null, triggerState, updateOnSet);
                        break;

                    case TriggerType.Both:
                        dualSenseGamepad.SetTriggerState(triggerState, triggerState, updateOnSet);
                        break;
                }
            }
        }

        public void SetFirst() => Set(DualSenseGamepadHID.FindFirst());
        public void SetCurrent() => Set();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TriggerStateSetter))]
    [CanEditMultipleObjects]
    public class TriggerStateSetterEditor : Editor
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
                (serializedObject.targetObject as TriggerStateSetter).Set(DualSenseGamepadHID.FindFirst());
            }

            //Button
            if (GUILayout.Button(new GUIContent("Call Set(First_Dualsense)", 
                "Sets the DualSenseGamepadHID.FindFirst() TriggerStates as specified by this component")))
            {
                foreach (TriggerStateSetter setter in targets)
                {
                    setter.SetFirst();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}