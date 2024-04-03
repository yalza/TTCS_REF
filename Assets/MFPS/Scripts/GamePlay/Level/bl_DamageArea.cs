using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_DamageArea : MonoBehaviour
{
    public AreaType m_Type = AreaType.OneTime;
    [Range(1, 100)] public int Damage = 5;

    private bool isPlayerCaused = false;
    private DamageData cacheInformation;

    private List<bl_PlayerHealthManagerBase> AllHitted = new List<bl_PlayerHealthManagerBase>();
    private bool isNetwork = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            var pdm = other.transform.GetComponent<bl_PlayerHealthManagerBase>();
            if (pdm != null)
            {
                if (m_Type == AreaType.Repeting)
                {
                    if (isPlayerCaused)
                    {
                        cacheInformation.Direction = transform.position;
                        int gi = bl_GameData.Instance.GetWeaponID("Molotov");
                        if (gi > 0)
                        {
                            cacheInformation.GunID = gi;
                        }

                        pdm.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageData = cacheInformation
                        });
                        AllHitted.Add(pdm);
                    }
                    else
                    {
                        pdm.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                        });
                    }
                }
                else if (m_Type == AreaType.OneTime)
                {
                    DamageData info = new DamageData();
                    info.Damage = Damage;
                    info.Direction = transform.position;
                    info.Cause = DamageCause.Fire;
                    pdm.DoDamage(info);
                }
            }
        }
        else if (other.transform.CompareTag("AI") && !isNetwork)
        {
            if (isPlayerCaused)
            {
                var ash = other.transform.root.GetComponent<bl_PlayerHealthManagerBase>();
                if (ash != null)
                {
                    if (!AllHitted.Contains(ash))
                    {
                        cacheInformation.Direction = transform.position;
                        int gi = bl_GameData.Instance.GetWeaponID("Molotov");
                        if (gi > 0)
                        {
                            cacheInformation.GunID = gi;
                        }
                        ash.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageData = cacheInformation
                        });
                        AllHitted.Add(ash);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            var pdm = other.transform.GetComponent<bl_PlayerHealthManagerBase>();
            if (pdm != null)
            {
                if (m_Type == AreaType.Repeting)
                {
                    pdm.CancelRepetingDamage();
                    AllHitted.Remove(pdm);
                }
            }
        }
        else if (other.transform.CompareTag("AI") && !isNetwork)
        {
            var ash = other.transform.root.GetComponent<bl_PlayerHealthManagerBase>();
            if (ash != null)
            {
                if (AllHitted.Contains(ash)) { AllHitted.Remove(ash); }
                if (m_Type == AreaType.Repeting)
                {
                    ash.CancelRepetingDamage();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        cacheInformation = null;
        foreach(var p in AllHitted)
        {
            if (p == null) continue;
            p.CancelRepetingDamage();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetInfo(string from, bool isNetwork)
    {
        isPlayerCaused = true;
        this.isNetwork = isNetwork;
        cacheInformation = new DamageData();
        cacheInformation.Actor = bl_PhotonNetwork.LocalPlayer;
        cacheInformation.Cause = DamageCause.Player;
        cacheInformation.From = from;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        SphereCollider c = GetComponent<SphereCollider>();
        if (c == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(c.center), c.radius);
        Gizmos.DrawWireSphere(transform.TransformPoint(c.center), c.radius);
    }


    [System.Serializable]
    public enum AreaType
    {
        Repeting,
        OneTime,
    }
}