using UnityEngine;
using MFPS.PlayerController;

/// <summary>
/// Base class for the player controller
/// If you want to use your own player controller, inherited your script from this class
/// and implement the override functions and properties
/// use the default child script bl_FirstPersonController.cs as reference.
/// </summary>
public abstract class bl_FirstPersonControllerBase : bl_MonoBehaviour
{
    /// <summary>
    /// The player state has to be updated from the child script
    /// this always has to represent the current state of the player
    /// </summary>
    public PlayerState State 
    {
        get; 
        set; 
    } = PlayerState.Idle;

    /// <summary>
    /// Has to be assigned from the inherited class
    /// returns the current velocity of the player
    /// </summary>
    public abstract Vector3 Velocity
    {
        get;
        set;
    }

    /// <summary>
    /// Has to be assigned from the inherited class
    /// returns the current velocity magnitude of the player
    /// </summary>
    public abstract float VelocityMagnitude
    {
        get; 
        set;
    }

    /// <summary>
    /// returns true if the player is currently controlled
    /// by the user input, otherwise returns false.
    /// </summary>
    public abstract bool isControlable
    {
        get;
        set;
    }

    /// <summary>
    /// Has to be assigned from the inherited class
    /// returns true if the player is touching a surface
    /// Normally use CharacterController.IsGrounded
    /// </summary>
    public abstract bool isGrounded 
    { 
        get;
    }

    /// <summary>
    /// Use to access to variables of the default MFPS controller
    /// If you are using your own inherited class, you don't need this.
    /// </summary>
    private bl_FirstPersonController _mfpsController = null;
    public bl_FirstPersonController MFPSController
    {
        get
        {
            if (_mfpsController == null) TryGetComponent(out _mfpsController);
            return _mfpsController;
        }
    }

    /// <summary>
    /// Make the player jump
    /// </summary>
    public abstract void DoJump();

    /// <summary>
    /// Make the player slide
    /// </summary>
    public abstract void DoSlide();

    /// <summary>
    /// Active the player dropping mode
    /// </summary>
    public abstract void DoDrop();

    /// <summary>
    /// Active the player gliding mode
    /// </summary>
    public abstract void DoGliding();

    /// <summary>
    /// Force the player to stop at least one frame.
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// Handle the vertical jump when the player enters in a platform
    /// </summary>
    /// <param name="force">up force</param>
    public abstract void PlatformJump(float force);

    /// <summary>
    /// Update the mouse look movement
    /// You can call to manually update it.
    /// </summary>
    public abstract void UpdateMouseLook();

    /// <summary>
    /// Play once the footstep sound
    /// The clip should be played respecting to the surface where the player is 
    /// currently hitting
    /// </summary>
    public abstract void PlayFootStepSound();

    /// <summary>
    /// Get the MouseLook controller module
    /// </summary>
    /// <returns></returns>
    public abstract MouseLookBase GetMouseLook();

    /// <summary>
    /// Get the footstep controller module
    /// </summary>
    /// <returns></returns>
    public abstract bl_Footstep GetFootStep();

    /// <summary>
    /// Return the top speed of the player at a specific state
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public abstract float GetSpeedOnState(PlayerState playerState, bool includeModifiers = false);

    /// <summary>
    /// Returns the current speed of the player
    /// </summary>
    /// <returns></returns>
    public abstract float GetCurrentSpeed();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract PlayerRunToAimBehave GetRunToAimBehave();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract float GetHeadBobMagnitudes(bool verticalMagnitude);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract float GetSprintFov();
}