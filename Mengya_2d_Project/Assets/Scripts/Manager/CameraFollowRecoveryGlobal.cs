using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using Cinemachine;

public class CameraFollowRecoveryGlobal : MonoBehaviour
{
    public static CameraFollowRecoveryGlobal Instance;
    public string playerVCamName = "PlayerVCam";
    public int activePriority = 100;
    public int inactivePriority = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureFollow();
        }
        else
        {
            if (Instance != this) Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureFollow();
    }

    void EnsureFollow()
    {
        var anyPlaying = false;
        var directors = Object.FindObjectsOfType<PlayableDirector>();
        for (int i = 0; i < directors.Length; i++)
        {
            var d = directors[i];
            if (d != null && d.state == PlayState.Playing)
            {
                anyPlaying = true;
                break;
            }
        }
        if (anyPlaying) return;

        var vcams = Object.FindObjectsOfType<CinemachineVirtualCamera>();
        CinemachineVirtualCamera playerVCam = null;
        for (int i = 0; i < vcams.Length; i++)
        {
            var v = vcams[i];
            if (v != null && v.name == playerVCamName) playerVCam = v;
        }
        if (playerVCam != null)
        {
            playerVCam.Priority = activePriority;
            for (int i = 0; i < vcams.Length; i++)
            {
                var v = vcams[i];
                if (v == null || v == playerVCam) continue;
                v.Priority = inactivePriority;
            }
        }
        else
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var brain = cam.GetComponent<CinemachineBrain>();
                if (brain != null) brain.enabled = false;
                var follow2D = cam.GetComponent<CameraFollow2D>();
                if (follow2D != null) follow2D.enabled = true;
            }
        }
    }
}
