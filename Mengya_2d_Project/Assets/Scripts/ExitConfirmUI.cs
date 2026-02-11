using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExitConfirmUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject cgPanel;
    public Image cgImage;

    void Awake()
    {
        if (panel == null)
        {
            var t = transform.Find("ExitConfirmPanel");
            if (t != null) panel = t.gameObject;
        }
        if (confirmButton == null)
        {
            var t = transform.Find("LeaveConfirmButton");
            if (t != null) confirmButton = t.GetComponent<Button>();
        }
        if (cancelButton == null)
        {
            var t = transform.Find("LeaveCancelButton");
            if (t != null) cancelButton = t.GetComponent<Button>();
        }
        if (messageText == null)
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Show(string message, Action onConfirm, Action onCancel)
    {
        gameObject.SetActive(true);
        if (panel != null) panel.SetActive(true);
        if (cgPanel != null) cgPanel.SetActive(false);
        if (messageText != null) messageText.text = message;
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => onConfirm?.Invoke());
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => onCancel?.Invoke());
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        if (cgPanel != null) cgPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowCG(Sprite sprite)
    {
        if (panel != null) panel.SetActive(false);
        if (cgPanel != null) cgPanel.SetActive(true);
        if (cgImage != null)
        {
            cgImage.sprite = sprite;
            cgImage.enabled = sprite != null;
            cgImage.preserveAspect = true;
        }
    }
}
