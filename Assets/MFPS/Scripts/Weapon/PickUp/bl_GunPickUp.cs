using System.Collections;
using UnityEngine;

/// <summary>
/// Default weapon pick up script
/// You can use your own script by inherited it from bl_GunPickUpBase
/// </summary>
public class bl_GunPickUp : bl_GunPickUpBase, IRayDetectable
{
    #region Public members
    public DetectMode m_DetectMode = DetectMode.Raycast;
    [HideInInspector]
    public bool PickupOnCollide = true;
    public float DestroyIn = 15f;
    public bl_EventHandler.UEvent onPickUp;
    #endregion

    #region Private members
    private bool isFocus = false;
    private bl_PlayerReferences localPlayerIn = null;
    private bool localInsideTrigger = false;
    private byte uniqueLocal = 0;
    #endregion
    
    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected) return;

        base.Awake();
        uniqueLocal = (byte)UnityEngine.Random.Range(0, 9998);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        if (AutoDestroy)
        {
            DestroyAfter(DestroyIn);
        }

        if (m_DetectMode == DetectMode.Trigger)
        {
            PickupOnCollide = false;
            // Delay before detect the throwed weapon again.
            yield return new WaitForSeconds(2f);
            PickupOnCollide = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!localInsideTrigger) return;

        bl_PickUpUIBase.Instance?.Hide();
        if (localPlayerIn != null)
        {
            if (m_DetectMode == DetectMode.Raycast)
            {
                if (localPlayerIn.cameraRay != null)
                {
                    localPlayerIn.cameraRay.SetActiver(false, uniqueLocal);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (!PickupOnCollide || bl_GameManager.Instance.GameMatchState == MatchState.Waiting)
            return;
        if (!GetGameMode.GetGameModeInfo().allowedPickupWeapons) return;
        // if it is not a local player, just ignore it.
        if (!c.isLocalPlayerCollider()) return;

        var playerReferences = c.GetComponent<bl_PlayerReferences>();
        if (playerReferences != null)
        {
            localInsideTrigger = true;
            bl_GunPickUpManagerBase.Instance.LastTrigger = this;
            localPlayerIn = playerReferences;

            if (m_DetectMode == DetectMode.Raycast)
            {
                if (playerReferences.cameraRay != null)
                {
                    playerReferences.cameraRay.SetActiver(true, uniqueLocal);
                }
            }
            else if (m_DetectMode == DetectMode.Trigger)
            {
                bl_PickUpUIBase.Instance?.OnOverWeapon(this);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTriggerExit(Collider c)
    {
        if (c.isLocalPlayerCollider() && localInsideTrigger)
        {
            localInsideTrigger = false;
            bl_PickUpUIBase.Instance?.Hide();
            if (localPlayerIn != null)
            {
                if (m_DetectMode == DetectMode.Raycast)
                {
                    if (localPlayerIn.cameraRay != null)
                    {
                        localPlayerIn.cameraRay.SetActiver(false, uniqueLocal);
                    }
                }
                localPlayerIn = null;
            }
        }
    } 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!localInsideTrigger) return;

        if (m_DetectMode == DetectMode.Trigger)
        {
            if (bl_GameInput.Interact() && bl_GunPickUpManagerBase.Instance.LastTrigger == this)
            {
                PickUp();
            }
        }
        else if (m_DetectMode == DetectMode.Raycast)
        {
            if (!localInsideTrigger || !isFocus) return;

            if (bl_GameInput.Interact())
            {
                PickUp();
            }
        }
    }

    /// <summary>
    /// Pickup the weapon
    /// </summary>
    public override void PickUp()
    {
        if (!GetGameMode.GetGameModeInfo().allowedPickupWeapons) return;

        bl_GunPickUpManagerBase.Instance?.SendPickUp(new bl_GunPickUpManagerBase.PickUpData()
        {
            Identifier = gameObject.name,
            GunID = GunID,
            Ammunition = Ammunition
        });

        bl_PickUpUIBase.Instance?.Hide();
        onPickUp?.Invoke();
    }

    /// <summary>
    /// Called when the player look at the weapon
    /// </summary>
    public void OnRayDetectedByPlayer()
    {
        isFocus = true;
        bl_PickUpUIBase.Instance?.OnOverWeapon(this);
    }

    /// <summary>
    /// Called when the player stop looking at the weapon
    /// </summary>
    public void OnUnDetectedByPlayer()
    {
        isFocus = false;
        bl_PickUpUIBase.Instance?.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    public override void DestroyAfter(float delay)
    {
        this.InvokeAfter(delay, () =>
        {
            Destroy(gameObject);
        });
    }

#if UNITY_EDITOR
    private SphereCollider SpheCollider;
    static Color _gizmoColor = new Color(0, 1, 0, 0.5f);
    private void OnDrawGizmos()
    {
        if (SpheCollider != null)
        {
            Gizmos.color = _gizmoColor;
            bl_UtilityHelper.DrawWireArc(SpheCollider.bounds.center, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
        }
        else
        {
            SpheCollider = GetComponent<SphereCollider>();
        }
    }
#endif

    [System.Serializable]
    public enum DetectMode
    {
        Raycast,
        Trigger,
    }
}