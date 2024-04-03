using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class bl_LocalizationUI : MonoBehaviour
{
    public string StringID = "";
    public string[] StringIDs;
    public string ExtraString = string.Empty;
    public UIType m_UIType = UIType.Text;
    [Header("Options")]
    public int StringCase = 0;
    public bool Plural = false;
    public bool Extra = false;
    public bool FormatedText = false;

    [HideInInspector] public bool ManuallyAssignId = false;
    [HideInInspector] public int _arrayID = 0;

#if LOCALIZATION
    private Text m_Text;
    private TextMeshProUGUI textTMP;
    private Dropdown m_Dropdown;
    private bool localized = false;
    private int localizedId = 0;
    private bool usingCustomFont = false;
    private TMP_FontAsset _defaultTMPFont;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (m_UIType == UIType.Text)
        {
            if (m_Text == null) { m_Text = GetComponent<Text>(); }
            if(textTMP == null) { textTMP = GetComponent<TextMeshProUGUI>(); }
        }
        else if (m_UIType == UIType.DropDown)
        {
            if(m_Dropdown == null) { m_Dropdown = GetComponent<Dropdown>(); }
        }
        bl_Localization.Instance.OnLanguageChange += OnChangeLanguage;
        if (!localized || localizedId != bl_Localization.Instance.CurrentLanguageID)
        {
            Localize();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_Localization.Instance.OnLanguageChange -= OnChangeLanguage;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnChangeLanguage(Dictionary<string, string> lang)
    {
        Localize();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Localize()
    {
        if (m_UIType == UIType.Text)
        {
            LocalizedText();
        }
        else if (m_UIType == UIType.DropDown)
        {
            LocalizedDropdown();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LocalizedText()
    {
        if (m_Text == null && textTMP == null) return;

        string t = bl_Localization.Instance.GetText(StringID);
        var language = bl_Localization.Instance.GetCurrentLanguage;
        if (Plural) { t += language.Text.PlurarLetter; }
        if (Extra) { t += ExtraString; }
        if (StringCase == 1)
        {
            t = t.ToUpper();
        }
        else if (StringCase == 2)
        {
            t = t.ToLower();
        }
        else if (StringCase == 3)
        {
            t = t[0].ToString().ToUpper() + t.ToLower().Substring(1, t.Length - 1);
        }
        if(m_Text != null) m_Text.text = t;
        if (textTMP != null)
        {
            if(language.Text.HasCustomFont() || usingCustomFont)
            {
                VerifyTextFont(language.Text.CustomFont);
            }
            textTMP.text = t;
        }
        localized = true;
        localizedId = bl_Localization.Instance.CurrentLanguageID;
    }

    /// <summary>
    /// 
    /// </summary>
    private void VerifyTextFont(TMP_FontAsset langFont)
    {
        if(langFont == null)
        {
            if (_defaultTMPFont == null) return;

            textTMP.font = _defaultTMPFont;
            usingCustomFont = false;
            return;
        }

        _defaultTMPFont = textTMP.font;
        usingCustomFont = true;
        textTMP.font = langFont;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LocalizedDropdown()
    {
        m_Dropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < StringIDs.Length; i++)
        {
            Dropdown.OptionData d = new Dropdown.OptionData();
            string t = bl_Localization.Instance.GetText(StringIDs[i]);
            if (StringCase == 1)
            {
                t = t.ToUpper();
            }
            else if (StringCase == 2)
            {
                t = t.ToLower();
            }
            else if (StringCase == 3)
            {
                t = t[0].ToString().ToUpper() + t.ToLower().Substring(1, t.Length - 1);
            }
            d.text = t;
            options.Add(d);
        }
        m_Dropdown.AddOptions(options);
    }
#endif

    [Serializable]
    public enum UIType
    {
        Text = 0,
        DropDown = 1,
    }
}