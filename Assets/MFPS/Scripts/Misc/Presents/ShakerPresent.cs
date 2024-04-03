using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Core.Motion
{
    [System.Serializable, CreateAssetMenu(fileName = "Shaker Present", menuName = "MFPS/Presents/Shaker", order = 302)]
    public class ShakerPresent : ScriptableObject
    {
        public ShakeMethod shakeMethod = ShakeMethod.PerlinNoise;
        [Range(0.01f, 5)]
        public float Duration = 0.4f;
        [Range(0, 50)]
        public float amplitude = 1;
        [Range(0.00001f, 0.99999f)]
        public float frequency = 0.98f;
        [Range(1, 4)]
        public int octaves = 2;
        [Range(0.00001f, 5)]
        public float persistance = 0.2f;
        [Range(0.00001f, 100)]
        public float lacunarity = 20;
        [Range(0.00001f, 0.99999f)]
        public float burstFrequency = 0.5f;
        [Range(0, 5)]
        public int burstContrast = 2;
        [Range(0.01f, 1)] public float fadeInTime = 0.2f;
        [LovattoToogle] public bool Loop = false;

        [HideInInspector] public float currentTime = 1;
        [HideInInspector] public float influence = 1;
        [HideInInspector] public bool starting = false;

        [Serializable]
        public enum ShakeMethod
        {
            PerlinNoise,
            RandomSet,
        }
    }
}