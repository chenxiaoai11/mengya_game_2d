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
    private static Transform[] levelItemOriginalParents;
    private static Quaternion[] levelItemOriginalRotations;
    private static Vector3[] levelItemOriginalScales;
    private static GameObject[] levelStoredSceneInstances;
    private class StoredItemSnapshot
    {
        public int itemId;
        public string itemName;
        public string itemDescription;
        public int belongToLevel;
        public Sprite itemIcon;
        public GameObject prefabReference;
    }
    private static StoredItemSnapshot[] levelCollectedSnapshots;
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
        levelItemOriginalParents = new Transform[maxLevelCount];
        levelItemOriginalRotations = new Quaternion[maxLevelCount];
        levelItemOriginalScales = new Vector3[maxLevelCount];
        levelCollectedSnapshots = new StoredItemSnapshot[maxLevelCount];
        levelStoredSceneInstances = new GameObject[maxLevelCount];
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

        // ========== 关键1：提前保存新物品的原始位置（销毁前） ==========
        Vector3 newItemPos = itemToAdd.transform.position;
        Debug.Log($"【新物品位置】{itemToAdd.itemName} 原始位置：{newItemPos}（销毁前）");

        // ========== 关键2：打印旧物品+旧位置的原始数据 ==========
        ItemData oldItem = levelCollectedPrefabs[targetSlotIndex];
        StoredItemSnapshot oldSnapshot = levelCollectedSnapshots[targetSlotIndex];
        Vector3 oldPos = levelItemOriginalPositions[targetSlotIndex];
        Transform oldParent = levelItemOriginalParents[targetSlotIndex];
        Quaternion oldRot = levelItemOriginalRotations[targetSlotIndex];
        Vector3 oldScale = levelItemOriginalScales[targetSlotIndex];
        Debug.Log($"【旧物品校验】Slot_{targetSlotIndex} | 旧物品：{(oldItem != null ? oldItem.itemName : (oldSnapshot != null ? "来自快照" : "空"))} | 旧位置：{oldPos}");
        Debug.Log($"【旧物品数据】oldItem是否为null：{(oldItem == null ? "是" : "否")} | oldItem来源：{(oldItem != null ? "直接引用" : (oldSnapshot != null ? "快照" : "无"))}");
        if (newItemPos == Vector3.zero || float.IsNaN(newItemPos.x))
        {
            Debug.LogError($"【位置异常】{itemToAdd.itemName} 位置为无效值：{newItemPos}，无法存储！");
            // 降级方案：用预制体的默认位置
            if (itemToAdd.prefabReference != null)
            {
                newItemPos = itemToAdd.prefabReference.transform.position;
                Debug.Log($"【降级处理】改用预制体位置：{newItemPos}");
            }
            else
            {
                Debug.LogError($"【无法降级】{itemToAdd.itemName} 预制体也为空！");
                return; // 终止存储
            }
        }
        // 触发旧物品回位（增加日志）
        Debug.Log($"【旧物品快照】isNull:{(oldSnapshot==null)} | hasPrefab:{(oldSnapshot!=null && oldSnapshot.prefabReference!=null)} | oldPos:{oldPos}");
        var oldInstance = levelStoredSceneInstances[targetSlotIndex];
        if (oldInstance != null)
        {
            Debug.Log($"【回位触发(实例)】准备回位 {oldInstance.name} 到 {oldPos}");
            ReturnOldInstanceToScene(oldInstance, oldPos, oldParent, oldRot, oldScale);
        }
        else if (oldSnapshot != null && oldSnapshot.prefabReference != null)
        {
            Debug.Log($"【回位触发(快照)】准备回位 {oldSnapshot.itemName} 到 {oldPos}");
            ReturnOldItemToSceneFromSnapshot(oldSnapshot, oldPos, oldParent, oldRot, oldScale);
        }
        else
        {
            Debug.LogWarning($"【回位跳过】Slot_{targetSlotIndex} 无旧物品或旧位置无效");
            if (oldPos == Vector3.zero) Debug.LogWarning($"【回位跳过】Slot_{targetSlotIndex} 旧位置为(0,0,0)");
        }

        // ========== 关键3：更新存储（用提前保存的新物品位置） ==========
        var prefabItemData = itemToAdd.prefabReference != null ? itemToAdd.prefabReference.GetComponent<ItemData>() : null;
        levelCollectedPrefabs[targetSlotIndex] = prefabItemData;
        levelCollectedSnapshots[targetSlotIndex] = new StoredItemSnapshot
        {
            itemId = itemToAdd.itemId,
            itemName = itemToAdd.itemName,
            itemDescription = itemToAdd.itemDescription,
            belongToLevel = itemToAdd.belongToLevel,
            itemIcon = itemToAdd.itemIcon,
            prefabReference = itemToAdd.prefabReference
        };
        levelItemOriginalPositions[targetSlotIndex] = newItemPos; // 核心：用销毁前的位置
        levelItemOriginalParents[targetSlotIndex] = itemToAdd.transform.parent;
        levelItemOriginalRotations[targetSlotIndex] = itemToAdd.transform.rotation;
        levelItemOriginalScales[targetSlotIndex] = itemToAdd.transform.localScale;
        Debug.Log($"【存储更新】Slot_{targetSlotIndex} | 新物品：{itemToAdd.itemName} | 存储位置：{newItemPos}");

        // 隐藏当前场景物品实例（不销毁，便于下次直接复原）
        levelStoredSceneInstances[targetSlotIndex] = itemToAdd.gameObject;
        itemToAdd.gameObject.SetActive(false);

        UpdateSingleSlotUI(targetSlotIndex, itemToAdd.itemIcon);
        Debug.Log($"<color=green>【收集成功】{itemToAdd.itemName}存入Slot_{targetSlotIndex}</color>");
    }

    private void ReturnOldItemToScene(ItemData oldPrefab, Vector3 spawnPos, Transform parent, Quaternion rot, Vector3 scale)
    {
        // 全量日志：打印传入的预制体和位置
        Debug.Log($"【回位执行】预制体名称：{(oldPrefab == null ? "空" : oldPrefab.itemName)} | 目标位置：{spawnPos}");

        if (oldPrefab == null)
        {
            Debug.LogError("【回位失败】旧物品预制体数据为空");
            return;
        }
        if (oldPrefab.prefabReference == null)
        {
            Debug.LogError($"【回位失败】{oldPrefab.itemName} 的 prefabReference 未赋值！请在Unity编辑器中拖入预制体");
            return;
        }
        if (spawnPos == Vector3.zero)
        {
            Debug.LogError($"【回位失败】{oldPrefab.itemName} 生成位置为(0,0,0)，无法回位");
            return;
        }

        GameObject spawnedItem = null;
        if (parent != null)
        {
            spawnedItem = Instantiate(oldPrefab.prefabReference, spawnPos, rot, parent);
        }
        else
        {
            spawnedItem = Instantiate(oldPrefab.prefabReference, spawnPos, rot);
        }
        if (spawnedItem == null)
        {
            Debug.LogError($"【回位失败】实例化 {oldPrefab.itemName} 预制体失败");
            return;
        }

        // 同步ItemData（确保实例化后的物品数据正确）
        ItemData spawnedItemData = spawnedItem.GetComponent<ItemData>();
        if (spawnedItemData != null)
        {
            spawnedItemData.itemId = oldPrefab.itemId;
            spawnedItemData.itemName = oldPrefab.itemName;
            spawnedItemData.itemDescription = oldPrefab.itemDescription;
            spawnedItemData.belongToLevel = oldPrefab.belongToLevel;
            spawnedItemData.itemIcon = oldPrefab.itemIcon;
            spawnedItemData.prefabReference = oldPrefab.prefabReference; // 关键：同步预制体引用
            if (scale != Vector3.zero)
            {
                spawnedItem.transform.localScale = scale;
            }
            Debug.Log($"【回位成功】{oldPrefab.itemName} 生成在 {spawnPos} | 实例名称：{spawnedItem.name}");
        }
        else
        {
            Debug.LogError($"【回位异常】{oldPrefab.itemName} 实例化后缺少 ItemData 组件");
        }
    }

    private void ReturnOldItemToSceneFromSnapshot(StoredItemSnapshot snap, Vector3 spawnPos, Transform parent, Quaternion rot, Vector3 scale)
    {
        Debug.Log($"【回位执行(快照)】名称：{(snap == null ? "空" : snap.itemName)} | 目标位置：{spawnPos}");
        if (snap == null || snap.prefabReference == null)
        {
            Debug.LogError("【回位失败(快照)】快照或预制体为空");
            return;
        }
        GameObject spawnedItem = Instantiate(snap.prefabReference);
        if (parent != null)
        {
            spawnedItem.transform.SetParent(parent, true);
        }
        spawnedItem.transform.position = spawnPos;
        spawnedItem.transform.rotation = rot;
        if (spawnedItem == null)
        {
            Debug.LogError($"【回位失败(快照)】实例化 {snap.itemName} 预制体失败");
            return;
        }

        var spawnedItemData = spawnedItem.GetComponent<ItemData>();
        if (spawnedItemData != null)
        {
            spawnedItemData.itemId = snap.itemId;
            spawnedItemData.itemName = snap.itemName;
            spawnedItemData.itemDescription = snap.itemDescription;
            spawnedItemData.belongToLevel = snap.belongToLevel;
            spawnedItemData.itemIcon = snap.itemIcon;
            spawnedItemData.prefabReference = snap.prefabReference;
        }
        if (scale != Vector3.zero) spawnedItem.transform.localScale = scale;
        Debug.Log($"【回位成功(快照)】{snap.itemName} 生成在 {spawnPos} | 实例名称：{spawnedItem.name}");
    }
    private void ReturnOldInstanceToScene(GameObject instance, Vector3 spawnPos, Transform parent, Quaternion rot, Vector3 scale)
    {
        if (instance == null)
        {
            Debug.LogError("【回位失败(实例)】实例为空");
            return;
        }
        if (parent != null)
        {
            instance.transform.SetParent(parent, true);
        }
        instance.transform.position = spawnPos;
        instance.transform.rotation = rot;
        if (scale != Vector3.zero) instance.transform.localScale = scale;
        instance.SetActive(true);
        Debug.Log($"【回位成功(实例)】{instance.name} 回到 {spawnPos}");
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
            else if (levelCollectedSnapshots[i] != null && levelCollectedSnapshots[i].itemIcon != null)
            {
                backpackSlots[i].UpdateSlot(levelCollectedSnapshots[i].itemIcon);
                Debug.Log($"【UI同步】Slot_{i} 显示{levelCollectedSnapshots[i].itemName}(快照)");
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
