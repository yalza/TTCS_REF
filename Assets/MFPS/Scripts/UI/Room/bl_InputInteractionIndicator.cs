using System;
using UnityEngine;
using TMPro;

public class bl_InputInteractionIndicator : MonoBehaviour
{
    [LovattoToogle] public bool forceUpperCase = true;
    [SerializeField] private GameObject content = null;
    [SerializeField] private TextMeshProUGUI inputNameText = null;
    [SerializeField] private TextMeshProUGUI descriptionText = null;

    private Action onClickCallback;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDie;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDie;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SetActive(bool active)
    {
        if (Instance == null) return;

        Instance.content.SetActive(active);
    }

    /// <summary>
    /// Set active will only execute if the current text showing is the same
    /// as the provided.
    /// </summary>
    public static void SetActiveIfSame(bool active, string description)
    {
        if (Instance == null) return;
        if (description.ToLower() != Instance.descriptionText.text.ToLower()) return;

        Instance.content.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputName">Input Name to execute the action</param>
    /// <param name="description">Description of the action.</param>
    public static void ShowIndication(string inputName, string description, Action clickCallback = null)
    {
        if (Instance == null) return;

        Instance.inputNameText.text = inputName.ToUpper();
        Instance.descriptionText.text = Instance.forceUpperCase ? description.ToUpper() : description;
        Instance.onClickCallback = clickCallback;
        SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnClick()
    {
        onClickCallback?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDie()
    {
        SetActive(false);
    }

    private static bl_InputInteractionIndicator _instance;
    public static bl_InputInteractionIndicator Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<bl_InputInteractionIndicator>();
            return _instance;
        }
    }
}