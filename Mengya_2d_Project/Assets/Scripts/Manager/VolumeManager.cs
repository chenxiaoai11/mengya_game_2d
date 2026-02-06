using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class VolumeManager : MonoBehaviour
{
    [Header("UI配置")]
    public GameObject volumeUI;
    public Slider masterVolumeSlider;
    public Slider soundEffectSlider;

    [Header("双滑条背景透明度相关（可选）")]
    public Image masterVolumeBg;
    public Image soundEffectBg;

    [Header("控制配置")]
    public KeyCode toggleVolumeKey = KeyCode.Escape;
    public PlayerMovement playerMovement;
    public List<MonoBehaviour> backgroundInteractiveScripts;

    [Header("双滑条背景透明度范围（0=最小，1=最大）")]
    [Range(0f, 1f)] public float masterMinAlpha = 0.2f;
    [Range(0f, 1f)] public float masterMaxAlpha = 0.9f;
    [Range(0f, 1f)] public float soundMinAlpha = 0.2f;
    [Range(0f, 1f)] public float soundMaxAlpha = 0.9f;

    [Header("音量默认值")]
    public float defaultMasterVolume = 1f;
    public float defaultSoundEffectVolume = 1f;
    private float currentMasterVolume;
    private float currentSoundEffectVolume;

    [Header("音效音频源")]
    public List<AudioSource> soundEffectSources = new List<AudioSource>();

    private bool isVolumeUIOpen = false;

    void Awake()
    {
        if (volumeUI != null)
        {
            volumeUI.SetActive(false);
            InitTransparentMask();
        }

        InitDoubleBgTransparency();

        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogWarning("未找到PlayerMovement组件，无法锁定玩家移动");
            }
        }

        InitMasterVolumeSlider();
        InitSoundEffectSlider();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleVolumeKey))
        {
            ToggleVolumeUI();
        }
    }

    private void ToggleVolumeUI()
    {
        if (volumeUI == null)
        {
            Debug.LogWarning("未赋值VolumeUI，无法切换显示");
            return;
        }

        isVolumeUIOpen = !volumeUI.activeSelf;
        volumeUI.SetActive(isVolumeUIOpen);

        // 玩家移动和鼠标控制修复核心
        if (playerMovement != null)
        {
            if (isVolumeUIOpen)
            {
                playerMovement.LockPlayerMovement();
                // UI打开时强制显示鼠标
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                playerMovement.UnlockPlayerMovement();
                // 延迟恢复鼠标，避免帧同步问题
                Invoke(nameof(RestoreCursorForInteraction), 0.1f);
            }
        }
        else
        {
            // 无玩家组件时直接控制鼠标
            Cursor.lockState = isVolumeUIOpen ? CursorLockMode.None : CursorLockMode.None;
            Cursor.visible = true;
        }

        // 启用/禁用背景交互
        ToggleBackgroundInteraction(!isVolumeUIOpen);
    }

    // 新增：恢复鼠标交互状态
    private void RestoreCursorForInteraction()
    {
        Cursor.lockState = CursorLockMode.None; // 鼠标可自由移动
        Cursor.visible = true; // 显示鼠标
    }

    #region 背景遮挡核心逻辑
    private void InitTransparentMask()
    {
        Image mask = volumeUI.GetComponent<Image>();
        if (mask == null)
        {
            mask = volumeUI.AddComponent<Image>();
            mask.color = new Color(0, 0, 0, 0);
        }
        mask.raycastTarget = true;
    }

    private void ToggleBackgroundInteraction(bool isEnable)
    {
        if (backgroundInteractiveScripts == null) return;

        foreach (var script in backgroundInteractiveScripts)
        {
            if (script != null)
            {
                script.enabled = isEnable;
            }
        }
    }
    #endregion

    #region 原有音量控制逻辑
    private void InitDoubleBgTransparency()
    {
        float initMasterVol = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        UpdateMasterBgAlpha(initMasterVol);

        float initSoundVol = PlayerPrefs.GetFloat("SoundEffectVolume", defaultSoundEffectVolume);
        UpdateSoundBgAlpha(initSoundVol);
    }

    private void UpdateMasterBgAlpha(float masterVolValue)
    {
        if (masterVolumeBg == null) return;
        float targetAlpha = Mathf.Lerp(masterMinAlpha, masterMaxAlpha, masterVolValue);
        Color bgColor = masterVolumeBg.color;
        bgColor.a = targetAlpha;
        masterVolumeBg.color = bgColor;
    }

    private void UpdateSoundBgAlpha(float soundVolValue)
    {
        if (soundEffectBg == null) return;
        float targetAlpha = Mathf.Lerp(soundMinAlpha, soundMaxAlpha, soundVolValue);
        Color bgColor = soundEffectBg.color;
        bgColor.a = targetAlpha;
        soundEffectBg.color = bgColor;
    }

    private void InitMasterVolumeSlider()
    {
        if (masterVolumeSlider != null)
        {
            currentMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            AudioListener.volume = currentMasterVolume;
            masterVolumeSlider.value = currentMasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(UpdateMasterBgAlpha);
        }
        else
        {
            Debug.LogWarning("未赋值MasterVolumeSlider，无法初始化主音量");
        }
    }

    private void InitSoundEffectSlider()
    {
        if (soundEffectSlider != null)
        {
            currentSoundEffectVolume = PlayerPrefs.GetFloat("SoundEffectVolume", defaultSoundEffectVolume);
            UpdateAllSoundEffectVolume();
            soundEffectSlider.value = currentSoundEffectVolume;
            soundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
            soundEffectSlider.onValueChanged.AddListener(UpdateSoundBgAlpha);
        }
        else
        {
            Debug.LogWarning("未赋值SoundEffectSlider，无法初始化音效音量");
        }
    }

    private void OnMasterVolumeChanged(float volumeValue)
    {
        currentMasterVolume = volumeValue;
        AudioListener.volume = currentMasterVolume;
        PlayerPrefs.SetFloat("MasterVolume", currentMasterVolume);
        PlayerPrefs.Save();
        Debug.Log($"当前主音量{currentMasterVolume:F2}");
    }

    private void OnSoundEffectVolumeChanged(float volumeValue)
    {
        currentSoundEffectVolume = volumeValue;
        UpdateAllSoundEffectVolume();
        PlayerPrefs.SetFloat("SoundEffectVolume", currentSoundEffectVolume);
        PlayerPrefs.Save();
        Debug.Log($"当前音效音量{currentSoundEffectVolume:F2}");
    }

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
    #endregion
}