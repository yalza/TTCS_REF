using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class bl_Projectile : bl_ProjectileBase
{
    #region Public members
    public ProjectileType m_Type = ProjectileType.Grenade;
    public ExplosionMethod explosionMethod = ExplosionMethod.Timer;
    public int TimeToExploit = 10;
    public GameObject explosion;   // instanced explosion 
    public TrailRenderer trailRenderer;
    #endregion

    #region Public properties
    public int ID { get; set; }
    public string mName { get; set; }
    public bool isNetwork { get; set; } = false;
    #endregion

    #region Private members
    private float speed = 75.0f;              // bullet speed
    private float impactForce;        // force applied to a rigid body object
    private float damage;
    private BulletData bulletData;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public override void InitProjectile(BulletData data)
    {
        bulletData = data;
        damage = data.Damage;
        impactForce = data.ImpactForce;
        speed = data.Speed;
        ID = data.WeaponID;
        mName = data.WeaponName;
        isNetwork = data.isNetwork;

        if (explosionMethod == ExplosionMethod.Timer)
        {
            InvokeRepeating(nameof(Counter), 1, 1);
        }

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
            if (bl_GameData.Instance.showProjectilesTrails)
            {
                this.InvokeAfter(0.2f, () =>
                {
                    trailRenderer.enabled = true;
                });
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCollisionEnter(Collision enterObject)
    {
        if (explosionMethod != ExplosionMethod.OnCollision)
            return;

        switch (enterObject.transform.tag)
        {
            case "Projectile":
                break;
            default:
                Destroy(gameObject, 0);
                ContactPoint contact = enterObject.contacts[0];
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, contact.normal);

                GameObject e = Instantiate(explosion, contact.point, rotation) as GameObject;
                if (m_Type == ProjectileType.Grenade)
                {
                    var blast = e.GetComponent<bl_ExplosionBase>();
                    if (blast != null)
                    {
                        blast.InitExplosion(bulletData, bulletData.MFPSActor);
                    }
                }
                else if (m_Type == ProjectileType.Molotov)
                {
                    var da = e.GetComponent<bl_DamageArea>();
                    if (bulletData.MFPSActor != null) da.SetInfo(bulletData.MFPSActor.Name, isNetwork);
                }
                if (enterObject.rigidbody)
                {
                    enterObject.rigidbody.AddForce(transform.forward * impactForce, ForceMode.Impulse);
                }
                break;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    void Counter()
    {
        TimeToExploit--;

        if (TimeToExploit <= 0)
        {
            GameObject e = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
            if (m_Type == ProjectileType.Grenade)
            {
                var blast = e.GetComponent<bl_ExplosionBase>();
                if (blast != null)
                {
                    var actor = bulletData.MFPSActor;
                    if (actor != null && actor.ActorView != null)
                    {
                        blast.InitExplosion(bulletData, actor);
                    }
                }
            }
            else if (m_Type == ProjectileType.Molotov)
            {

            }
            CancelInvoke();
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public enum ProjectileType
    {
        Grenade,
        Molotov,
        Smoke,
    }

    [System.Serializable]
    public enum ExplosionMethod
    {
        Timer,
        OnCollision
    }
}