using System;
using UnityEngine;
using System.Collections.Generic;
using MFPS.Internal.Scriptables;
using MFPS.Runtime.Level;

namespace MFPS.PlayerController
{
    [Serializable]
    public class MouseLook : MouseLookBase
    {
        #region Public members
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        #endregion

        #region Private members
        private Quaternion m_CharacterTargetRot, m_CameraTargetRot;
        private bool InvertVertical, InvertHorizontal;
        private float verticalRotation, horizontalRotation;
        private float sensitivity, aimSensitivity = 3f;
        private Quaternion extraRotation = Quaternion.identity;
        private Transform m_CameraTransform, m_CharacterBody;
        private List<float> rotArrayX = new List<float>();
        private float rotAverageX;
        private List<float> rotArrayY = new List<float>();
        private float rotAverageY;
        private bool ClampHorizontal = false;
        private Vector2 horizontalClamp = new Vector2(-360, 360);
        private bl_PlayerReferences playerReferences;
        private MouseLookSettings lookSettings;
        private float tiltValue, tiltTarget = 0;
        private float verticalOffset = 0;
        #endregion

        public float CurrentSensitivity { get; set; } = 3;
        public bool onlyCameraTransform { get; set; } = false;

        /// <summary>
        ///  Initialize the camera controller with the character initial rotation.
        /// </summary>
        public override void Init(Transform character, Transform camera)
        {
            m_CameraTransform = camera;
            m_CharacterBody = character;
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            FetchSettings();
            CurrentSensitivity = sensitivity;
            playerReferences = character.GetComponent<bl_PlayerReferences>();
            lookSettings = bl_GameData.Instance.mouseLookSettings;
        }

