using UnityEngine;
using System.Collections.Generic;
using MFPS.InputManager;
using UnityEngine.UI;

public static class bl_Input
{
    public static bl_InputData InputData => bl_InputData.Instance;

    public static InputType inputType
    {
        get { return bl_InputData.Instance.inputType; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize()
    {
        bl_InputData.Instance.Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButton(string keyName)
    {
        return bl_InputData.Instance.GetButton(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonDown(string keyName)
    {
        return bl_InputData.Instance.GetButtonDown(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonUp(string keyName)
    {
        return bl_InputData.Instance.GetButtonUp(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static string GetButtonName(string keyName)
    {
        return bl_InputData.Instance.GetButtonName(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static InputType GetInputType()
    {
        string[] names = Input.GetJoystickNames();
        InputType t = InputType.Keyboard;
        for(int  i = 0; i < names.Length; i++)
        {
            Debug.Log("Joystick: " + names[i]);
        }
        return t;
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Vertical");
    /// </summary>
    public static float VerticalAxis
    {
        get
        {
            if (!isGamePad)
            {
                if (isButton("Forward") && !isButton("Backward"))
                {
                    return 1;
                }
                else if (!isButton("Forward") && isButton("Backward"))
                {
                    return -1;
                }
                else if (isButton("Forward") && isButton("Backward"))
                {
                    return 0.5f;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return Input.GetAxis("Vertical");
            }
        }
    }

    /// <summary>
    /// start button on game pad controllers
    /// </summary>
    public static bool isStartPad
    {
        get
        {
            return Input.GetKeyDown("joystick button 7");
        }
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Horizontal");
    /// </summary>
    public static float HorizontalAxis
    {
        get
        {
            if (!isGamePad)
            {
                if (isButton("Right") && !isButton("Left"))
                {
                    return 1;
                }
                else if (!isButton("Right") && isButton("Left"))
                {
                    return -1;
                }
                else if (isButton("Right") && isButton("Left"))
                {
                    return 0.5f;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return Input.GetAxis("Horizontal");
            }
        }
    }

    public static void CheckGamePadRequired()
    {
        if (bl_InputData.Instance.mappedInstance == null || !bl_InputData.Instance.useGamePadNavigation) return;
        if (bl_InputData.Instance.mappedInstance.inputType != InputType.Xbox && bl_InputData.Instance.mappedInstance.inputType != InputType.Playstation)
            return;

        bl_GamePadPointerModule dpm = Object.FindObjectOfType<bl_GamePadPointerModule>();
        if(dpm == null)
        {
            GameObject go = GameObject.Instantiate(bl_InputData.Instance.GamePadInputModule) as GameObject;
            dpm = go.GetComponent<bl_GamePadPointerModule>();
            dpm.CheckCanvas();
        }

        bl_GamePadPointer gmp = bl_GamePadPointer.Instance; 
        if(gmp == null)
        {
            bl_UIReferences uir = bl_UIReferences.Instance;
            GameObject go = GameObject.Instantiate(bl_InputData.Instance.GamePadPointerPrefab) as GameObject;
            gmp = go.GetComponent<bl_GamePadPointer>();
            if (uir != null)
            {
                Transform parent = uir.transform.GetChild(1);
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
            }
            else
            {
                GraphicRaycaster c = GameObject.FindObjectOfType<GraphicRaycaster>();
                Transform parent = c.transform;
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
            }
        }
    }

    public static bool isGamePad { get { return bl_InputData.Instance.inputType == InputType.Playstation || bl_InputData.Instance.inputType == InputType.Xbox; } }

}