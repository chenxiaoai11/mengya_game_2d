using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 背包管理器（单例模式）：开局保持当前位置（隐藏），Tab键向右滑动显示
/// </summary>
public class BackpackManager : MonoBehaviour
{
    // 全局单例实例
    public static BackpackManager Instance;

    [Header("背包配置")]
    public int maxLevelCount = 8;
    public BackpackSlot[] backpackSlots;
    public GameObject backpackPanel;

    [Header("Tab键平滑滑动配置")]
    public KeyCode toggleKey = KeyCode.Tab; // 绑定Tab键
    public float showOffsetX = 300f; // 【关键修改】向右显示的位移距离（正数=向右，推荐200~400）
    public float moveSpeed = 8f; // 滑动速度（越大越丝滑，推荐6~12）
    private Vector3 hidePos; // 隐藏位置（=开局位置，你在编辑器中摆放的位置）
    private Vector3 showPos; // 显示位置（=隐藏位置+向右偏移showOffsetX）
    private Vector3 targetPos; // 背包目标位置
    private bool isHidden = true; // 默认标记为已隐藏（开局处于hidePos）

    // 静态数据：跨场景保留
    private static ItemData[] levelCollectedPrefabs;
    private static Vector3[] levelItemOriginalPositions;
    private static bool isGlobalDataInited = false;

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (!isGlobalDataInited)
            {
                InitLevelDataArrays();
                isGlobalDataInited = true;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化背包位置（核心：开局位置=隐藏位置，无需额外计算）
        if (backpackPanel != null)
        {
            // 1. 隐藏位置 = 背包开局的当前位置（你在Unity编辑器中摆放的位置）
            hidePos = backpackPanel.transform.localPosition;
            // 2. 显示位置 = 隐藏位置 + 向右偏移showOffsetX（正数向右）
            showPos = new Vector3(
                hidePos.x + showOffsetX,
                hidePos.y,
                hidePos.z
            );
            // 3. 初始目标位置 = 隐藏位置（保持开局位置不变，无需移动）
            targetPos = hidePos;
        }

        InitBackpackSlots();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("【背包管理器】初始化完成（开局保持当前位置，Tab键向右滑动显示）");
    }

    void Update()
    {
        // 1. 监听Tab键，切换目标位置
        if (Input.GetKeyDown(toggleKey) && backpackPanel != null)
        {
            ToggleBackpackTargetPos();
        }

        // 2. 核心：每帧平滑移动到目标位置（实现丝滑效果）
        if (backpackPanel != null)
        {
            SmoothMoveToTargetPos();
        }
    }

    /// <summary>
    /// 切换背包的目标位置（隐藏→显示：向右滑；显示→隐藏：向左滑）
    /// </summary>
    private void ToggleBackpackTargetPos()
    {
        if (isHidden)
        {
            // 当前隐藏（开局位置）→ 要显示：目标位置=向右偏移后的显示位置
            targetPos = showPos;
        }
        else
        {
            // 当前显示（向右偏移后）→ 要隐藏：目标位置=开局的隐藏位置
            targetPos = hidePos;
        }
        isHidden = !isHidden;
    }

    /// <summary>
    /// 丝滑移动到目标位置（核心插值逻辑，左右滑动均适用）
    /// </summary>
    private void SmoothMoveToTargetPos()
    {
        backpackPanel.transform.localPosition = Vector3.Lerp(
            backpackPanel.transform.localPosition, // 当前位置
            targetPos, // 目标位置
            moveSpeed * Time.deltaTime // 每帧移动比例（保证不同帧率速度一致）
        );
    }

