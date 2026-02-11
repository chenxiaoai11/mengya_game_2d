using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ItemInvestigationManager : MonoBehaviour
{
    public static ItemInvestigationManager Instance;
    public string itemDetailPanelName = "ItemDetailPanel";
    public string targetItemName = "PROP_StudentHandbook_1";
    public int targetItemId = 0;
    public ItemData picturePrefab;
    public GameObject panel;
    public Button investigateButton;
    public bool overrideButtonOnClick = true;
    private bool hasBoundClick = false;
    private ItemData currentItem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            AutoBind();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoBind();
    }

    void AutoBind()
    {
        if (panel == null)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < all.Length; i++)
            {
                var go = all[i];
                if (go.name == itemDetailPanelName)
                {
                    panel = go;
                    break;
                }
            }
        }
        EnsureButton();
        SetButtonVisible(false);
    }

    void EnsureButton()
    {
        if (investigateButton == null && panel != null)
        {
            var btnGo = panel.transform.Find("InvestigateButton")?.gameObject;
            if (btnGo == null)
            {
                btnGo = new GameObject("InvestigateButton");
                btnGo.transform.SetParent(panel.transform, false);
                var img = btnGo.AddComponent<Image>();
                var btn = btnGo.AddComponent<Button>();
                investigateButton = btn;
                var txtGO = new GameObject("Text");
                txtGO.transform.SetParent(btnGo.transform, false);
                var txt = txtGO.AddComponent<Text>();
                txt.text = "调查";
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.black;
                var rt = btnGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0f);
                rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 30f);
                rt.sizeDelta = new Vector2(120f, 36f);
            }
            else
            {
                investigateButton = btnGo.GetComponent<Button>();
            }
            hasBoundClick = false;
        }
        if (investigateButton != null && !hasBoundClick)
        {
            if (overrideButtonOnClick)
            {
                investigateButton.onClick.RemoveAllListeners();
                investigateButton.onClick.AddListener(OnInvestigate);
            }
            else
            {
                investigateButton.onClick.AddListener(OnInvestigate);
            }
            hasBoundClick = true;
        }
    }

    void Update()
    {
        if (panel == null)
        {
            SetButtonVisible(false);
            return;
        }
        if (!panel.activeInHierarchy)
        {
            SetButtonVisible(false);
            return;
        }
        bool match = false;
        if (currentItem != null)
        {
            if (targetItemId > 0) match = currentItem.itemId == targetItemId;
            else if (!string.IsNullOrEmpty(targetItemName)) match = currentItem.itemName == targetItemName;
        }
        SetButtonVisible(match);
    }

    int FindTargetSlotIndex(BackpackManager bm)
    {
        if (targetItemId > 0)
        {
            for (int i = 0; i < bm.maxLevelCount; i++)
            {
                int id = bm.GetItemIdInSlot(i);
                if (id == targetItemId) return i;
            }
        }
        if (!string.IsNullOrEmpty(targetItemName))
        {
            for (int i = 0; i < bm.maxLevelCount; i++)
            {
                var name = bm.GetItemNameInSlot(i);
                if (name == targetItemName) return i;
            }
        }
        return -1;
    }

    void SetButtonVisible(bool v)
    {
        if (investigateButton != null) investigateButton.gameObject.SetActive(v);
    }

    void OnInvestigate()
    {
        if (currentItem != null && picturePrefab != null)
        {
            var pos = currentItem.transform.position;
            var rot = currentItem.transform.rotation;
            var parent = currentItem.transform.parent;
            currentItem.gameObject.SetActive(false);
            GameObject template = picturePrefab.prefabReference != null ? picturePrefab.prefabReference : picturePrefab.gameObject;
            if (template == null) { SetButtonVisible(false); return; }
            GameObject spawned = null;
            if (parent != null) spawned = Instantiate(template, pos, rot, parent);
            else spawned = Instantiate(template, pos, rot);
            var data = spawned.GetComponent<ItemData>();
            if (data != null)
            {
                data.itemId = picturePrefab.itemId;
                data.itemName = picturePrefab.itemName;
                data.itemDescription = picturePrefab.itemDescription;
                data.belongToLevel = picturePrefab.belongToLevel;
                data.itemIcon = picturePrefab.itemIcon;
                data.prefabReference = picturePrefab.prefabReference;
            }
            spawned.SetActive(true);
        }
        SetButtonVisible(false);
        if (ItemDetailManager.Instance != null)
        {
            ItemDetailManager.Instance.HideDetailPanel();
        }
    }

    public void SetCurrentItem(ItemData item)
    {
        currentItem = item;
    }
}
