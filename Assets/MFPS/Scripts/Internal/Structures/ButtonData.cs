using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable]
    public class ButtonData
    {
        public string KeyName;
        [TextArea(1, 2)] public string Description;
        [KeyFinder] public KeyCode PrimaryKey = KeyCode.None;
        [KeyFinder] public KeyCode AlternativeKey = KeyCode.None;

        public string PrimaryAxis = "";
        public string AlternativeAxis = "";

        public bool PrimaryIsAxis = false;
        public bool AlternativeIsAxis = false;

        public float AxisValue = 1;

        private bool wasPressed = false;
        private int lastDownFrame = 0;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButtonDown()
        {
            bool isPressedThisFrame = !PrimaryIsAxis ? Input.GetKeyDown(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isPressedThisFrame)
            {
                if (wasPressed && lastDownFrame != Time.frameCount)
                {
                    isPressedThisFrame = false;
                }
                else
                {
                    wasPressed = true;
                }
            }
            else if (!isPressedThisFrame) { wasPressed = false; }
            lastDownFrame = Time.frameCount;
            
            if (isPressedThisFrame) return isPressedThisFrame;
            isPressedThisFrame = !AlternativeIsAxis ? Input.GetKeyDown(AlternativeKey) : isAxisTrue(AlternativeAxis);

            return isPressedThisFrame;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButton()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKey(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isTrue) return isTrue;
            isTrue = !AlternativeIsAxis ? Input.GetKey(AlternativeKey) : isAxisTrue(AlternativeAxis);
            return isTrue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isButtonUp()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKeyUp(PrimaryKey) : isAxisTrue(PrimaryAxis);
            if (isTrue) { wasPressed = false; return isTrue; }
            isTrue = !AlternativeIsAxis ? Input.GetKeyUp(AlternativeKey) : isAxisTrue(AlternativeAxis);
            return isTrue;
        }

        private bool isAxisTrue(string axisName)
        {
            if (string.IsNullOrEmpty(axisName)) return false;
            return Input.GetAxis(axisName) == AxisValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetInputName()
        {
            if (PrimaryIsAxis) return PrimaryAxis;
            else return PrimaryKey.ToString();
        }
    }
}