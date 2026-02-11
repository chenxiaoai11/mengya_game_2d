using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackoutManager : MonoBehaviour
{
    public static BlackoutManager Instance;
    private Canvas canvas;
    private Image overlay;
    public int sortingOrder = 10000;
    private Camera[] cachedCameras;
    private int[] cachedMasks;
    private CameraClearFlags[] cachedFlags;
    private Color[] cachedBgColors;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureOverlay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void EnsureOverlay()
    {
        if (canvas == null)
        {
            var go = new GameObject("BlackoutCanvas");
            go.transform.SetParent(transform, false);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
        }
        if (overlay == null)
        {
            var imgGo = new GameObject("Overlay");
            imgGo.transform.SetParent(canvas.transform, false);
            overlay = imgGo.AddComponent<Image>();
            var rt = overlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            overlay.color = new Color(0, 0, 0, 0);
        }
    }

    public void ShowBlack()
    {
        EnsureOverlay();
        overlay.color = new Color(0, 0, 0, 1);
    }

    public void FadeToBlack(float duration)
    {
        EnsureOverlay();
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(duration));
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            overlay.color = new Color(0, 0, 0, a);
            yield return null;
        }
        overlay.color = new Color(0, 0, 0, 1);
    }

    public void BlackoutOnlyLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogWarning($"[Blackout] 指定层不存在: {layerName}，将仅设置黑色背景");
        }
        var cams = Camera.allCameras;
        cachedCameras = cams;
        cachedMasks = new int[cams.Length];
        cachedFlags = new CameraClearFlags[cams.Length];
        cachedBgColors = new Color[cams.Length];
        int mask = (layer >= 0) ? (1 << layer) : 0;
        for (int i = 0; i < cams.Length; i++)
        {
            var cam = cams[i];
            cachedMasks[i] = cam.cullingMask;
            cachedFlags[i] = cam.clearFlags;
            cachedBgColors[i] = cam.backgroundColor;
            cam.cullingMask = mask;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
        if (overlay != null) overlay.color = new Color(0, 0, 0, 0);
    }

    public void RestoreCameras()
    {
        if (cachedCameras == null) return;
        for (int i = 0; i < cachedCameras.Length; i++)
        {
            var cam = cachedCameras[i];
            if (cam == null) continue;
            cam.cullingMask = cachedMasks[i];
            cam.clearFlags = cachedFlags[i];
            cam.backgroundColor = cachedBgColors[i];
        }
        cachedCameras = null;
        if (overlay != null) overlay.color = new Color(0, 0, 0, 0);
    }
}
