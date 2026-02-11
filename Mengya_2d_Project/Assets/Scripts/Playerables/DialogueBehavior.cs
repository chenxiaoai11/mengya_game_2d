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
    public Sprite portrait;

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
            UIManager.instance.StartTypewriter(characterName, dialogueLine, dialogueSize, portrait);
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
            // ��ͣTimelineǰ����ǿ����ɴ��ֻ�����ѡ������������Ƿ�ֱ����ʾȫ����
            UIManager.instance.CompleteTypewriter();
            GameManager.instance.PauseTimeline(playableDirector);
        }
        else
        {
            UIManager.instance.ToggleDialogueBox(false);
        }
    }
}
