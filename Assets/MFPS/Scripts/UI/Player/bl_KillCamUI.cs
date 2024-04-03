using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class bl_KillCamUI :  bl_KillCamUIBase
{
    [SerializeField] private GameObject content = null;
    [SerializeField] private TextMeshProUGUI KillerNameText = null;
    [SerializeField] private TextMeshProUGUI KillerHealthText = null;
    [SerializeField] private TextMeshProUGUI GunNameText = null;
    [SerializeField] private TextMeshProUGUI respawnCountdown = null;
    [SerializeField] private Image GunImage = null;
    public Image levelIcon;
    public TextMeshProUGUI KillCamSpectatingText;

    /// <summary>
    /// 
    /// </summary>
    public override void Show(bl_KillCamBase.KillCamInfo kcinfo)
{
        var killer = kcinfo.TargetName;
        if (string.IsNullOrEmpty(killer) && kcinfo.Target != null)
        {
            killer = kcinfo.Target.name;
        }
        
        content.SetActive(true);
        bl_GunInfo info = bl_GameData.Instance.GetWeapon(kcinfo.GunID);
        GunImage.sprite = info.GunIcon;
        GunNameText.text = info.Name.ToUpper();
        killer = killer.Replace("(die)", "");
        KillerNameText.text = killer;
        KillCamSpectatingText.text = string.Format("<size=8>{0}:</size>\n{1}", bl_GameTexts.Spectating.Localized(26).ToUpper(), killer);

        levelIcon.gameObject.SetActive(false);
        StartCoroutine(RespawnCountDown());
        MFPSPlayer actor = bl_GameManager.Instance.FindActor(killer);
        if(actor != null)
        {

            var pdm = actor.Actor.GetComponent<bl_PlayerHealthManagerBase>();
            int health = Mathf.FloorToInt(pdm.GetHealth());
            if (pdm != null) { KillerHealthText.text = string.Format("HEALTH: {0}", health); }

            if (actor.isRealPlayer)
            {
#if LM
                if (actor.ActorView != null)
                {
                    var level = bl_LevelManager.Instance.GetPlayerLevelInfo(actor.ActorView.Owner);
                    levelIcon.sprite = level.Icon;
                    levelIcon.gameObject.SetActive(true);
                }
#endif
            }
            else
            {
#if LM
                if (actor.GetBotStats() != null)
                {
                    var level = bl_LevelManager.Instance.GetLevel(actor.GetBotStats().Score);
                    levelIcon.sprite = level.Icon;
                    levelIcon.gameObject.SetActive(true);
                }
#endif
            }
        }
        else
        {
            KillerHealthText.text = string.Empty;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator RespawnCountDown()
    {
        float d = 0;
        float rt = bl_GameData.Instance.PlayerRespawnTime;
        while (d < 1)
        {
            d += Time.deltaTime / rt;
            int remaing = Mathf.FloorToInt(rt * (1 - d));
            remaing = Mathf.Max(0, remaing);
            respawnCountdown.text = string.Format(bl_GameTexts.RespawnIn.Localized(38), remaing);
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Hide()
    {
        content.SetActive(false);
    }
}