using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 背包管理器（单例模式）：负责跨场景数据存储、物品收集/回位、UI同步
/// </summary>
public class BackpackManager : MonoBehaviour
{
    // 全局单例实例
    public static BackpackManager Instance;

    [Header("背包配置")]
    public int maxLevelCount = 8; // 最大格子数（对应8个关卡）
    public BackpackSlot[] backpackSlots; // 当前场景的格子数组

    // 静态数据：跨场景保留（核心）
    private static ItemData[] levelCollectedPrefabs; // 各关卡收集的物品数据
    private static Vector3[] levelItemOriginalPositions; // 物品原始位置（用于回位）
    private static bool isGlobalDataInited = false; // 数据初始化标记

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保留
            // 初始化全局数据（仅第一次执行）
            if (!isGlobalDataInited)
            {
                InitLevelDataArrays();
                isGlobalDataInited = true;
            }
        }
        else
        {
            Destroy(gameObject); // 销毁重复实例
            return;
        }

        // 初始化当前场景格子
        InitBackpackSlots();
        // 注册场景切换监听
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("【背包管理器】初始化完成");
    }

    /// <summary>
    /// 场景加载完成回调（延迟同步UI，避免UI未加载）
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"【场景切换】进入{scene.name}，延迟同步UI");
        Invoke(nameof(SyncGlobalDataToUI), 0.2f);
    }

    void OnDestroy()
    {
        // 解除监听，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 初始化全局数据数组（跨场景保留）
    /// </summary>
    private void InitLevelDataArrays()
    {
        maxLevelCount = Mathf.Max(maxLevelCount, 8); // 兜底：最少8个格子
        levelCollectedPrefabs = new ItemData[maxLevelCount];
        levelItemOriginalPositions = new Vector3[maxLevelCount];
        Debug.Log($"【数据初始化】创建{maxLevelCount}长度的全局数据数组");
    }

    /// <summary>
    /// 初始化当前场景的格子（显示背景、清空图标）
    /// </summary>
    private void InitBackpackSlots()
    {
        // 自动查找当前场景所有格子
        if (backpackSlots == null || backpackSlots.Length == 0)
        {
            backpackSlots = FindObjectsOfType<BackpackSlot>();
            SortBackpackSlots(); // 按Slot_0~Slot_7排序
        }

        // 重置所有格子状态
        foreach (var slot in backpackSlots)
        {
            slot?.ResetSlot();
        }
        Debug.Log($"【格子初始化】当前场景共{backpackSlots.Length}个格子");
    }

    /// <summary>
    /// 核心：收集物品到背包（旧物品回位，新物品存储）
    /// </summary>
    /// <param name="itemToAdd">要收集的物品数据</param>
    public void AddItemToBackpack(ItemData itemToAdd)
    {
        // 基础判空
        if (itemToAdd == null || itemToAdd.prefabReference == null)
        {
            Debug.LogWarning("【收集失败】物品或预制体为空");
            return;
        }

        // 1. 校验关卡归属合法性
        int targetLevel = itemToAdd.belongToLevel;
        if (targetLevel < 1 || targetLevel > maxLevelCount)
        {
            Debug.LogWarning($"【收集失败】物品{itemToAdd.itemName}归属关卡{targetLevel}无效");
            return;
        }
        int targetSlotIndex = targetLevel - 1; // 转换为数组索引（0~7）

        // 2. 旧物品回位到场景
        ItemData oldItem = levelCollectedPrefabs[targetSlotIndex];
        Vector3 oldPos = levelItemOriginalPositions[targetSlotIndex];
        if (oldItem != null && oldPos != Vector3.zero)
        {
            ReturnOldItemToScene(oldItem, oldPos);
            Debug.Log($"【旧物品回位】{oldItem.itemName}已返回场景");
        }

        // 3. 存储新物品数据
        levelCollectedPrefabs[targetSlotIndex] = itemToAdd.prefabReference.GetComponent<ItemData>();
        levelItemOriginalPositions[targetSlotIndex] = itemToAdd.transform.position;

        // 4. 销毁场景中的物品实例
        Destroy(itemToAdd.gameObject);

        // 5. 刷新对应格子UI
        UpdateSingleSlotUI(targetSlotIndex, itemToAdd.itemIcon);

        Debug.Log($"<color=green>【收集成功】{itemToAdd.itemName}存入Slot_{targetSlotIndex}</color>");
    }

    /// <summary>
    /// 将旧物品回位到原始位置
    /// </summary>
    private void ReturnOldItemToScene(ItemData oldPrefab, Vector3 spawnPos)
    {
        // 多层空引用防护
        if (oldPrefab == null)
        {
            Debug.LogWarning("【回位失败】旧物品数据为空");
            return;
        }
        if (oldPrefab.prefabReference == null)
        {
            Debug.LogWarning($"【回位失败】{oldPrefab.itemName}预制体引用未赋值");
            return;
        }
        if (spawnPos == Vector3.zero)
        {
            Debug.LogWarning($"【回位失败】{oldPrefab.itemName}原始位置无效");
            return;
        }

        // 实例化旧物品
        GameObject newItem = Instantiate(oldPrefab.prefabReference, spawnPos, Quaternion.identity);
        if (newItem == null)
        {
            Debug.LogError($"【回位失败】实例化{oldPrefab.itemName}预制体失败");
            return;
        }

        // 复制物品数据（确保可再次收集）
        ItemData newItemData = newItem.GetComponent<ItemData>();
        if (newItemData != null)
        {
            newItemData.belongToLevel = oldPrefab.belongToLevel;
            newItemData.itemName = oldPrefab.itemName;
            newItemData.itemIcon = oldPrefab.itemIcon;
            newItemData.prefabReference = oldPrefab.prefabReference;
        }
    }

    /// <summary>
    /// 刷新单个格子的UI（精准刷新，避免全量更新）
    /// </summary>
    private void UpdateSingleSlotUI(int slotIndex, Sprite icon)
    {
        if (backpackSlots == null || slotIndex < 0 || slotIndex >= backpackSlots.Length)
        {
            Debug.LogWarning($"【刷新失败】格子索引{slotIndex}无效");
            return;
        }

        backpackSlots[slotIndex]?.UpdateSlot(icon);
    }

    /// <summary>
    /// 跨场景同步数据到UI（核心：保留背景，仅更新有物品的图标）
    /// </summary>
    public void SyncGlobalDataToUI()
    {
        // 刷新当前场景格子引用
        RefreshSlotReferences();

        // 先重置所有格子（显示背景）
        foreach (var slot in backpackSlots)
        {
            slot?.ResetSlot();
        }

        // 同步有物品的格子图标
        for (int i = 0; i < maxLevelCount; i++)
        {
            if (backpackSlots[i] == null) continue;

            ItemData item = levelCollectedPrefabs[i];
            if (item != null && item.itemIcon != null)
            {
                backpackSlots[i].UpdateSlot(item.itemIcon);
                Debug.Log($"【UI同步】Slot_{i} 显示{item.itemName}");
            }
        }
    }

    /// <summary>
    /// 刷新当前场景的格子引用（并排序）
    /// </summary>
    private void RefreshSlotReferences()
    {
        backpackSlots = FindObjectsOfType<BackpackSlot>();
        SortBackpackSlots();

        // 兜底：数组长度不足时补空
        if (backpackSlots.Length < maxLevelCount)
        {
            BackpackSlot[] temp = new BackpackSlot[maxLevelCount];
            Array.Copy(backpackSlots, temp, backpackSlots.Length);
            backpackSlots = temp;
        }
        Debug.Log($"【引用刷新】当前场景绑定{backpackSlots.Length}个格子");
    }

    /// <summary>
    /// 按格子名称排序（Slot_0 → Slot_7）
    /// </summary>
    private void SortBackpackSlots()
    {
        Array.Sort(backpackSlots, (a, b) =>
        {
            int aIdx = int.Parse(a.name.Replace("Slot_", ""));
            int bIdx = int.Parse(b.name.Replace("Slot_", ""));
            return aIdx.CompareTo(bIdx);
        });
    }
}