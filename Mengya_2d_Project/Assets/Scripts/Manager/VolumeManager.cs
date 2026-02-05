using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 音量管理：控制音量UI显示 + 分别调节总音量/音效音量
/// </summary>
public class VolumeManager : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject volumeUI; // 音量UI根物体（挂在VolumeUI上）
    public Slider masterVolumeSlider; // 总音量滑块（MasterVolumeSlider）
    public Slider soundEffectSlider; // 音效音量滑块（SoundEffectSlider）

    [Header("控制设置")]
    public KeyCode toggleVolumeKey = KeyCode.Escape; // 切换UI的按键（默认Escape）
    public PlayerMovement playerMovement; // 玩家移动组件引用

    [Header("音量默认值")]
    public float defaultMasterVolume = 1f; // 默认总音量0~1
    public float defaultSoundEffectVolume = 1f; // 默认音效音量0~1
    private float currentMasterVolume; // 当前总音量
    private float currentSoundEffectVolume; // 当前音效音量

    [Header("音效音量控制的音频源")]
    public List<AudioSource> soundEffectSources = new List<AudioSource>(); // 音效音频源列表（需手动添加）

    void Awake()
    {
        // 初始化UI
        if (volumeUI != null)
        {
            volumeUI.SetActive(false); // 确保初始状态隐藏
        }

        // 自动获取玩家移动组件（如果未手动赋值）
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("未找到PlayerMovement组件，音量UI切换时无法控制玩家移动");
            }
        }

        // 初始化音量滑块
        InitMasterVolumeSlider();
        InitSoundEffectSlider();
    }

    void Update()
    {
        // 检测指定按键切换音量UI显示
        if (Input.GetKeyDown(toggleVolumeKey))
        {
            ToggleVolumeUI();
        }
    }

    /// <summary>
    /// 切换音量UI显示/隐藏状态
    /// </summary>
    private void ToggleVolumeUI()
    {
        if (volumeUI != null)
        {
            bool isActive = volumeUI.activeSelf;
            volumeUI.SetActive(!isActive);

            // 控制玩家移动：打开UI时锁定，关闭时解锁
            if (playerMovement != null)
            {
                if (!isActive) // UI从隐藏变显示 → 锁定移动
                {
                    playerMovement.LockPlayerMovement();
                }
                else // UI从显示变隐藏 → 解锁移动
                {
                    playerMovement.UnlockPlayerMovement();
                }
            }

            // 鼠标锁定/显示：显示UI时解锁鼠标，隐藏时锁定
            Cursor.lockState = !isActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !isActive;
        }
        else
        {
            Debug.LogWarning("未赋值VolumeUI，无法切换音量面板");
        }
    }

    /// <summary>
    /// 初始化总音量滑块
    /// </summary>
    private void InitMasterVolumeSlider()
    {
        if (masterVolumeSlider != null)
        {
            // 1. 读取保存的总音量（无则用默认值）
            currentMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            // 2. 应用到全局音量
            AudioListener.volume = currentMasterVolume;
            // 3. 设置滑块初始值
            masterVolumeSlider.value = currentMasterVolume;
            // 4. 绑定值变更事件
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        else
        {
            Debug.LogWarning("未赋值MasterVolumeSlider，无法调节总音量");
        }
    }

    /// <summary>
    /// 初始化音效音量滑块
    /// </summary>
    private void InitSoundEffectSlider()
    {
        if (soundEffectSlider != null)
        {
            // 1. 读取保存的音效音量（无则用默认值）
            currentSoundEffectVolume = PlayerPrefs.GetFloat("SoundEffectVolume", defaultSoundEffectVolume);
            // 2. 应用到所有音效音频源
            UpdateAllSoundEffectVolume();
            // 3. 设置滑块初始值
            soundEffectSlider.value = currentSoundEffectVolume;
            // 4. 绑定值变更事件
            soundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
        }
        else
        {
            Debug.LogWarning("未赋值SoundEffectSlider，无法调节音效音量");
        }
    }

    /// <summary>
    /// 总音量滑块值变更时的回调
    /// </summary>
    /// <param name="volumeValue">当前音量值（0~1）</param>
    private void OnMasterVolumeChanged(float volumeValue)
    {
        currentMasterVolume = volumeValue;
        // 设置Unity全局音量（通过AudioListener）
        AudioListener.volume = currentMasterVolume;
        // 保存音量设置
        PlayerPrefs.SetFloat("MasterVolume", currentMasterVolume);
        PlayerPrefs.Save();

        Debug.Log($"当前总音量：{currentMasterVolume:F2}");
    }

    /// <summary>
    /// 音效音量滑块值变更时的回调
    /// </summary>
    /// <param name="volumeValue">当前音量值（0~1）</param>
    private void OnSoundEffectVolumeChanged(float volumeValue)
    {
        currentSoundEffectVolume = volumeValue;
        // 更新所有音效音频源的音量
        UpdateAllSoundEffectVolume();
        // 保存音效音量设置
        PlayerPrefs.SetFloat("SoundEffectVolume", currentSoundEffectVolume);
        PlayerPrefs.Save();

        Debug.Log($"当前音效音量：{currentSoundEffectVolume:F2}");
    }

    /// <summary>
    /// 更新所有音效音频源的音量
    /// </summary>
    private void UpdateAllSoundEffectVolume()
    {
        foreach (AudioSource source in soundEffectSources)
        {
            if (source != null)
            {
                source.volume = currentSoundEffectVolume;
            }
        }
    }
}