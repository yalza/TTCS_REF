using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable, CreateAssetMenu(fileName = "Button Mapped", menuName = "MFPS/Input/Input Mapped")]
    public class ButtonMapped : ScriptableObject
    {
        public InputType inputType = InputType.Keyboard;
        public Mapped mapped = new Mapped();
        public List<ButtonData> ButtonMap { get { return mapped.ButtonMap; } }
    }
}