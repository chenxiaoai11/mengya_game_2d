using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueAutoHideOnClipEnd : MonoBehaviour
{
    public PlayableDirector director;
    public string dialogueTrackNameContains = "Dialogue";
    public float epsilon = 0.01f;
    private TimelineAsset asset;
    private readonly List<(double start, double end)> ranges = new List<(double, double)>();
    private double lastDirectorTime = -1;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        RefreshRanges();
    }

    void OnEnable()
    {
        RefreshRanges();
    }

    void Update()
    {
        if (director == null) return;
        if (asset == null) RefreshRanges();
        double t = director.time;
        if (Math.Abs(t - lastDirectorTime) < 1e-6) return;
        lastDirectorTime = t;
        bool inside = false;
        for (int i = 0; i < ranges.Count; i++)
        {
            var r = ranges[i];
            if (t >= r.start - epsilon && t <= r.end + epsilon) { inside = true; break; }
        }
        if (!inside) HideDialogue();
    }

    private void RefreshRanges()
    {
        ranges.Clear();
        asset = director != null ? director.playableAsset as TimelineAsset : null;
        if (asset == null) return;
        foreach (var track in asset.GetOutputTracks())
        {
            if (track == null) continue;
            if (!string.IsNullOrEmpty(dialogueTrackNameContains) && !track.name.Contains(dialogueTrackNameContains)) continue;
            foreach (var clip in track.GetClips())
            {
                var start = clip.start;
                var end = clip.end;
                ranges.Add((start, end));
            }
        }
    }

    private void HideDialogue()
    {
        var uiManagerType = Type.GetType("UIManager");
        if (uiManagerType != null)
        {
            var instField = uiManagerType.GetField("instance", BindingFlags.Public | BindingFlags.Static);
            var instProp = uiManagerType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
            object instance = instField != null ? instField.GetValue(null) : (instProp != null ? instProp.GetValue(null) : null);
            if (instance != null)
            {
                var toggleMethod = uiManagerType.GetMethod("ToggleDialogueBox", BindingFlags.Public | BindingFlags.Instance);
                if (toggleMethod != null) toggleMethod.Invoke(instance, new object[] { false });
                var spaceMethod = uiManagerType.GetMethod("ToggleSpaceBar", BindingFlags.Public | BindingFlags.Instance);
                if (spaceMethod != null) spaceMethod.Invoke(instance, new object[] { false });
            }
        }
    }
}
