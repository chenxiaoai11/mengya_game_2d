using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemClickDetector : MonoBehaviour
{
    private ItemData currentItemData; // 当前点击的物品数据

    void Start()
    {
        // 获取当前物品身上的ItemData组件
        currentItemData = GetComponent<ItemData>();

        // 安全校验
        if (currentItemData == null)
        {
            Debug.LogWarning("当前物品未挂载ItemData组件，请添加！");
        }
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            CheckItemClick();
        }
    }

    /// <summary>
    /// 检测是否点击到当前物品（已修复z轴赋值错误）
    /// </summary>
    private void CheckItemClick()
    {
        // 1. 将鼠标屏幕坐标转换为世界坐标（用Vector3接收，设置z轴为0）
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 2. 检测鼠标是否命中物品碰撞体
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        // 3. 命中则弹出详情面板（不再直接收集）
        if (hitCollider != null && hitCollider.gameObject == this.gameObject)
        {
            ShowItemDetail();
        }
    }

    /// <summary>
    /// 弹出物品详情面板
    /// </summary>
    private void ShowItemDetail()
    {
        if (currentItemData == null || ItemDetailManager.Instance == null)
        {
            Debug.LogWarning("无法显示详情面板：物品数据或详情管理器不存在！");
            return;
        }

        // 调用详情管理器，显示面板并传递物品数据
        ItemDetailManager.Instance.ShowDetailPanel(currentItemData);
    }
}