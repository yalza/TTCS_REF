using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_ThrowKits : bl_MonoBehaviour
{
    [Header("SETTINGS")]
    public int AmountOfKits = 3;
    /// <summary>
    /// force when it is instantiated prefabs
    /// </summary>
    public float ForceImpulse = 500;
    [Range(1, 4)] public float CallDelay = 1.4f;
    public float dropInstanceDistance = 1f;

    [Header("REFERENCES")]
    /// <summary>
    /// Prefabs for instantiate
    /// </summary>
    public GameObject DropCallerPrefab;
    public AudioClip SpawnSound;
    public PlayerClass CurrentPlayerClass { get; set; } = PlayerClass.Assault;


#if !ACTK_IS_HERE
    private int remaingKits;
#else
    private ObscuredInt remaingKits;
#endif

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        CurrentPlayerClass = PlayerClass.Assault.GetSavePlayerClass();
        remaingKits = AmountOfKits;
    }

#if MFPSM
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_TouchHelper.OnKit += OnMobileClick;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_TouchHelper.OnKit -= OnMobileClick;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMobileClick()
    {
        DispatchThrow();
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (bl_GameData.Instance.isChating) return;
       

        if (bl_GameInput.ThrowKit())
        {
            DispatchThrow();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DispatchThrow()
    {
        if (remaingKits <= 0 || DropCallerPrefab == null) return;

        int id = 0;
        if ((CurrentPlayerClass == PlayerClass.Assault || CurrentPlayerClass == PlayerClass.Recon))
        {
            id = 1;
        }

#if CLASS_CUSTOMIZER
        id = bl_ClassManager.Instance.ClassKit;
#endif

        ThrowCaller(id);
    }

    /// <summary>
    /// 
    /// </summary>
    void ThrowCaller(int id)
    {
        remaingKits--;
        Vector3 point = (transform.position + (PlayerReferences.PlayerCameraTransform.forward * dropInstanceDistance)) + transform.up;
        GameObject kit = Instantiate(DropCallerPrefab, point, Quaternion.identity) as GameObject;
        kit.GetComponent<bl_DropCallerBase>().SetUp(new bl_DropCallerBase.DropData()
        {
            KitID = id,
            Delay = CallDelay
        });
        kit.GetComponent<Rigidbody>().AddForce(bl_MFPS.LocalPlayerReferences.PlayerCameraTransform.forward * ForceImpulse);
        if (SpawnSound)
        {
            AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    public void AddKits(int amount)
    {
        remaingKits += amount;
    }

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}