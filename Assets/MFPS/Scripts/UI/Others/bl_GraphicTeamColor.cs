using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically apply a team color to an UI element 
/// Or team name to an UI Text
/// </summary>
public class bl_GraphicTeamColor : MonoBehaviour
{
    public Team team = Team.All;
    public bool applyColor = true;
    public bool applyTeamName = false;
    public bool toUpper = true;

    private Graphic graphic;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Apply()
    {
        if (graphic == null) { graphic = GetComponent<Graphic>(); }
        if (applyColor)
        {
            graphic.color = team.GetTeamColor();
        }

        if (applyTeamName)
        {
            var t = GetComponent<Text>();
            if (t != null) t.text = toUpper ? team.GetTeamName().ToUpper() : team.GetTeamName();
            else
            {
                var tmp = GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = toUpper ? team.GetTeamName().ToUpper() : team.GetTeamName();
            }
        }
    }
}