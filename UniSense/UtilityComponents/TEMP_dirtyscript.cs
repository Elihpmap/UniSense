using System.Collections;
using System.Collections.Generic;
using UniSense;
using UniSense.LowLevel;
using UnityEngine;

public class TEMP_dirtyscript : MonoBehaviour
{
    public static TEMP_dirtyscript instance = null;

    private void Start()
    {
        instance = this;
    }
    bool lastbutton = false;
    public bool button;

    [ByteDisplay]
    public byte prevFlag1;
    [ByteDisplay]
    public byte prevFlag2;
    
    [Space]
    [ByteDisplay]
    public byte flag1;
    [ByteDisplay]
    public byte flag2;

    [Range(0, 255), Tooltip("maxes out at 127")]
    public byte externalVolume;
    [Range(0,255), Tooltip("PS5 seems to set only from 61 to 100")]
    public byte internalVolume;
    [ByteDisplay]
    public byte internalMicVolume;
    [ByteDisplay]
    public byte audioFlags;

    [ByteDisplay]
    public byte secondInternalVolume;


    //[Space]
    //[Space, SerializeField]
    //DualSenseHIDOutputReport test;


    private void Update()
    {
        DualSenseGamepadHID dualSense = DualSenseGamepadHID.FindFirst();
        if (button != lastbutton)
        {
            dualSense.UpdateGamepad();
            button = lastbutton;
        }
    }
}
