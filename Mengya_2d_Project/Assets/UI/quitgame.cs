using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameQuit : MonoBehaviour
{
    public void QuitGame()
    {
        // 如果是在编辑器中，停止播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}