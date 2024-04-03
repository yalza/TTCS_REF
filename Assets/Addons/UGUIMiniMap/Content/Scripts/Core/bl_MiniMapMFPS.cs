using UnityEngine;

public class bl_MiniMapMFPS : MonoBehaviour
{

    private bl_MiniMap miniMap;
    private bl_MiniMapCompass compass;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        TryGetComponent(out miniMap);
        TryGetComponent(out compass);
        miniMap.m_Canvas.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnLocalSpawn()
    {
         miniMap.m_Target = bl_GameManager.Instance.LocalPlayer;
        if (compass != null) { compass.Target = miniMap.m_Target.transform; }
        if (miniMap.m_Mode == bl_MiniMap.RenderMode.Mode3D) { miniMap.ConfigureCamera3D(); }
        miniMap.m_Canvas.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        miniMap.m_Canvas.enabled = false;
    }
}