using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

[RequireComponent(typeof(PlayableDirector))]
public class Level2AutoPlayTimeline : MonoBehaviour
{
    public string requireSceneContains = "Level2";
    public float delay = 0f;
    public bool disableCinemachineBrainOnStop = true;
    public bool switchToPlayerVCamOnStop = false;
    public string playerVCamName = "PlayerVCam";
    public int requiredBackpackItemId = 0;
    private PlayableDirector director;
    private bool played;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        TryPlay();
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
            director.stopped += OnTimelineStopped;
        }
    }

    void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }

    void TryPlay()
    {
        if (played) return;
        if (director == null) return;
        var sn = SceneManager.GetActiveScene().name ?? "";
        if (!string.IsNullOrEmpty(requireSceneContains) && !sn.Contains(requireSceneContains)) return;
        if (requiredBackpackItemId > 0 && !HasRequiredItem(requiredBackpackItemId)) return;
        LogTimelineInfo("准备播放");
        if (delay > 0f) { Invoke(nameof(PlayNow), delay); } else { PlayNow(); }
    }

    void PlayNow()
    {
        if (director == null) return;
        director.Play();
        played = true;
    }

    void LogTimelineInfo(string prefix)
    {
        var dir = director;
        var asset = dir != null ? dir.playableAsset : null;
        var assetName = asset != null ? asset.name : "null";
        Debug.Log($"[Level2Timeline] {prefix} | Director:{(dir!=null?dir.name:"null")} Asset:{assetName} State:{(dir!=null?dir.state.ToString():"null")}");
        var timelineAsset = asset as TimelineAsset;
        if (dir != null && timelineAsset != null)
        {
            foreach (var output in timelineAsset.outputs)
            {
                var src = output.sourceObject;
                var bind = dir.GetGenericBinding(src);
                var stream = output.streamName;
                var bindName = bind != null ? bind.name : "null";
                var targetType = output.outputTargetType != null ? output.outputTargetType.Name : "unknown";
                Debug.Log($"[Level2Timeline] 轨道:{stream} 目标类型:{targetType} 绑定:{bindName}");
            }
        }
    }

    void OnTimelineStopped(PlayableDirector d)
    {
        var mainCam = Camera.main;
        var brain = mainCam != null ? mainCam.GetComponent<CinemachineBrain>() : null;
        if (switchToPlayerVCamOnStop)
        {
            var vcams = FindObjectsOfType<CinemachineVirtualCamera>();
            CinemachineVirtualCamera playerVCam = null;
            for (int i = 0; i < vcams.Length; i++)
            {
                var v = vcams[i];
                if (v != null && v.name == playerVCamName) playerVCam = v;
            }
            if (playerVCam != null)
            {
                playerVCam.Priority = 100;
                for (int i = 0; i < vcams.Length; i++)
                {
                    var v = vcams[i];
                    if (v == null || v == playerVCam) continue;
                    v.Priority = 0;
                }
            }
        }
        if (disableCinemachineBrainOnStop && brain != null)
        {
            brain.enabled = false;
        }
    }

    bool HasRequiredItem(int itemId)
    {
        var bm = BackpackManager.Instance;
        if (bm == null) return false;
        for (int slot = 0; slot < bm.maxLevelCount; slot++)
        {
            if (bm.GetItemIdInSlot(slot) == itemId) return true;
        }
        return false;
    }
}
