using MFPS.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class bl_NetworkGun : MonoBehaviour
{
    #region Public members
    [Header("Settings")]
    public bl_Gun LocalGun;
    public bool useCustomPlayerAnimations = false;
    public int customUpperAnimationID = 20;
    public string customFireAnimationName = "Fire";

    [Header("References")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public GameObject DesactiveOnOffAmmo;
    public Transform LeftHandPosition;

    /// <summary>
    /// Automatically setup this weapon when enabled
    /// </summary>
    public bool AutoSetup
    {
        get;
        set;
    }
    #endregion

    #region Private members
    private AudioSource Source;
    private int WeaponID = -1;
    [System.NonSerialized]
    public BulletData m_BulletData = new BulletData();
    Vector3 bulletPosition = Vector3.zero;
    Quaternion bulletRotation = Quaternion.identity;
    Transform Root; 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        SetupAudio();
        Root = transform.root;
    }

    /// <summary>
    /// Update type each is enable 
    /// </summary>
    void OnEnable()
    {
        if (AutoSetup) SetUpType();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpType()
    {
        PlayerSync?.SetNetworkWeapon(Info.Type, this);
        LocalGun?.customWeapon?.Initialitate(LocalGun);
    }

    /// <summary>
    /// Fire Sync in network player
    /// </summary>
    public void Fire(Vector3 hitPoint, Vector3 inacuracity)
    {
        if (LocalGun != null)
        {
            if (MuzzleFlash)
            {
                PlayMuzzleflash();
                bulletPosition = MuzzleFlash.transform.position;
            }
            else
            {
                bulletPosition = transform.position;      
            }

            bulletRotation = Quaternion.LookRotation(hitPoint - bulletPosition);
            //bullet info is set up in start function
            GameObject newBullet = bl_ObjectPoolingBase.Instance.Instantiate(LocalGun.BulletName, bulletPosition, bulletRotation); // create a bullet
            // set the gun's info into an array to send to the bullet
            m_BulletData.Damage = 0;
            m_BulletData.ImpactForce = 0;
            m_BulletData.Speed = LocalGun.bulletSpeed;
            m_BulletData.Inaccuracity = inacuracity;
            m_BulletData.DropFactor = LocalGun.bulletDropFactor;
            m_BulletData.Position = Root.position;
            m_BulletData.isNetwork = true;

            newBullet.GetComponent<bl_ProjectileBase>().InitProjectile(m_BulletData);
            PlayLocalFireAudio();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool FireCustomLogic(ExitGames.Client.Photon.Hashtable data)
    {
        if (LocalGun != null && LocalGun.customWeapon != null)
        {
            LocalGun.customWeapon.TPFire(this, data);
            return true;
        }
        return false;
    }

    /// <summary>
    /// if grenade 
    /// </summary>
    /// <param name="s"></param>
    public void GrenadeFire(float s,Vector3 position, Quaternion rotation, Vector3 direction)
    {
        if (LocalGun != null)
        {    
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            BulletData bulletData = new BulletData();    
            bulletData.SetInaccuracity(s, LocalGun.spreadMinMax.y);
            bulletData.Speed = LocalGun.bulletSpeed;
            bulletData.Position = Root.position;
            bulletData.isNetwork = true;

            var proRigid = newBullet.GetComponent<Rigidbody>();
            if (proRigid != null)
            {
                proRigid.AddForce(direction, ForceMode.Impulse);
            }
            
            newBullet.GetComponent<bl_ProjectileBase>().InitProjectile(bulletData); //bl_Projectile.cs
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// When is knife only reply sounds
    /// </summary>
    public void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void DesactiveGrenade(bool active,Material mat)
    {
        if(Info.Type != GunType.Grenade)
        {
            Debug.LogError("Gun type is not grenade, can't desactive it: " + Info.Type);
            return;
        }
        //when hide network gun / grenade we use method of change material to a invincible
        //due that if desactive the render cause animation  player broken.
        if (DesactiveOnOffAmmo != null)
        {
            DesactiveOnOffAmmo.SetActive(active);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayMuzzleflash()
    {
        if (MuzzleFlash == null) return;

        MuzzleFlash.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayLocalFireAudio()
    {
        Source.clip = LocalGun.FireSound;
        Source.spread = Random.Range(1.0f, 1.5f);
        Source.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupAudio()
    {
        Source = GetComponent<AudioSource>();
        Source.playOnAwake = false;
        Source.spatialBlend = 1;
        if (Info.Type != GunType.Knife)
        {
            Source.maxDistance = bl_AudioController.Instance.maxWeaponDistance;
        }
        else
        {
            Source.maxDistance = 5;
        }
        Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        Source.minDistance = bl_AudioController.Instance.maxWeaponDistance * 0.09f;
        Source.spatialize = true;
    }

    /// <summary>
    /// Returns the upper state ID
    /// That will be used to identify which upper body animations 
    /// will be played when this weapon is equipped
    /// </summary>
    /// <returns></returns>
    public int GetUpperStateID()
    {
        if (!useCustomPlayerAnimations || customUpperAnimationID <= 20) return (int)Info.Type;
        return customUpperAnimationID;
    }

    /// <summary>
    /// 
    /// </summary>
    public int GetWeaponID
    {
        get
        {
            if(WeaponID == -1)
            {
                if (LocalGun != null)
                {
                    WeaponID = LocalGun.GunID;
                }
            }
            return WeaponID;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bl_GunInfo _info = null;
    public bl_GunInfo Info
    {
        get
        {
            if (LocalGun != null)
            {
                if(_info == null) { _info = bl_GameData.Instance.GetWeapon(GetWeaponID); }
                return _info;
            }
            else
            {
                Debug.LogError("This tpv weapon: " + gameObject.name + " has not been defined!");
                return bl_GameData.Instance.GetWeapon(0);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bl_PlayerNetwork m_PS;
    private bl_PlayerNetwork PlayerSync
    {
        get
        {
            if(m_PS == null) { m_PS = transform.root.GetComponent<bl_PlayerNetwork>(); }
            return m_PS;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
      /*  if (Application.isPlaying)
            return;*/

        if (LeftHandPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LeftHandPosition.position, 0.02f);
            Gizmos.DrawWireSphere(LeftHandPosition.position, 0.05f);
        }
    }
#endif
}