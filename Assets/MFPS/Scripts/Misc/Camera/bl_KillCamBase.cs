using Photon.Realtime;
using UnityEngine;

public abstract class bl_KillCamBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public abstract bl_KillCamBase SetTarget(KillCamInfo info);

    private static bl_KillCamBase _killcam;
    public static bl_KillCamBase Instance
    {
        get
        {
            if (_killcam == null) _killcam = bl_SpawnPointManager.Instance.killCameraInstance;
            return _killcam;
        }
    }

    public struct KillCamInfo
    {
        public Transform Target;
        public string TargetName;
        public int GunID;//with which the player was terminated
        public Transform FallbackTarget;
        public Player RealPlayer;
    }
}