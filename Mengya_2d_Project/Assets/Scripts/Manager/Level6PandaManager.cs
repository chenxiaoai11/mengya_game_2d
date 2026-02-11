using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level6PandaManager : MonoBehaviour
{
    public static Level6PandaManager Instance;
    public string itemDetailPanelName = "ItemDetailPanel";
    public string targetItemName = "PROP_Panda";
    public int targetItemId = 0;
    public ItemData badPandaPrefab;
    public GameObject panel;
    public Button investigateButton;
    public bool overrideButtonOnClick = true;
    private bool hasBoundClick = false;
    private ItemData currentItem;
    public int requiredBackpackItemId = 7;
    public GameObject phoneToActivate;
    public string phoneObjectName;
    public string requireSceneContains = "Level6";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
        AutoBind();
    }

    void OnEnable()
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
            var btnGo = panel.transform.Find("InvestigateButton_Level6")?.gameObject;
            if (btnGo == null)
            {
                btnGo = new GameObject("InvestigateButton_Level6");
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
        if (match && !string.IsNullOrEmpty(requireSceneContains))
        {
            var sn = SceneManager.GetActiveScene().name ?? "";
            if (!sn.Contains(requireSceneContains)) match = false;
        }
        if (match && requiredBackpackItemId > 0)
        {
            var bm = BackpackManager.Instance;
            bool hasTool = false;
            if (bm != null)
            {
                for (int i = 0; i < bm.maxLevelCount; i++)
                {
                    if (bm.GetItemIdInSlot(i) == requiredBackpackItemId) { hasTool = true; break; }
                }
            }
            match = match && hasTool;
        }
        SetButtonVisible(match);
    }

    void SetButtonVisible(bool v)
    {
        if (investigateButton != null) investigateButton.gameObject.SetActive(v);
    }

    void OnInvestigate()
    {
        if (currentItem != null && badPandaPrefab != null)
        {
            var pos = currentItem.transform.position;
            var rot = currentItem.transform.rotation;
            var parent = currentItem.transform.parent;
            currentItem.gameObject.SetActive(false);
            GameObject template = badPandaPrefab.prefabReference != null ? badPandaPrefab.prefabReference : badPandaPrefab.gameObject;
            if (template == null) { SetButtonVisible(false); return; }
            GameObject spawned = null;
            if (parent != null) spawned = Instantiate(template, pos, rot, parent);
            else spawned = Instantiate(template, pos, rot);
            var data = spawned.GetComponent<ItemData>();
            if (data != null)
            {
                data.itemId = badPandaPrefab.itemId;
                data.itemName = badPandaPrefab.itemName;
                data.itemDescription = badPandaPrefab.itemDescription;
                data.belongToLevel = badPandaPrefab.belongToLevel;
                data.itemIcon = badPandaPrefab.itemIcon;
                data.prefabReference = badPandaPrefab.prefabReference;
            }
            spawned.SetActive(true);
        }
        SetButtonVisible(false);
        if (ItemDetailManager.Instance != null)
        {
            ItemDetailManager.Instance.HideDetailPanel();
        }
        if (phoneToActivate != null) phoneToActivate.SetActive(true);
        else if (!string.IsNullOrEmpty(phoneObjectName))
        {
            var go = GameObject.Find(phoneObjectName);
            if (go != null) go.SetActive(true);
        }
    }

    public void SetCurrentItem(ItemData item)
    {
        currentItem = item;
    }
}
