using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个背包格子的UI控制：负责背景显示、物品图标刷新/清空
/// </summary>
public class BackpackSlot : MonoBehaviour
{
    [Header("UI组件引用")]
    [Tooltip("格子背景Image组件")]
    public Image slotBackground;
    [Tooltip("物品图标Image组件（挂载在格子子节点）")]
    public Image itemIcon;

    void Awake()
    {
        // 强制激活物体，防止逻辑失效
        gameObject.SetActive(true);
        // 自动查找未赋值的UI组件
        AutoFindComponents();
        // 初始化格子默认状态
        ResetSlot();
    }

    /// <summary>
    /// 自动查找UI组件（避免手动赋值遗漏）
    /// </summary>
    private void AutoFindComponents()
    {
        // 查找背景组件
        if (slotBackground == null)
        {
            slotBackground = transform.Find("Background")?.GetComponent<Image>();
            slotBackground ??= GetComponent<Image>(); // 兜底：自身作为背景
            Debug.Log($"{gameObject.name} 背景组件查找{(slotBackground != null ? "成功" : "失败")}");
        }

        // 查找图标组件
        if (itemIcon == null)
        {
            itemIcon = transform.Find("Icon")?.GetComponent<Image>();
            itemIcon ??= GetComponentInChildren<Image>(); // 兜底：子节点第一个Image
            Debug.Log($"{gameObject.name} 图标组件查找{(itemIcon != null ? "成功" : "失败")}");
        }
    }

    /// <summary>
    /// 重置格子到默认状态（显示背景、清空图标）
    /// </summary>
    public void ResetSlot()
    {
        // 显示背景并重置颜色
        if (slotBackground != null)
        {
            slotBackground.enabled = true;
            slotBackground.color = Color.white;
        }

        // 清空图标
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemIcon.color = Color.white;
        }
    }

    /// <summary>
    /// 更新格子显示的物品图标
    /// </summary>
    /// <param name="icon">物品图标（null则清空）</param>
    public void UpdateSlot(Sprite icon)
    {
        // 空引用防护
        if (this == null || gameObject == null || slotBackground == null || itemIcon == null)
        {
            Debug.LogWarning($"{gameObject.name} 组件不完整，无法更新图标");
            return;
        }

        // 强制显示背景
        slotBackground.enabled = true;

        // 更新图标
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = true;
            Debug.Log($"{gameObject.name} 成功显示图标：{icon.name}");
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            Debug.Log($"{gameObject.name} 清空物品图标");
        }
    }

    /// <summary>
    /// 清空格子的物品图标（保留背景）
    /// </summary>
    public void ClearSlot()
    {
        if (this == null || gameObject == null || itemIcon == null)
        {
            Debug.LogWarning($"{gameObject.name} 组件无效，无法清空图标");
            return;
        }

        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }
}