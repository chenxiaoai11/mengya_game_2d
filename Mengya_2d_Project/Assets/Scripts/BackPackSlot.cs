using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackpackSlot : MonoBehaviour
{
    [Header("格子组件引用")]
    public Image itemIconImage; // 拖拽绑定格子下的ItemIcon

    /// <summary>
    /// 更新格子显示（填充物品图标）
    /// </summary>
    /// <param name="itemIcon">要显示的物品图标</param>
    public void UpdateSlot(Sprite itemIcon)
    {
        Debug.Log($"格子更新：当前格子是{gameObject.name}，收到图标：{itemIcon?.name ?? "空"}");
        if (itemIconImage != null)
        {
            Debug.Log($"  图标载体：{itemIconImage.gameObject.name}，设置激活：{itemIcon != null}");
            itemIconImage.gameObject.SetActive(itemIcon != null);
            itemIconImage.sprite = itemIcon;
        }
        else
        {
            Debug.LogWarning("格子的Item Icon Image未绑定！");
        }
    }

    /// <summary>
    /// 清空格子（隐藏图标）
    /// </summary>
    public void ClearSlot()
    {
        if (itemIconImage != null)
        {
            itemIconImage.gameObject.SetActive(false);
            itemIconImage.sprite = null;
        }
    }
}