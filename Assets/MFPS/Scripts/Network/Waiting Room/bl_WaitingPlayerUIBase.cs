using Photon.Realtime;
using UnityEngine;

public abstract class bl_WaitingPlayerUIBase : MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract Player GetPlayer();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public abstract void SetInfo(Player player);

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdateState();
}