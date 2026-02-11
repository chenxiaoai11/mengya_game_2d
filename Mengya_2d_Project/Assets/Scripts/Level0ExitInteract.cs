using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Level0ExitInteract : MonoBehaviour, IInteractable
{
    public ExitConfirmUI ui;
    public PlayableDirector timelineDirector;
    public string timelineLayerName = "Timeline";
    private int step = 0;
    private readonly string[] messages = new[]
    {
        "真的要走吗",
        "确定要走吗",
        "一定要走吗"
    };

    public bool CanInteract(GameObject player)
    {
        if (ui == null) Debug.LogWarning("[出口交互] UI未绑定");
        if (timelineDirector == null) Debug.LogWarning("[出口交互] Timeline未绑定");
        return true;
    }

    public void Interact(GameObject player)
    {
        Debug.Log("[出口交互] 收到Interact，开始显示确认步骤");
        step = 0;
        ShowStep();
    }

    private void ShowStep()
    {
        if (ui == null)
        {
            Debug.LogWarning("[出口交互] UI为空，无法显示确认");
            return;
        }
        if (step < messages.Length)
        {
            ui.Show(messages[step], OnConfirm, OnCancel);
            Debug.Log($"[出口交互] 显示提示: {messages[step]}");
        }
        else
        {
            StartTimeline();
        }
    }

    private void OnConfirm()
    {
        step++;
        ShowStep();
    }

    private void OnCancel()
    {
        ui?.Hide();
    }

    private void StartTimeline()
    {
        ui?.Hide();
        if (timelineDirector != null)
        {
            var bm = Object.FindObjectOfType<BlackoutManager>();
            if (bm != null) bm.BlackoutOnlyLayer(timelineLayerName);
            LogTimelineInfo("准备播放");
            timelineDirector.stopped -= OnTimelineStopped;
            timelineDirector.stopped += OnTimelineStopped;
            timelineDirector.Play();
            Debug.Log("[出口交互] 播放Timeline");
        }
        else
        {
            Debug.LogWarning("[出口交互] Timeline未设置，无法播放");
        }
    }

    private void LogTimelineInfo(string prefix)
    {
        var dir = timelineDirector;
        var asset = dir != null ? dir.playableAsset : null;
        var assetName = asset != null ? asset.name : "null";
        Debug.Log($"[出口交互] {prefix} | Director:{(dir!=null?dir.name:"null")} Asset:{assetName} State:{(dir!=null?dir.state.ToString():"null")}");
        var timelineAsset = asset as TimelineAsset;
        if (dir != null && timelineAsset != null)
        {
            foreach (var output in timelineAsset.outputs)
            {
                var src = output.sourceObject;
                var bind = dir.GetGenericBinding(src);
                var stream = output.streamName;
                var bindName = bind != null ? bind.name : "null";
                Debug.Log($"[出口交互] 轨道:{stream} 绑定:{bindName}");
            }
        }
    }

    private void OnTimelineStopped(PlayableDirector d)
    {
        timelineDirector.stopped -= OnTimelineStopped;
        var bm = Object.FindObjectOfType<BlackoutManager>();
        if (bm != null) bm.RestoreCameras();
        QuitGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
