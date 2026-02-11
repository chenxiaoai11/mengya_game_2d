using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public enum GameMode
    {
        GamePlay,
        DialogueMoment
    }
    public GameMode gameMode;
    private PlayableDirector currentPlayableDirector;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);

        gameMode = GameMode.GamePlay;
    }

    private void Update()
    {
        if (gameMode == GameMode.DialogueMoment)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResumeTimeline();
            }
        }
    }

    public void PauseTimeline(PlayableDirector _playableDirector)
    {
        currentPlayableDirector = _playableDirector;
        gameMode = GameMode.DialogueMoment;
        currentPlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);

        if (UIManager.instance != null) UIManager.instance.ToggleSpaceBar(true);
    }

    public void ResumeTimeline()
    {
        gameMode = GameMode.GamePlay;
        currentPlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);

        if (UIManager.instance != null)
        {
            UIManager.instance.ToggleSpaceBar(false);
            UIManager.instance.ToggleDialogueBox(true);
        }
    }
}
