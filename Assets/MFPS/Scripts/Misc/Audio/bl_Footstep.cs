using MFPS.Audio;
using UnityEngine;

public class bl_Footstep : MonoBehaviour
{
    public bl_FootStepsLibrary footStepsLibrary;
    public LayerMask surfaceLayers;

    [Header("Settings")]
    [Range(0, 1)] public float stepsVolume = 0.7f;
    [Range(0, 1)] public float stealthModeVolumeMultiplier = 0.1f;
    public float walkSpeed = 4;
    [Range(0.1f, 1)] public float walkStepRate = 0.44f;
    [Range(0.1f, 1)] public float runStepRate = 0.34f;
    [Range(0.1f, 1)] public float crouchStepRate = 0.54f;

    private AudioSource audioSource;
    private RaycastHit m_raycastHit;
    private Transform m_Transform;
    private string surfaceTag;
    private float nextRate = 0;
    private float lastStepTime = 0;
    private float volumeMultiplier = 1;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.maxDistance = bl_AudioController.Instance.maxFootstepDistance;
        audioSource.spatialBlend = 1;
        audioSource.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        audioSource.minDistance = bl_AudioController.Instance.maxFootstepDistance * 0.05f;
        audioSource.spatialize = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateStep(float speed)
    {
        if (Time.time - lastStepTime <= nextRate) return;

        DetectAndPlaySurface();

        if (speed > (walkSpeed + 1)) nextRate = runStepRate;
        else if (speed < (walkSpeed - 1)) nextRate = crouchStepRate;
        else nextRate = walkStepRate;

        lastStepTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DetectAndPlaySurface()
    {
        if (Physics.Raycast(m_Transform.position, -Vector3.up, out m_raycastHit, 5, surfaceLayers, QueryTriggerInteraction.Ignore))
        {
            surfaceTag = m_raycastHit.transform.tag;
            bl_TerrainSurfaces ts = m_raycastHit.transform.GetComponent<bl_TerrainSurfaces>();
            if (ts != null)
            {
                surfaceTag = ts.GetSurfaceTag(m_raycastHit.point);
            }
            PlayStepForTag(surfaceTag);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepForTag(string tag)
    {
        AudioClip step = GetStepSoundFor(tag);
        PlayStepSound(step);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepSound(AudioClip clip)
    {
        if (footStepsLibrary != null)
            audioSource.pitch = Random.Range(footStepsLibrary.pitchRange.x, footStepsLibrary.pitchRange.y);
        audioSource.clip = clip;
        audioSource.volume = stepsVolume * volumeMultiplier;
        audioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetVolumeMuliplier(float multiplier)
    {
        volumeMultiplier = multiplier;
    }

    /// <summary> 
    /// Get a random step sound clip for the given tag
    /// </summary>
    /// <returns></returns>
    public AudioClip GetStepSoundFor(string tag)
    {
        if (footStepsLibrary == null) return null;

        var stepsGroup = footStepsLibrary.GetGroupFor(tag);
        return stepsGroup.GetRandomClip();
    }
}