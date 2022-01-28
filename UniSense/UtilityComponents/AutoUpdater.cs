using System.Collections;
using UnityEngine;
using UnityEditor;
using UniSense;

/// <summary>
/// A simple autoUpdater to ensure Legacy rumbles stay activated (if they are) (they will stop after a few seconds without update otherwise)
/// and that updates that failed to be sent are tried again without too much delay.
/// </summary>
public class DualSenseAutoUpdater : MonoBehaviour
{
    [Tooltip("Should the auto updates starts as soon as the game is launched")]
    public bool autoStartOnPlay = true;
    [Tooltip("The time elapsed in seconds since the last update after which an auto update should be triggered.\n" +
        "Lower than 0.01 will cause numerous send failures and higher than 5 will not be enough to seamlessly revalidate legacy rumbles.")]
    public float autoUpdateTime = 0.1f;

    private bool _autoUpdateActivated = false;
    DualSenseGamepadHID DualSense;
    public bool AutoUpdateActive
    {
        get { return _autoUpdateActivated; }
        set
        {
            if (!Application.isPlaying)
                return;

            if (value && !_autoUpdateActivated)
                StartCoroutine(AutoUpdateCoroutine());

            if (!value && _autoUpdateActivated)
                StopAllCoroutines();

            _autoUpdateActivated = value;
        }
    }

    public void Start()
    {
        if (autoStartOnPlay)
            AutoUpdateActive = true;
    }

    IEnumerator AutoUpdateCoroutine()
    {
        while (enabled)
        {
            if (DualSense == null)
            {
                DualSense = DualSenseGamepadHID.FindFirst();
            }
            else 
            {
                if (DualSense.TimeSinceLastGamepadUpdate >= autoUpdateTime &&
                        (DualSense.UseLegacyHaptics || !DualSense.UpdateSucceeded))
                {
                    DualSense.UpdateGamepad();
                }

                float nextCallTime = autoUpdateTime - DualSense.TimeSinceLastGamepadUpdate;
                if (nextCallTime > 0)
                    yield return new WaitForSecondsRealtime(nextCallTime);
                else
                    yield return null;
            }
            else
                yield return null;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(DualSenseAutoUpdater))]
public class DualsenseAutoUpdaterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DualSenseAutoUpdater autoUpdater = (DualSenseAutoUpdater)serializedObject.targetObject;

        string state;
        string button;

        if (autoUpdater.AutoUpdateActive)
        {
            state = "AutoUpdate is activated!";
            button = "Stop";
        }
        else
        {
            state = "Stopped...";
            button = "Activate";
        }
        GUILayout.Label("       " + state);

        base.DrawDefaultInspector();

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button(button))
            autoUpdater.AutoUpdateActive = !autoUpdater.AutoUpdateActive;
        GUI.enabled = true;
    }
}

#endif
