using UnityEngine;

public class BlackPersonVisibility : MonoBehaviour
{
    public string targetSlotName = "Slot_1";
    public int targetItemId = 0;
    public Sprite targetSprite;
    public GameObject target;
    public float checkInterval = 0.2f;
    private float timer;
    private bool lastVisible;
    public bool enableDebug = false;
    public bool matchAnySlot = true;
    public GameObject flickerObject;
    public GameObject[] flickerObjects;
    public bool enableFlicker = true;
    public float flickerOnDuration = 0.12f;
    public float flickerOffDuration = 0.08f;
    private float flickerTimer;
    private bool flickerState;

    void OnEnable()
    {
        Evaluate();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            Evaluate();
        }
    }

    private void Evaluate()
    {
        if (target == null) target = gameObject;
        bool shouldShow = false;
        var bm = BackpackManager.Instance;
        if (bm != null && bm.backpackSlots != null && bm.backpackSlots.Length > 0)
        {
            int idx = 0;
            if (!matchAnySlot)
            {
                if (targetSlotName.StartsWith("Slot_"))
                {
                    var numStr = targetSlotName.Substring(5);
                    int n;
                    if (int.TryParse(numStr, out n)) idx = Mathf.Max(0, n - 1);
                }
                int itemId = bm.GetItemIdInSlot(idx);
                shouldShow = (targetItemId > 0) && (itemId == targetItemId);
                if (!shouldShow && targetSprite != null)
                {
                    var slot = bm.backpackSlots[idx];
                    if (slot != null && slot.itemIcon != null && slot.itemIcon.sprite != null)
                    {
                        shouldShow = slot.itemIcon.sprite == targetSprite;
                    }
                }
            }
            else
            {
                int foundItemId = 0;
                for (int i = 0; i < bm.backpackSlots.Length; i++)
                {
                    int itemId = bm.GetItemIdInSlot(i);
                    bool matchId = (targetItemId > 0) && (itemId == targetItemId);
                    bool matchSprite = false;
                    if (!matchId && targetSprite != null)
                    {
                        var slot = bm.backpackSlots[i];
                        if (slot != null && slot.itemIcon != null && slot.itemIcon.sprite != null)
                        {
                            matchSprite = slot.itemIcon.sprite == targetSprite;
                        }
                    }
                    if (matchId || matchSprite)
                    {
                        shouldShow = true;
                        idx = i;
                        foundItemId = itemId;
                        break;
                    }
                }
            }
        }
        if (shouldShow != lastVisible)
        {
            if (target != null) target.SetActive(shouldShow);
            lastVisible = shouldShow;
            if (enableFlicker)
            {
                flickerTimer = 0f;
                flickerState = false;
                if (!shouldShow) SetLightsActive(true);
            }
        }
        if (enableFlicker && lastVisible)
        {
            flickerTimer += Time.unscaledDeltaTime;
            float dur = flickerState ? flickerOnDuration : flickerOffDuration;
            if (flickerTimer >= dur)
            {
                flickerTimer = 0f;
                flickerState = !flickerState;
                SetLightsActive(flickerState);
            }
        }
    }

    private void SetLightsActive(bool state)
    {
        if (flickerObjects != null && flickerObjects.Length > 0)
        {
            for (int i = 0; i < flickerObjects.Length; i++)
            {
                var obj = flickerObjects[i];
                if (obj != null) obj.SetActive(state);
            }
            return;
        }
        if (flickerObject != null)
        {
            flickerObject.SetActive(state);
        }
    }
}