        /// <summary>
        /// Updates the character and camera rotation based on the player input.
        /// </summary>
        public override void UpdateLook(Transform character, Transform camera, bl_Ladder ladder = null)
        {
            // When use a mobile device or Unity Remote
            if (bl_UtilityHelper.isMobile)
            {
#if MFPSM
                Vector2 input = bl_TouchPad.Instance.GetInput(CurrentSensitivity);
                input.x = !InvertHorizontal ? input.x : (input.x * -1f);
                input.y = InvertVertical ? (input.y * -1f) : input.y;

                Move(input.x, input.y, character, camera, ladder);
#endif
            }
            else
            {
                if (!bl_RoomMenu.Instance.isCursorLocked)
                    return;

                Move(bl_GameInput.MouseX, bl_GameInput.MouseY, character, camera, ladder);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Move(float inputX, float inputY, Transform character, Transform camera, bl_Ladder ladder)
        {
            if (ladder == null)
            {
                horizontalRotation = inputX * CurrentSensitivity;
                horizontalRotation = (InvertHorizontal) ? (horizontalRotation * -1f) : horizontalRotation;

                if (lookSettings.useSmoothing)
                {
                    rotArrayX.Add(horizontalRotation);
                    if ((float)rotArrayX.Count >= lookSettings.framesOfSmoothing)
                    {
                        rotArrayX.RemoveAt(0);
                    }
                    for (int i = 0; i < rotArrayX.Count; i++)
                    {
                        rotAverageX += rotArrayX[i];
                    }
                    rotAverageX /= rotArrayX.Count;
                }
                else rotAverageX = horizontalRotation;

                m_CharacterTargetRot *= Quaternion.Euler(0f, rotAverageX, 0f);
            }
            else
            {
                Vector3 direction = ladder.GetLookDirection();
                direction.y = 0;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                m_CharacterTargetRot = Quaternion.Slerp(m_CharacterTargetRot, lookRotation, Time.deltaTime * 5);
            }

            verticalRotation = inputY * CurrentSensitivity;
            verticalRotation = (InvertVertical) ? (verticalRotation * -1f) : verticalRotation;
            if (lookSettings.useSmoothing)
            {
                rotArrayY.Add(verticalRotation);
                if ((float)rotArrayY.Count >= lookSettings.framesOfSmoothing)
                {
                    rotArrayY.RemoveAt(0);
                }
                for (int j = 0; j < rotArrayY.Count; j++)
                {
                    rotAverageY += rotArrayY[j];
                }
                rotAverageY /= rotArrayY.Count;
            }
            else rotAverageY = verticalRotation;

            if (!onlyCameraTransform)
                m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, 0f, 0);
            else
                m_CameraTargetRot *= Quaternion.Euler(-rotAverageY, horizontalRotation, 0);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (ClampHorizontal)
            {
                var re = m_CharacterTargetRot.eulerAngles;
                re.y = bl_MathUtility.Clamp360Angle(re.y, horizontalClamp.x, horizontalClamp.y);
                m_CharacterTargetRot = Quaternion.Euler(re);
            }

            if (lookSettings.lerpMovement)
            {
                if (character != null && !onlyCameraTransform) { character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, lookSettings.smoothTime * Time.deltaTime); }
                if (camera != null) { camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, lookSettings.smoothTime * Time.deltaTime); }
            }
            else
            {
                if (!onlyCameraTransform)
                {
                    character.localRotation = m_CharacterTargetRot;
                }

                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot * extraRotation, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            tiltValue = Mathf.Lerp(tiltValue, tiltTarget, Time.deltaTime * 4);
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetVerticalOffset(float amount)
        {
            verticalOffset = amount;
            extraRotation = Quaternion.Euler(amount, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CombineVerticalOffset()
        {
            m_CameraTargetRot *= extraRotation;
            verticalOffset = 0;
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);

            if (clampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
        }

        /// <summary>
        /// Don't rotate the character body but only the Camera/Head
        /// </summary>
        public void UseOnlyCameraRotation()
        {
            onlyCameraTransform = true;
        }

        /// <summary>
        /// Port the Current Camera Rotation to separate the vertical and horizontal rotation in the body and head
        /// horizontal rotation for the body and vertical for the camera/head
        /// That should only be called when OnlyCameraRotation was used before.
        /// </summary>
        public void PortBodyOrientationToCamera()
        {
            onlyCameraTransform = false;
            Vector3 direction = Vector3.zero;
            direction.y = m_CameraTransform.eulerAngles.y;
            m_CharacterBody.rotation = Quaternion.Euler(direction);

            direction = Vector3.zero;
            direction.x = m_CameraTransform.localEulerAngles.x;
            m_CameraTransform.localRotation = Quaternion.Slerp(m_CameraTransform.localRotation, Quaternion.Euler(direction), 1);

            m_CharacterTargetRot = m_CharacterBody.localRotation;
            m_CameraTargetRot = m_CameraTransform.localRotation;

            m_CameraTransform.localRotation *= extraRotation;
        }

        /// <summary>
        /// Forces the character to look at a position in the world.
        /// </summary>
        public override void LookAt(Transform reference, bool extrapolate = true)
        {
            m_CharacterTargetRot = Quaternion.Euler(0f, reference.eulerAngles.y, 0f);
            Quaternion relative = Quaternion.Inverse(Quaternion.identity) * reference.rotation;
            m_CameraTargetRot = Quaternion.Euler(relative.eulerAngles.x, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LookAt(Vector3 direction, bool extrapolate = true, float influence = 1)
        {
            Quaternion rotate = Quaternion.LookRotation(direction);
            rotate = Quaternion.Slerp(m_CameraTransform.rotation, rotate, influence);

            m_CharacterTargetRot = Quaternion.Euler(0f, rotate.eulerAngles.y, 0f);
            m_CameraTargetRot = Quaternion.Euler(rotate.eulerAngles.x, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        public override void SetTiltAngle(float angle)
        {
            tiltTarget = angle;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchSettings()
        {
            sensitivity = (float)bl_MFPS.Settings.GetSettingOf("Sensitivity");
            aimSensitivity = (float)bl_MFPS.Settings.GetSettingOf("Aim Sensitivity");
            InvertHorizontal = (bool)bl_MFPS.Settings.GetSettingOf("MouseH Invert");
            InvertVertical = (bool)bl_MFPS.Settings.GetSettingOf("MouseV Invert");
            CurrentSensitivity = sensitivity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAimChange(bool isAiming)
        {
            CurrentSensitivity = isAiming ? sensitivity * aimSensitivity : sensitivity;

            if (isAiming && lookSettings.aimSensitivityAdjust == MouseLookSettings.AimSensitivityAdjust.Relative)
                AdjustSensitivityBasedOnFOV();
        }

        /// <summary>
        /// Adjust the mouse sensitivity based on the camera field of view.
        /// </summary>
        public void AdjustSensitivityBasedOnFOV()
        {
            if (playerReferences == null) return;

            float fovPercentage = playerReferences.playerCamera.fieldOfView / playerReferences.DefaultCameraFOV;
            CurrentSensitivity = CurrentSensitivity * fovPercentage;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ClampHorizontalRotation(float min, float max)
        {
            horizontalClamp = new Vector2(min, max);
            ClampHorizontal = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UnClampHorizontal() => ClampHorizontal = false;

        public float VerticalAngle => m_CameraTransform.localEulerAngles.x;

        Quaternion ClampRotationAroundXAxis(Quaternion q) => bl_MathUtility.ClampRotationAroundAxis(q, MinimumX, MaximumX, UnityEngine.Animations.Axis.X);
        Quaternion ClampRotationAroundYAxis(Quaternion q) => bl_MathUtility.ClampRotationAroundAxis(q, horizontalClamp.x, horizontalClamp.y, UnityEngine.Animations.Axis.Y);

        public override Vector2 HorizontalLimits { get => horizontalClamp; set => horizontalClamp = value; }
        public override Vector2 VerticalLimits
        {
            get => new Vector2(MinimumX, MaximumX);
            set
            {
                MinimumX = value.x;
                MaximumX = value.y;
            }
        }
    }
}