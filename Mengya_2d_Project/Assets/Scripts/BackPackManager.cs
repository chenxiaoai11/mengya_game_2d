using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 背包管理器（单例模式，全局唯一）
public class BackpackManager : MonoBehaviour
{
    // 单例实例
    public static BackpackManager Instance;

    // 背包配置
    [Header("背包配置")]
    public int maxSlotCount = 8; // 最大格子数（固定8格）
    public BackpackSlot[] backpackSlots; // 拖拽绑定8个背包格子（按顺序从Slot_0到Slot_7）

    // 背包物品列表
    public List<ItemData> backpackItems = new List<ItemData>();

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化背包格子（清空所有格子）
        InitBackpackSlots();
    }

    /// <summary>
    /// 初始化背包格子
    /// </summary>
    private void InitBackpackSlots()
    {
        if (backpackSlots == null || backpackSlots.Length != maxSlotCount)
        {
            Debug.LogWarning($"背包格子数量不匹配，要求{maxSlotCount}格，请检查绑定！");
            return;
        }

        foreach (var slot in backpackSlots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// 向背包添加物品（带UI刷新）
    /// </summary>
    public void AddItemToBackpack(ItemData itemToAdd)
    {
        if (itemToAdd == null)
        {
            Debug.LogWarning("无法添加空物品到背包！");
            return;
        }

        // 1. 判断背包是否已满
        if (backpackItems.Count >= maxSlotCount)
        {
            Debug.LogWarning($"<color=red>【背包】背包已满（{maxSlotCount}格），无法添加{itemToAdd.itemName}！</color>");
            return;
        }

        // 2. 添加物品到列表
        backpackItems.Add(itemToAdd);
        Debug.Log($"<color=green>【背包】成功添加物品：{itemToAdd.itemName}</color>");

        // 3. 刷新背包UI（核心：填充对应格子）
        UpdateBackpackUI();

        // 4. 打印背包信息（调试用）
        PrintBackpackItems();
    }

    /// <summary>
    /// 刷新背包UI（从左到右填充格子）
    /// </summary>
    private void UpdateBackpackUI()
    {
        if (backpackSlots == null || backpackSlots.Length != maxSlotCount)
        {
            return;
        }

        // 先清空所有格子（避免残留）
        InitBackpackSlots();

        // 从左到右（Slot_0到Slot_7）填充已收集的物品图标
        for (int i = 0; i < backpackItems.Count; i++)
        {
            ItemData item = backpackItems[i];
            backpackSlots[i].UpdateSlot(item.itemIcon);
        }
    }

    /// <summary>
    /// 打印背包物品（调试用）
    /// </summary>
    public void PrintBackpackItems()
    {
        Debug.Log($"=== 当前背包物品（{backpackItems.Count}/{maxSlotCount}）===");
        for (int i = 0; i < backpackItems.Count; i++)
        {
            ItemData item = backpackItems[i];
            Debug.Log($"{i + 1}. ID：{item.itemId} | 名称：{item.itemName}");
        }
        Debug.Log("================================");
    }
}