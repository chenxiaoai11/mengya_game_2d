using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;
    [Header("UI����")]
    public GameObject volumeUI;
    public Slider masterVolumeSlider;
    public Slider soundEffectSlider;

    [Header("˫��������͸������أ���ѡ��")]
    public Image masterVolumeBg;
    public Image soundEffectBg;

    [Header("��������")]
    public KeyCode toggleVolumeKey = KeyCode.Escape;
    public PlayerMovement playerMovement;
    public List<MonoBehaviour> backgroundInteractiveScripts;

    [Header("˫��������͸���ȷ�Χ��0=��С��1=���")]
    [Range(0f, 1f)] public float masterMinAlpha = 0.2f;
    [Range(0f, 1f)] public float masterMaxAlpha = 0.9f;
    [Range(0f, 1f)] public float soundMinAlpha = 0.2f;
    [Range(0f, 1f)] public float soundMaxAlpha = 0.9f;

    [Header("����Ĭ��ֵ")]
    public float defaultMasterVolume = 1f;
    public float defaultSoundEffectVolume = 1f;
    private float currentMasterVolume;
    private float currentSoundEffectVolume;

    [Header("��Ч��ƵԴ")]
    public List<AudioSource> soundEffectSources = new List<AudioSource>();

    private bool isVolumeUIOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
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
                Debug.LogWarning("δ�ҵ�PlayerMovement������޷���������ƶ�");
            }
        }

        RebindSlidersIfNeeded();
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

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (volumeUI == null)
        {
            var uiObj = GameObject.Find("VolumeUI");
            if (uiObj != null) volumeUI = uiObj;
        }
        if (volumeUI != null)
        {
            volumeUI.SetActive(false);
            InitTransparentMask();
        }
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        RebindSlidersIfNeeded();
        InitMasterVolumeSlider();
        InitSoundEffectSlider();
    }

    private void ToggleVolumeUI()
    {
        if (volumeUI == null)
        {
            Debug.LogWarning("δ��ֵVolumeUI���޷��л���ʾ");
            return;
        }

        isVolumeUIOpen = !volumeUI.activeSelf;
        volumeUI.SetActive(isVolumeUIOpen);

        // ����ƶ����������޸�����
        if (playerMovement != null)
        {
            if (isVolumeUIOpen)
            {
                playerMovement.LockPlayerMovement();
                // UI��ʱǿ����ʾ���
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                playerMovement.UnlockPlayerMovement();
                // �ӳٻָ���꣬����֡ͬ������
                Invoke(nameof(RestoreCursorForInteraction), 0.1f);
            }
        }
        else
        {
            // ��������ʱֱ�ӿ������
            Cursor.lockState = isVolumeUIOpen ? CursorLockMode.None : CursorLockMode.None;
            Cursor.visible = true;
        }

        // ����/���ñ�������
        ToggleBackgroundInteraction(!isVolumeUIOpen);
    }

    // �������ָ���꽻��״̬
    private void RestoreCursorForInteraction()
    {
        Cursor.lockState = CursorLockMode.None; // ���������ƶ�
        Cursor.visible = true; // ��ʾ���
    }

    #region �����ڵ������߼�
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

    #region ԭ�����������߼�
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
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            currentMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            AudioListener.volume = currentMasterVolume;
            masterVolumeSlider.value = currentMasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            masterVolumeSlider.onValueChanged.AddListener(UpdateMasterBgAlpha);
        }
        else
        {
            Debug.LogWarning("δ��ֵMasterVolumeSlider���޷���ʼ��������");
        }
    }

    private void InitSoundEffectSlider()
    {
        if (soundEffectSlider != null)
        {
            soundEffectSlider.onValueChanged.RemoveAllListeners();
            currentSoundEffectVolume = PlayerPrefs.GetFloat("SoundEffectVolume", defaultSoundEffectVolume);
            UpdateAllSoundEffectVolume();
            soundEffectSlider.value = currentSoundEffectVolume;
            soundEffectSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
            soundEffectSlider.onValueChanged.AddListener(UpdateSoundBgAlpha);
        }
        else
        {
            Debug.LogWarning("δ��ֵSoundEffectSlider���޷���ʼ����Ч����");
        }
    }

    private void OnMasterVolumeChanged(float volumeValue)
    {
        currentMasterVolume = volumeValue;
        AudioListener.volume = currentMasterVolume;
        PlayerPrefs.SetFloat("MasterVolume", currentMasterVolume);
        PlayerPrefs.Save();
        Debug.Log($"��ǰ������{currentMasterVolume:F2}");
    }

    private void OnSoundEffectVolumeChanged(float volumeValue)
    {
        currentSoundEffectVolume = volumeValue;
        UpdateAllSoundEffectVolume();
        PlayerPrefs.SetFloat("SoundEffectVolume", currentSoundEffectVolume);
        PlayerPrefs.Save();
        Debug.Log($"��ǰ��Ч����{currentSoundEffectVolume:F2}");
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

    private void RebindSlidersIfNeeded()
    {
        if (volumeUI != null)
        {
            var sliders = volumeUI.GetComponentsInChildren<Slider>(true);
            if ((masterVolumeSlider == null || masterVolumeSlider.Equals(null)) && sliders.Length > 0)
            {
                masterVolumeSlider = sliders[0];
            }
            if ((soundEffectSlider == null || soundEffectSlider.Equals(null)) && sliders.Length > 1)
            {
                soundEffectSlider = sliders[1];
            }
            var images = volumeUI.GetComponentsInChildren<Image>(true);
            if ((masterVolumeBg == null || masterVolumeBg.Equals(null)) && images.Length > 0) masterVolumeBg = images[0];
            if ((soundEffectBg == null || soundEffectBg.Equals(null)) && images.Length > 1) soundEffectBg = images[1];
        }
    }
    #endregion
}
