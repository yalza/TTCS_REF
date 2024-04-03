using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Default weapon walk bob movement
/// You use your custom weapon script by inherited your script from the base class bl_WeaponBobBase.cs
/// NOTE: this default script also control the head bob movement, so if you replace this with you own script
/// make sure you also handle the head bob.
/// </summary>
public class bl_WeaponBob : bl_WeaponBobBase
{
    public bl_WeaponBobSettings settings;

    #region Private members
    Vector3 midpoint;
    Vector3 localRotation;
    float timer = 0.0f;
    float lerp = 2;
    float bobbingSpeed;
    bl_FirstPersonControllerBase motor;
    float BobbingAmount;
    float tempWalkSpeed = 0;
    float tempRunSpeed = 0;
    float tempIdleSpeed = 0;
    float waveslice = 0.0f;
    float waveslice2 = 0.0f;
    float eulerZ = 0;
    float eulerX = 0;
    private bool rightFoot = false;
    Vector3 currentPosition, currentRotation = Vector3.zero;
    Vector3 bobEuler = Vector3.zero;
    private Action<PlayerState> AnimateCallback = null;
    private bl_PlayerReferences playerReferences;
    private bool useAnimation = false;
    private Quaternion bobRotation = Quaternion.identity;
    private float rotationIntensity = 1;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
        motor = playerReferences.firstPersonController;
        midpoint = transform.localPosition;
        localRotation = transform.localEulerAngles;
        Intensitity = 1;
        rotationIntensity = 1;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalAimChanged += OnLocalAimChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalAimChanged -= OnLocalAimChange;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (motor == null) return;

