using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MFPS.Tween
{
    public static class Easing
    {
        public static float Do(float t, EasingType type = EasingType.Quintic, EasingMode mode = EasingMode.InOut)
        {
            switch (type)
            {
                //-----------------------------------------------------------
                case EasingType.Exponential:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Exponential.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Exponential.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Exponential.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Exponential.OutIn(t);
                        default: return EasingFunctions.Exponential.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Quintic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quintic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quintic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quintic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quintic.OutIn(t);
                        default: return EasingFunctions.Quintic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Quadratic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quadratic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quadratic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quadratic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quadratic.OutIn(t);
                        default: return EasingFunctions.Quadratic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Sinusoidal:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Sinusoidal.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Sinusoidal.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Sinusoidal.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Sinusoidal.OutIn(t);
                        default: return EasingFunctions.Sinusoidal.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Bounce:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Bounce.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Bounce.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Bounce.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Bounce.InOut(t);
                        default: return EasingFunctions.Bounce.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Back:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Back.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Back.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Back.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Back.InOut(t);
                        default: return EasingFunctions.Back.InOut(t);
                    }
                //-----------------------------------------------------------
                default:
                case EasingType.Linear:
                    switch (mode)
                    {
                        case EasingMode.In:
                        case EasingMode.Out:
                        case EasingMode.InOut:
                        case EasingMode.OutIn:
                        default: return EasingFunctions.Linear.Identity(t);
                    }
            }
        }
    }

    internal static class EasingFunctions
    {
        public static class Exponential
        {
            public static float In(float t)
            {
                return Mathf.Pow(2, 10 * (t - 1));
            }

            public static float Out(float t)
            {
                return -Mathf.Pow(2, -10 * t) + 1;
            }

            public static float InOut(float t)
            {
                var x = 2 * t - 1;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Pow(2, 10 * x)
                    : 0.5f * -Mathf.Pow(2, -10 * x) + 1;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * (-Mathf.Pow(2, -20 * t) + 1)
                    : 0.5f * (Mathf.Pow(2, 20 * (t - 1)) + 1);
            }
        }
        public static class Linear
        {
            public static float Identity(float t)
            {
                return t;
            }
        }
        public static class Quintic
        {
            public static float In(float t)
            {
                return t * t * t * t * t;
            }

            public static float Out(float t)
            {
                float x = t - 1;

                return x * x * x * x * x + 1;
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 16 * t * t * t * t * t
                    : 16 * (t - 1) * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;
            }

            public static float OutIn(float t)
            {
                float x = 2 * t - 1;

                return 0.5f * (x * x * x * x * x + 1);
            }
        }
        public static class Quadratic
        {
            public static float In(float t)
            {
                return t * t;
            }

            public static float Out(float t)
            {
                return -t * (t - 2);
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 2 * t * t
                    : (-2 * t * t) + (4 * t) - 1;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? (-2 * t * t) + (2 * t)
                    : (2 * t * t) - (2 * t) + 1;
            }
        }
        public static class Sinusoidal
        {
            public static float In(float t)
            {
                const float HALF_PI = Mathf.PI / 2;

                return 1 - Mathf.Cos(t * HALF_PI);
            }

            public static float Out(float t)
            {
                const float HALF_PI = Mathf.PI / 2;

                return Mathf.Sin(t * HALF_PI);
            }

            public static float InOut(float t)
            {
                return (1 - Mathf.Cos(t * Mathf.PI)) / 2;
            }

            public static float OutIn(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                var x = 2 * t * HALF_PI;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Sin(x)
                    : 0.5f * (-Mathf.Cos(x - HALF_PI) + 2);
            }
        }

        public static class Bounce
        {
            public static float Out(float t)
            {
                {
                    if ((t /= 1) < (1 / 2.75f))
                    {
                        return (7.5625f * t * t);
                    }
                    else if (t < (2 / 2.75f))
                    {
                        return (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f);
                    }
                    else if (t < (2.5f / 2.75f))
                    {
                        return (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f);
                    }
                    else
                    {
                        return (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f);
                    }
                }
            }

            public static float In(float t)
            {
                return Out(1 - t);
            }

            public static float InOut(float t)
            {
                if (t < 0.5f)
                    return In(t * 2) * .5f;
                return Out(t * 2) * .5f + .5f;
            }
        }

        public static class Back
        {
            const float HALF_PI = 1.70158f;

            public static float In(float t)
            {
                return  (t /= 1) * t * ((HALF_PI + 1) * t - HALF_PI);
            }

            public static float Out(float t)
            {
                return ((t = t / 1 - 1) * t * ((HALF_PI + 1) * t + HALF_PI) + 1);
            }

            public static float InOut(float t, float s = 1.70158f)
            {
                if ((t /= 0.5f) < 1)
                    return 0.5f * (t * t * (((s *= (1.525f)) + 1) * t - s));
                return 0.5f * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2);
            }
        }
    }

    [Serializable]
    public enum EasingType
    {
        Exponential,
        Linear,
        Quintic,
        Quadratic,
        Sinusoidal,
        Bounce,
        Back,
    }

    [Serializable]
    public enum EasingMode
    {
        In,
        Out,
        InOut,
        OutIn,
    }
}