using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class Level0IntroUIPanel : MonoBehaviour
{
    public PlayableDirector director;
    public string directorObjectName = "timeline_begin";
    public RectTransform panel;
    public Button closeButton;
    public float slideDuration = 0.4f;
    public float hiddenOffsetY = 800f;
    private Vector2 shownPos;
    private Vector2 hiddenPos;
    private bool isShown;
    private bool animating;

    void Awake()
    {
        if (director == null && !string.IsNullOrEmpty(directorObjectName))
        {
            var go = GameObject.Find(directorObjectName);
            if (go != null) director = go.GetComponent<PlayableDirector>();
        }
        if (panel != null)
        {
            shownPos = panel.anchoredPosition;
            hiddenPos = shownPos + new Vector2(0f, hiddenOffsetY);
            panel.anchoredPosition = hiddenPos;
            panel.gameObject.SetActive(false);
            isShown = false;
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }
    }

    void OnEnable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
            director.stopped += OnTimelineStopped;
        }
    }

    void OnDisable()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }

    void OnTimelineStopped(PlayableDirector d)
    {
        ShowPanel();
    }

    void ShowPanel()
    {
        if (panel == null) return;
        if (animating) return;
        panel.gameObject.SetActive(true);
        StartCoroutine(Slide(panel, panel.anchoredPosition, shownPos, slideDuration, true));
    }

    void HidePanel()
    {
        if (panel == null) return;
        if (animating) return;
        StartCoroutine(Slide(panel, panel.anchoredPosition, hiddenPos, slideDuration, false));
    }

    System.Collections.IEnumerator Slide(RectTransform rt, Vector2 from, Vector2 to, float duration, bool toShown)
    {
        animating = true;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            yield return null;
        }
        rt.anchoredPosition = to;
        isShown = toShown;
        animating = false;
        if (!isShown) rt.gameObject.SetActive(false);
    }
}