        if (useAnimation)
        {
            if (AnimateCallback == null) return;

            AnimateCallback.Invoke(motor.State);
            MoveToDefault();
            CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, midpoint, Time.deltaTime * lerp);
        }
        StateControl();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        Movement();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Stop()
    {
        bobbingSpeed = tempIdleSpeed;
        BobbingAmount = settings.WalkOscillationAmount * 0.1f;
        lerp = settings.WalkLerpSpeed;
        eulerZ = settings.EulerZAmount;
        eulerX = settings.EulerXAmount;
        MoveToDefault();
    }

    /// <summary>
    /// 
    /// </summary>
    void StateControl()
    {
        if (motor.State == PlayerState.Jumping || motor.State == PlayerState.InVehicle) return;
        if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Running)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = settings.WalkOscillationAmount;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
        else if (motor.State == PlayerState.Running)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = settings.RunOscillationAmount;
            lerp = settings.RunLerpSpeed;
            eulerZ = settings.RunEulerZAmount;
            eulerX = settings.RunEulerXAmount;
        }

        if (motor.State != PlayerState.Running && motor.VelocityMagnitude < 0.1f || !bl_RoomMenu.Instance.isCursorLocked)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = settings.WalkOscillationAmount * 0.1f;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Movement()
    {
        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if (tempIdleSpeed != settings.idleBobbingSpeed)
        {
            tempWalkSpeed = motor.GetCurrentSpeed() * 0.06f * settings.WalkSpeedMultiplier;
            tempRunSpeed = motor.GetCurrentSpeed() * 0.03f * settings.RunSpeedMultiplier;
            tempIdleSpeed = settings.idleBobbingSpeed;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer = timer - (Mathf.PI * 2);
        }

        float w1v = (waveslice + 1) / 2;
        w1v = settings.pitchCurve.Evaluate(w1v);
        waveslice = Mathf.Lerp(-1, 1, w1v);

        float w2v = (waveslice2 + 1) / 2;
        w2v = settings.rollCurve.Evaluate(w2v);
        waveslice2 = Mathf.Lerp(-1, 1, w2v);

        ApplyMovement();
        UpdateFootStep();
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMovement()
    {
        if (useAnimation) return;
        float time = Time.smoothDeltaTime;
        if (waveslice != 0)
        {
            float verticalMovement = waveslice * BobbingAmount * Intensitity;
            if (!settings.pitchTowardUp) verticalMovement = -verticalMovement;
            float horizontalMovement = waveslice2 * BobbingAmount * Intensitity;
            float zFlow = waveslice2 * eulerZ * rotationIntensity;
            float xFlow = waveslice * eulerX * rotationIntensity;

            if (motor.isGrounded && motor.State != PlayerState.InVehicle)
            {
                //if player is moving
                if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Idle)
                {
                    currentPosition.Set(midpoint.x + horizontalMovement, midpoint.y + verticalMovement, midpoint.z);
                    currentRotation.Set(localRotation.x + xFlow, localRotation.y, localRotation.z + zFlow);
                    bobEuler.Set((xFlow * 0.33f) * motor.GetHeadBobMagnitudes(true), 0, zFlow * motor.GetHeadBobMagnitudes(false));
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
                    bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(bobEuler), time * lerp);
                }
                else//if player is idle
                {
                    currentPosition.Set(midpoint.x, midpoint.y + verticalMovement, midpoint.z);
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
                    bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(Vector3.zero), time * lerp);
                }
            }
            else
            {
                //Player not moving
                MoveToDefault();
            }
        }
        else
        {
            //Player not moving
            MoveToDefault();
        }
        CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, currentPosition, time * lerp);
        playerReferences.cameraMotion.AddCameraRotation(bobRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void MoveToDefault()
    {
        currentPosition = midpoint;
        CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), Time.smoothDeltaTime * 12);
        bobRotation = Quaternion.Slerp(bobRotation, Quaternion.Euler(Vector3.zero), Time.smoothDeltaTime * lerp);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateFootStep()
    {
        if (!motor.isGrounded || motor.State == PlayerState.InVehicle) return;

        if (motor.VelocityMagnitude > 0.1f)
        {
            if (waveslice2 >= 0.97f && !rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = true;
            }
            else if (waveslice2 <= (-0.97f) && rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aiming"></param>
    void OnLocalAimChange(bool aiming)
    {
        Intensitity = aiming ? settings.AimIntensity : 1;
        rotationIntensity = aiming ? settings.aimRotationIntensity : 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="useAnim"></param>
    public override void AnimatedThis(Action<PlayerState> callback, bool useAnim)
    {
        AnimateCallback = callback;
        useAnimation = useAnim;
    }


#if ENABLE_SIMULATION
    public float simulationSpeed = 0;
    public bool isSimulating = false;

    public void SimulateMovement()
    {
        float time = 0.018f;

        if (simulationSpeed > 0.1f && simulationSpeed <= 4)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = settings.WalkOscillationAmount;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
        else if (simulationSpeed > 4)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = settings.RunOscillationAmount;
            lerp = settings.RunLerpSpeed;
            eulerZ = settings.RunEulerZAmount;
            eulerX = settings.RunEulerXAmount;
        }

        if (simulationSpeed < 0.1f)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = settings.WalkOscillationAmount * 0.1f;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }

        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if (tempIdleSpeed != settings.idleBobbingSpeed)
        {
            tempWalkSpeed = simulationSpeed * 0.06f * settings.WalkSpeedMultiplier;
            tempRunSpeed = simulationSpeed * 0.03f * settings.RunSpeedMultiplier;
            tempIdleSpeed = settings.idleBobbingSpeed;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;

        float w1v = (waveslice + 1) / 2;
        w1v = settings.pitchCurve.Evaluate(w1v);
        waveslice = Mathf.Lerp(-1, 1, w1v);

        float w2v = (waveslice2 + 1) / 2;
        w2v = settings.rollCurve.Evaluate(w2v);
        waveslice2 = Mathf.Lerp(-1, 1, w2v);

        if (waveslice != 0)
        {
            float verticalMovement = -(waveslice * BobbingAmount * Intensitity);
            float horizontalMovement = waveslice2 * BobbingAmount * Intensitity;
            float zFlow = waveslice2 * eulerZ;
            float xFlow = waveslice * eulerX;

            //if player is moving
            if (simulationSpeed > 0.1f)
            {
                currentPosition.Set(midpoint.x + horizontalMovement, midpoint.y + verticalMovement, currentPosition.z);
                currentRotation.Set(localRotation.x + xFlow, localRotation.y, localRotation.z + zFlow);
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
            }
            else//if player is idle
            {
                currentPosition.Set(midpoint.x, midpoint.y + verticalMovement, currentPosition.z);
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
            }
        }
        else
        {
            //Player not moving
            MoveToDefault();
        }

        CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, currentPosition, time * lerp);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_WeaponBob))]
public class bl_WeaponEditorBob : Editor
{
    bl_WeaponBob script;

    private void OnEnable()
    {
        script = (bl_WeaponBob)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(5);
        SerializedProperty so = serializedObject.FindProperty("settings");
        EditorGUILayout.PropertyField(so);
        if (so != null && so.objectReferenceValue != null)
        {
            var editor = Editor.CreateEditor(so.objectReferenceValue);
            if (editor != null)
            {
                EditorGUILayout.BeginVertical("box");
                editor.DrawDefaultInspector();
                EditorGUILayout.EndVertical();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
#if ENABLE_SIMULATION
        if (!script.isSimulating)
        {
            if (GUILayout.Button("Simulate"))
            {
                script.isSimulating = true;
                script.Init();
                EditorApplication.update += OnEditorUpdate;
            }
        }
        else
        {
            script.simulationSpeed = EditorGUILayout.Slider("Simulation Speed", script.simulationSpeed, 0, 8);
            if (GUILayout.Button("Stop Simulate"))
            {
                EditorApplication.update -= OnEditorUpdate;
                script.isSimulating = false;
            }
        }
#endif
    }

#if ENABLE_SIMULATION
    void OnEditorUpdate()
    {
        script.SimulateMovement();
    }
#endif
}
#endif