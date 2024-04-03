namespace MFPS.InputManager
{
    using UnityEngine;
    using System.Collections.Generic;

    public static class GamePadButtonsNames
    {
        public static readonly string[] TriggerAxis = new string[4] { "RT", "LT", "DPV", "DPH" };
        public static readonly Dictionary<string, Dictionary<float, string>> DirectionalAxisNames = new Dictionary<string, Dictionary<float, string>>() {
        { "DPV", new Dictionary<float, string>{ {1,"D-Pad Up" },{ -1, "D-Pad Down" } } },
        { "DPH", new Dictionary<float, string>{ {1,"D-Pad Right" }, {-1, "D-Pad Left" } } }
        };

        public static readonly Dictionary<string, float> SubAxisValues = new Dictionary<string, float>() { { "D-Pad Up", 1 }, { "D-Pad Down",-1 }, { "D-Pad Right", 1 }, { "D-Pad Left", -1 }, };


        public static readonly Dictionary<KeyCode, string> Xbox = new Dictionary<KeyCode, string> {
        { KeyCode.JoystickButton0, "A" },{KeyCode.JoystickButton1, "B" }, {KeyCode.JoystickButton2, "X" }, {KeyCode.JoystickButton3, "Y" },
        {KeyCode.JoystickButton4, "LB" }, {KeyCode.JoystickButton5, "RB" }, {KeyCode.JoystickButton6, "Back" }, {KeyCode.JoystickButton7, "Start" },
        {KeyCode.JoystickButton8, "LSC" }, {KeyCode.JoystickButton9, "RSC" },/*1*/{KeyCode.Joystick1Button0, "A" },{KeyCode.Joystick1Button1, "B" }, 
        {KeyCode.Joystick1Button2, "X" }, {KeyCode.Joystick1Button3, "Y" }, {KeyCode.Joystick1Button4, "LB" }, {KeyCode.Joystick1Button5, "RB" },
        {KeyCode.Joystick1Button6, "Back" }, {KeyCode.Joystick1Button7, "Start" },{KeyCode.Joystick1Button8, "LSC" }, {KeyCode.Joystick1Button9, "RSC" }};
    }
}