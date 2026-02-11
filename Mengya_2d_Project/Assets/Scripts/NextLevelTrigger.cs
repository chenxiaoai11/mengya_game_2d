using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour, IInteractable
{
    public string nextSceneName;
    public int nextSceneBuildIndex = -1;
    public bool useBackpackItemGate = false;
    public string successSceneName = "Level8_current";
    public string failureSceneName = "Level8_error";
    public int[] requiredItemIds = new int[] { 11, 20, 21, 22, 23, 25, 26 };
    public bool useChangeCheckGate = false;
    public bool captureSnapshotOnAwake = false;
    public string unchangedSceneName = "Level9_he";
    public string changedSceneName = "Level9_pe";
    private int[] snapshotItemIds;

    void Awake()
    {
        if (captureSnapshotOnAwake)
        {
            CaptureSnapshot();
        }
    }

    void LoadNextLevel()
    {
        if (useChangeCheckGate)
        {
            bool unchanged = IsBackpackUnchanged();
            string sceneToLoad = unchanged ? unchangedSceneName : changedSceneName;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
                return;
            }
        }
        if (useBackpackItemGate)
        {
            bool ok = HasAllRequiredItems();
            string sceneToLoad = ok ? successSceneName : failureSceneName;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
                return;
            }
        }
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }

    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        LoadNextLevel();
    }

    bool HasAllRequiredItems()
    {
        var bm = BackpackManager.Instance;
        if (bm == null || requiredItemIds == null || requiredItemIds.Length == 0) return false;
        for (int i = 0; i < requiredItemIds.Length; i++)
        {
            int req = requiredItemIds[i];
            bool found = false;
            for (int slot = 0; slot < bm.maxLevelCount; slot++)
            {
                if (bm.GetItemIdInSlot(slot) == req) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }

    void CaptureSnapshot()
    {
        var bm = BackpackManager.Instance;
        if (bm == null)
        {
            snapshotItemIds = new int[0];
            return;
        }
        int count = 0;
        int[] temp = new int[bm.maxLevelCount];
        for (int slot = 0; slot < bm.maxLevelCount; slot++)
        {
            int id = bm.GetItemIdInSlot(slot);
            if (id > 0)
            {
                temp[count++] = id;
            }
        }
        snapshotItemIds = new int[count];
        for (int i = 0; i < count; i++) snapshotItemIds[i] = temp[i];
    }

    bool IsBackpackUnchanged()
    {
        var bm = BackpackManager.Instance;
        if (bm == null) return false;
        int count = 0;
        int[] current = new int[bm.maxLevelCount];
        for (int slot = 0; slot < bm.maxLevelCount; slot++)
        {
            int id = bm.GetItemIdInSlot(slot);
            if (id > 0)
            {
                current[count++] = id;
            }
        }
        int[] cur = new int[count];
        for (int i = 0; i < count; i++) cur[i] = current[i];
        return SetsEqual(snapshotItemIds, cur);
    }

    bool SetsEqual(int[] a, int[] b)
    {
        int alen = a != null ? a.Length : 0;
        int blen = b != null ? b.Length : 0;
        if (alen != blen) return false;
        for (int i = 0; i < alen; i++)
        {
            if (!Contains(b, a[i])) return false;
        }
        for (int i = 0; i < blen; i++)
        {
            if (!Contains(a, b[i])) return false;
        }
        return true;
    }

    bool Contains(int[] arr, int v)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == v) return true;
        }
        return false;
    }
}
