using UnityEngine;
using UnityEngine.Playables;

public class TimelineUIHider : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject backpackPanel;
    public GameObject settingPanel;
    public GameObject backpackIcon;
    public string backpackPanelName = "BackpackPanel";
    public string settingPanelName = "Setting";
    public string backpackIconName = "BackpackIcon";
    private bool prevBackpackActive;
    private bool prevSettingActive;
    private bool prevIconActive;
    public bool forceShowOnStop = true;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        AutoBind();
    }

    void OnEnable()
    {
        if (director != null)
        {
            director.played -= OnPlayed;
            director.stopped -= OnStopped;
            director.played += OnPlayed;
            director.stopped += OnStopped;
            Invoke(nameof(SyncPlayingState), 0.02f);
        }
    }

    void OnDisable()
    {
        if (director != null)
        {
            director.played -= OnPlayed;
            director.stopped -= OnStopped;
        }
    }

    void AutoBind()
    {
        if (backpackPanel == null)
        {
            var go = GameObject.Find(backpackPanelName);
            if (go == null) go = FindByNameIncludingInactive(backpackPanelName);
            if (go != null) backpackPanel = go;
        }
        if (settingPanel == null)
        {
            var go = GameObject.Find(settingPanelName);
            if (go == null) go = FindByNameIncludingInactive(settingPanelName);
            if (go != null) settingPanel = go;
        }
        if (backpackIcon == null)
        {
            var go = GameObject.Find(backpackIconName);
            if (go == null) go = FindByNameIncludingInactive(backpackIconName);
            if (go != null) backpackIcon = go;
        }
    }

    void OnPlayed(PlayableDirector d)
    {
        prevBackpackActive = backpackPanel != null && backpackPanel.activeSelf;
        prevSettingActive = settingPanel != null && settingPanel.activeSelf;
        prevIconActive = backpackIcon != null && backpackIcon.activeSelf;
        if (backpackPanel != null) backpackPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);
        if (backpackIcon != null) backpackIcon.SetActive(false);
    }

    void OnStopped(PlayableDirector d)
    {
        if (forceShowOnStop)
        {
            ShowAll();
        }
        else
        {
            if (backpackPanel != null) backpackPanel.SetActive(prevBackpackActive);
            if (settingPanel != null) settingPanel.SetActive(prevSettingActive);
            if (backpackIcon != null) backpackIcon.SetActive(prevIconActive);
        }
    }

    void SyncPlayingState()
    {
        if (director != null && director.state == PlayState.Playing)
        {
            OnPlayed(director);
        }
    }

    void ShowAll()
    {
        if (backpackPanel != null) backpackPanel.SetActive(true);
        if (settingPanel != null) settingPanel.SetActive(true);
        if (backpackIcon != null) backpackIcon.SetActive(true);
    }

    GameObject FindByNameIncludingInactive(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var found = FindChildByNameRecursive(roots[i].transform, name);
            if (found != null) return found;
        }
        return null;
    }

    GameObject FindChildByNameRecursive(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root.gameObject;
        for (int i = 0; i < root.childCount; i++)
        {
            var c = root.GetChild(i);
            var found = FindChildByNameRecursive(c, name);
            if (found != null) return found;
        }
        return null;
    }
}
