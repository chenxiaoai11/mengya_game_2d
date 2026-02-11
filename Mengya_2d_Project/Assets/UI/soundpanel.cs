using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundpanel : MonoBehaviour
{
    public GameObject targetPanel;

    // 显示弹窗
    public void ShowPanel()
    {
        targetPanel.SetActive(true);
    }

    // 隐藏弹窗
    public void HidePanel()
    {
        targetPanel.SetActive(false);
    }

    // 切换显示/隐藏
    public void TogglePanel()
    {
        targetPanel.SetActive(!targetPanel.activeSelf);
    }
}
