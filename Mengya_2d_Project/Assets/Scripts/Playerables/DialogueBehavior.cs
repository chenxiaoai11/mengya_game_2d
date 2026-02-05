using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class DialogueBehavior : PlayableBehaviour
{
    private PlayableDirector playableDirector;

    public string characterName;
    [TextArea(8, 1)] public string dialogueLine;
    public int dialogueSize;

    private bool isClipPlayed;
    public bool requirePause;
    private bool pauseScheduled;

    public override void OnPlayableCreate(Playable playable)
    {
        playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (isClipPlayed == false && info.weight > 0)
        {
            // 启动打字机效果（替代直接赋值文本）
            UIManager.instance.StartTypewriter(characterName, dialogueLine, dialogueSize);
            if (requirePause)
            {
                pauseScheduled = true;
            }
            isClipPlayed = true;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        isClipPlayed = false;
        Debug.Log("Clip is Stopped");
        if (pauseScheduled)
        {
            pauseScheduled = false;
            // 暂停Timeline前，先强制完成打字机（可选：按需求决定是否直接显示全部）
            UIManager.instance.CompleteTypewriter();
            GameManager.instance.PauseTimeline(playableDirector);
        }
        else
        {
            UIManager.instance.ToggleDialogueBox(false);
        }
    }
}