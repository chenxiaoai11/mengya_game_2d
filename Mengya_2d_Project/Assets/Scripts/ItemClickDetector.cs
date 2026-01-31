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

        // 3. 命中则收集物品（存入背包+消失）
        if (hitCollider != null && hitCollider.gameObject == this.gameObject)
        {
            CollectItem();
        }
    }

    /// <summary>
    /// 收集物品（存入背包+地图上物品消失）
    /// </summary>
    private void CollectItem()
    {
        if (currentItemData == null || BackpackManager.Instance == null)
        {
            Debug.LogWarning("收集物品失败：物品数据或背包管理器不存在！");
            return;
        }

        // 1. 先打印物品信息（保留原有功能）
        Debug.Log($"=== 物品信息 ===\n物品ID：{currentItemData.itemId}\n物品名称：{currentItemData.itemName}\n物品描述：{currentItemData.itemDescription}\n===============");

        // 2. 调用背包管理器，将物品存入背包
        BackpackManager.Instance.AddItemToBackpack(currentItemData);

        // 3. 让地图上的物品消失（二选一即可，推荐销毁）
        // 方式1：直接销毁物品（彻底从场景中移除，无法恢复）【推荐】
        Destroy(gameObject);

        // 方式2：隐藏物品（保留物体，可后续恢复，取消注释即可使用）
        // gameObject.SetActive(false);
    }
}