    /// <summary>
    /// 场景加载完成回调（重置位置，保持开局隐藏逻辑）
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"【场景切换】进入{scene.name}，延迟同步UI");
        Invoke(nameof(SyncGlobalDataToUI), 0.2f);

        // 场景切换后，仍保持「开局位置=隐藏位置」的逻辑
        if (backpackPanel != null)
        {
            hidePos = backpackPanel.transform.localPosition;
            showPos = new Vector3(
                hidePos.x + showOffsetX,
                hidePos.y,
                hidePos.z
            );
            targetPos = hidePos;
            isHidden = true;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ---------------- 以下为原有核心逻辑，无修改 ----------------
    private void InitLevelDataArrays()
    {
        maxLevelCount = Mathf.Max(maxLevelCount, 8);
        levelCollectedPrefabs = new ItemData[maxLevelCount];
        levelItemOriginalPositions = new Vector3[maxLevelCount];
        Debug.Log($"【数据初始化】创建{maxLevelCount}长度的全局数据数组");
    }

    private void InitBackpackSlots()
    {
        if (backpackSlots == null || backpackSlots.Length == 0)
        {
            backpackSlots = FindObjectsOfType<BackpackSlot>();
            SortBackpackSlots();
        }

        foreach (var slot in backpackSlots)
        {
            slot?.ResetSlot();
        }
        Debug.Log($"【格子初始化】当前场景共{backpackSlots.Length}个格子");
    }

    public void AddItemToBackpack(ItemData itemToAdd)
    {
        if (itemToAdd == null || itemToAdd.prefabReference == null)
        {
            Debug.LogWarning("【收集失败】物品或预制体为空");
            return;
        }

        int targetLevel = itemToAdd.belongToLevel;
        if (targetLevel < 1 || targetLevel > maxLevelCount)
        {
            Debug.LogWarning($"【收集失败】物品{itemToAdd.itemName}归属关卡{targetLevel}无效");
            return;
        }
        int targetSlotIndex = targetLevel - 1;

        ItemData oldItem = levelCollectedPrefabs[targetSlotIndex];
        Vector3 oldPos = levelItemOriginalPositions[targetSlotIndex];
        if (oldItem != null && oldPos != Vector3.zero)
        {
            ReturnOldItemToScene(oldItem, oldPos);
            Debug.Log($"【旧物品回位】{oldItem.itemName}已返回场景");
        }

        levelCollectedPrefabs[targetSlotIndex] = itemToAdd.prefabReference.GetComponent<ItemData>();
        levelItemOriginalPositions[targetSlotIndex] = itemToAdd.transform.position;

        Destroy(itemToAdd.gameObject);

        UpdateSingleSlotUI(targetSlotIndex, itemToAdd.itemIcon);

        Debug.Log($"<color=green>【收集成功】{itemToAdd.itemName}存入Slot_{targetSlotIndex}</color>");
    }

    private void ReturnOldItemToScene(ItemData oldPrefab, Vector3 spawnPos)
    {
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

        GameObject newItem = Instantiate(oldPrefab.prefabReference, spawnPos, Quaternion.identity);
        if (newItem == null)
        {
            Debug.LogError($"【回位失败】实例化{oldPrefab.itemName}预制体失败");
            return;
        }

        ItemData newItemData = newItem.GetComponent<ItemData>();
        if (newItemData != null)
        {
            newItemData.belongToLevel = oldPrefab.belongToLevel;
            newItemData.itemName = oldPrefab.itemName;
            newItemData.itemIcon = oldPrefab.itemIcon;
            newItemData.prefabReference = oldPrefab.prefabReference;
        }
    }

    private void UpdateSingleSlotUI(int slotIndex, Sprite icon)
    {
        if (backpackSlots == null || slotIndex < 0 || slotIndex >= backpackSlots.Length)
        {
            Debug.LogWarning($"【刷新失败】格子索引{slotIndex}无效");
            return;
        }

        backpackSlots[slotIndex]?.UpdateSlot(icon);
    }

    public void SyncGlobalDataToUI()
    {
        RefreshSlotReferences();

        foreach (var slot in backpackSlots)
        {
            slot?.ResetSlot();
        }

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

    private void RefreshSlotReferences()
    {
        backpackSlots = FindObjectsOfType<BackpackSlot>();
        SortBackpackSlots();

        if (backpackSlots.Length < maxLevelCount)
        {
            BackpackSlot[] temp = new BackpackSlot[maxLevelCount];
            Array.Copy(backpackSlots, temp, backpackSlots.Length);
            backpackSlots = temp;
        }
        Debug.Log($"【引用刷新】当前场景绑定{backpackSlots.Length}个格子");
    }

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