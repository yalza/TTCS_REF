using System.Collections.Generic;
using MFPS.Internal;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class bl_ScoreboardPopUpMenu : bl_ScoreboardPopUpMenuBase
{
    public List<MenuOptions> options;
    public UIListHandler listHandler;
    [SerializeField] private GameObject content = null;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        PrepareMenu();
        ((RectTransform)transform).position = Input.mousePosition;
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_ScoreboardPopUpMenuBase SetActive(bool active)
    {
        PrepareMenu();
        content.SetActive(active);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_ScoreboardPopUpMenuBase SetTargetPlayer(Player player)
    {
        TargetPlayer = player;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_ScoreboardPopUpMenuBase FilterMenuOptions(MenuFilter filter)
    {
        foreach (var item in options)
        {
            if (filter.IsBot)
            {
                if (!item.AllowedForBots)
                {
                    item.OptionButton.interactable = false;
                    continue;
                }
            }

            if (filter.IsLocalPlayer)
            {
                if (!item.AllowedForLocal)
                {
                    item.OptionButton.interactable = false;
                    continue;
                }
            }

            item.OptionButton.interactable = true;
        }
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrepareMenu()
    {
        if (listHandler.IsInitialize) return;

        listHandler.Prefab.SetActive(false);
        for (int i = 0; i < options.Count; i++)
        {
            var btn = listHandler.InstatiateAndGet<Button>();
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if(text != null)
            {
                text.text = options[i].Title;
            }
            options[i].OptionButton = btn;
            int id = i;
            btn.onClick.AddListener(() => { OnOptionClicked(id); });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override int AddButton(string buttonName, MenuFilter filter)
    {
        if (!listHandler.IsInitialize)
        {
            PrepareMenu();
        }

        var btn = listHandler.InstatiateAndGet<Button>();
        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = buttonName;
        }
        int id = listHandler.Count - 1;
        btn.onClick.AddListener(() => { OnOptionClicked(id); });

        options.Add(new MenuOptions()
        {
            Title = buttonName,
            OptionButton = btn,
            AllowedForBots = filter.IsBot,
            AllowedForLocal = filter.IsLocalPlayer
        });

        return id;
    }

    /// <summary>
    /// Called when the player click one of the popup buttons
    /// </summary>
    /// <param name="option"></param>
    public void OnOptionClicked(int option)
    {
        // The kick event is handled in bl_KickVotationUI.cs
        onScoreboardMenuAction?.Invoke(option);
        SetActive(false);
    }
}