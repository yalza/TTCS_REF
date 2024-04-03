using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Bob Settings", menuName = "MFPS/Weapons/Bob/Settings")]
public class bl_WeaponBobSettings : ScriptableObject
{
    [Header("Walk")]
    [Range(0.1f, 2)] public float WalkSpeedMultiplier = 1f;
    [Range(0, 15)] public float EulerZAmount = 5;
    [Range(0, 15)] public float EulerXAmount = 5;
    [Range(0, 0.2f)] public float WalkOscillationAmount = 0.04f;
    public float WalkLerpSpeed = 2;

    [Header("Sprint")]
    [Range(0.1f, 2)] public float RunSpeedMultiplier = 1f;
    [Range(0, 15)] public float RunEulerZAmount = 5;
    [Range(0, 15)] public float RunEulerXAmount = 5;
    [Range(0, 0.2f)] public float RunOscillationAmount = 0.1f;
    public float RunLerpSpeed = 4;

    [Header("Misc")]
    public float idleBobbingSpeed = 0.1f;
    public float AimIntensity = 0.01f;
    public float aimRotationIntensity = 0.33f;
    [LovattoToogle] public bool pitchTowardUp = true;

    public AnimationCurve rollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve pitchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}