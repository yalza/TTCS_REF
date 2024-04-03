using UnityEngine;
using TMPro;

public class bl_FrameRate : bl_MonoBehaviour
{
    [Range(0.1f, 1)] public float updateInterval = 0.4f;
    public string textFormat = "<b>FPS:</b> {0}";
    public TextMeshProUGUI TextUI = null;

    private string framerate;
    private float timeleft;
    private int rate = 0;
    private float accum;
    private int frames;
    private bool countFPS = true;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        timeleft = updateInterval;
        OnSettingsChanged();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onGameSettingsChange += OnSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onGameSettingsChange -= OnSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnSettingsChanged()
    {
        countFPS = (bool)bl_MFPS.Settings.GetSettingOf("Show Frame Rate");
        if (TextUI != null) TextUI.gameObject.SetActive(countFPS);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (TextUI == null || !countFPS) return;

        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;
        if (timeleft <= 0)
        {
            rate = Mathf.FloorToInt(accum / frames);
            framerate = rate.ToString();
            timeleft = updateInterval;
            accum = 0;
            frames = 0;
        }
        TextUI.text = string.Format(textFormat, framerate);
    }
}