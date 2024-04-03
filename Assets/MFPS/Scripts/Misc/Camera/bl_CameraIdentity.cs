using System.Collections;
using UnityEngine;

/// <summary>
/// Camera Identity is used to identify which camera is rendering currently
/// instead of use the Unity Camera.current which is slow and can't be used in loop functions
/// You must attach this script in any camera that you want to use in game.
/// If you want to get the current camera that is rendered, use: bl_GameManager.Instance.CameraRendered.
/// </summary>
public class bl_CameraIdentity : MonoBehaviour
{
    private Camera m_Camera;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(Set());
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Set()
    {
        yield return new WaitForSeconds(0.5f);
        if (m_Camera == null)
        {
            m_Camera = GetComponent<Camera>();
        }
        if (m_Camera != null)
            bl_GameManager.Instance.CameraRendered = m_Camera;
    }

    /// <summary>
    /// 
    /// </summary>
    public static Camera CurrentCamera => bl_GameManager.Instance.CameraRendered;
}