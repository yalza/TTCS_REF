using UnityEngine;
using System;

namespace MFPS.Core.Motion
{
    [CreateAssetMenu(fileName = "CameraMotionData", menuName = "MFPS/Camera/MotionData")]
    public class FirstPersonMotionData : ScriptableObject
    {
        public string Name = "Motion Name";
        [Header("Head Rotation")]
        public MotionCurves HeadAnimation;

        [Header("Arms Rotation")]
        public MotionCurves ArmsAnimation;

        [Range(0.1f, 10)] public float Duration = 2;

        /// <summary>
        /// 
        /// </summary>
        public void ApplyFrame(float time, Vector3 headVector, Vector3 armsVector)
        {
            HeadAnimation.ApplyFrame(headVector, time);
            ArmsAnimation.ApplyFrame(armsVector, time);
        }

        [Serializable]
        public class MotionCurves
        {
            public AnimationCurve X = AnimationCurve.EaseInOut(0, 0, 1, 0);
            public AnimationCurve Y = AnimationCurve.EaseInOut(0, 0, 1, 0);
            public AnimationCurve Z = AnimationCurve.EaseInOut(0, 0, 1, 0);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="time"></param>
            /// <returns></returns>
            public Vector3 Vector(float time = 0) => new Vector3(X.Evaluate(time), Y.Evaluate(time), Z.Evaluate(time));

            /// <summary>
            /// Get the final rotation of the animation curves
            /// </summary>
            /// <returns></returns>
            public Vector3 FinalVector()
            {
                return new Vector3(X.Evaluate(1), Y.Evaluate(1), Z.Evaluate(1));
            }

            /// <summary>
            /// 
            /// </summary>
            public void ApplyFrame(Vector3 vector, float time)
            {
                SetKey(X, time, vector.x);
                SetKey(Y, time, vector.y);
                SetKey(Z, time, vector.z);
            }

            /// <summary>
            /// 
            /// </summary>
            private void SetKey(AnimationCurve curve, float time, float value)
            {
                for (int i = curve.keys.Length - 1; i >= 0; i--)
                {
                    if (curve.keys[i].time == time)
                    {
                        curve.keys[i].value = value;
                        return;
                    }
                }
                curve.AddKey(time, value);
            }
        }
    }
}