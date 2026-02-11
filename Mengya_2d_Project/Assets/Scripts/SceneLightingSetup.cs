using System;
using System.Reflection;
using UnityEngine;

public class SceneLightingSetup : MonoBehaviour
{
    private class LightMarker : MonoBehaviour {}
    public bool enableGlobalNight = true;
    public bool enablePerimeter = true;
    public bool enableCeiling = true;
    public bool enableExitSigns = true;
    public bool enableDoorRed = true;
    public bool enableRestrooms = true;
    public bool autoRebuildOnAwake = true;
    public bool clearExistingBeforeBuild = true;
    public bool forceSpriteFallback = false;
    public string spriteSortingLayerName = "Default";
    public int spriteSortingOrder = 1000;
    public bool autoCollectChildren = true;

    public Transform[] ceilingPoints;
    public Transform[] exitSignPoints;
    public Transform[] doorRedPoints;
    public Transform[] restroomPoints;
    public Transform[] perimeterPoints;

    public Color ceilingColor = new Color(1f, 0.95f, 0.85f, 1f);
    public Color exitGreenColor = new Color(0.3f, 1f, 0.3f, 1f);
    public Color doorRedColor = new Color(1f, 0.25f, 0.25f, 1f);
    public Color restroomColor = new Color(1f, 0.95f, 0.85f, 1f);
    public Color nightBlueColor = new Color(0.2f, 0.35f, 0.6f, 1f);

    public float pointOuterRadius = 3.5f;
    public float pointInnerRadius = 0.8f;
    public float pointIntensity = 1.0f;
    public float perimeterOuterRadius = 6.0f;
    public float perimeterInnerRadius = 1.0f;
    public float perimeterIntensity = 0.6f;
    public float globalNightIntensity = 0.2f;

    private Type light2DType;
    private Type lightTypeEnum;

    void Awake()
    {
        TryResolveLight2D();
        if (autoRebuildOnAwake)
        {
            EnsurePoints();
            Rebuild();
        }
    }

    private void TryResolveLight2D()
    {
        light2DType = Type.GetType("UnityEngine.Experimental.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null)
        {
            light2DType = Type.GetType("UnityEngine.Experimental.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal");
        }
        if (light2DType != null)
        {
            lightTypeEnum = light2DType.GetNestedType("LightType");
        }
    }

    private bool IsNullOrEmpty(Transform[] arr)
    {
        if (arr == null || arr.Length == 0) return true;
        for (int i = 0; i < arr.Length; i++) if (arr[i] != null) return false;
        return true;
    }

    private Transform[] CollectChildrenByPrefix(params string[] prefixes)
    {
        var list = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            var nameLower = c.name.ToLowerInvariant();
            for (int j = 0; j < prefixes.Length; j++)
            {
                var p = prefixes[j].ToLowerInvariant();
                if (nameLower.StartsWith(p))
                {
                    list.Add(c);
                    break;
                }
            }
        }
        return list.ToArray();
    }

    private void EnsurePoints()
    {
        if (!autoCollectChildren) return;
        if (IsNullOrEmpty(ceilingPoints)) ceilingPoints = CollectChildrenByPrefix("ceiling");
        if (IsNullOrEmpty(exitSignPoints)) exitSignPoints = CollectChildrenByPrefix("exit", "exit_signal", "exitsignal");
        if (IsNullOrEmpty(doorRedPoints)) doorRedPoints = CollectChildrenByPrefix("reddoor", "red", "doorred");
        if (IsNullOrEmpty(restroomPoints)) restroomPoints = CollectChildrenByPrefix("restroom", "wc", "toilet");
        if (IsNullOrEmpty(perimeterPoints)) perimeterPoints = CollectChildrenByPrefix("perimeter", "edge");
        Debug.Log($"点位统计 | ceiling:{(ceilingPoints==null?0:ceilingPoints.Length)} exit:{(exitSignPoints==null?0:exitSignPoints.Length)} red:{(doorRedPoints==null?0:doorRedPoints.Length)} restroom:{(restroomPoints==null?0:restroomPoints.Length)} perimeter:{(perimeterPoints==null?0:perimeterPoints.Length)}");
    }

    private void BuildLights()
    {
        if (clearExistingBeforeBuild) ClearLights();
        if (!forceSpriteFallback && light2DType != null)
        {
            Debug.Log("使用URP Light2D生成灯光");
            BuildURPLights();
        }
        else
        {
            Debug.Log("使用精灵加法混合生成灯光（fallback）");
            BuildFallbackSprites();
        }
    }

    public void Rebuild()
    {
        BuildLights();
    }

    public void ClearLights()
    {
        int removed = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.GetComponent<LightMarker>() != null ||
                c.name.StartsWith("LightSprite") ||
                c.name.StartsWith("PointLight2D") ||
                c.name.StartsWith("GlobalLight2D"))
            {
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
                removed++;
            }
        }
        Debug.Log($"清理灯光对象数量: {removed}");
    }

    private void BuildURPLights()
    {
        if (enableGlobalNight) CreateGlobalLight(nightBlueColor, globalNightIntensity);
        if (enableCeiling) CreatePoints(ceilingPoints, ceilingColor, pointIntensity, pointOuterRadius, pointInnerRadius);
        if (enableExitSigns) CreatePoints(exitSignPoints, exitGreenColor, pointIntensity, pointOuterRadius * 0.8f, pointInnerRadius * 0.6f);
        if (enableDoorRed) CreatePoints(doorRedPoints, doorRedColor, pointIntensity, pointOuterRadius, pointInnerRadius);
        if (enableRestrooms) CreatePoints(restroomPoints, restroomColor, pointIntensity, pointOuterRadius, pointInnerRadius);
        if (enablePerimeter) CreatePoints(perimeterPoints, nightBlueColor, perimeterIntensity, perimeterOuterRadius, perimeterInnerRadius);
    }

    private void CreateGlobalLight(Color c, float intensity)
    {
        var go = new GameObject("GlobalLight2D");
        go.transform.SetParent(transform, false);
        go.AddComponent<LightMarker>();
        var comp = go.AddComponent(light2DType);
        SetEnumProperty(comp, "lightType", "Global");
        SetColorProperty(comp, "color", c);
        SetFloatProperty(comp, "intensity", intensity);
    }

    private void CreatePoints(Transform[] points, Color c, float intensity, float outer, float inner)
    {
        if (points == null) return;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (p == null) continue;
            var go = new GameObject("PointLight2D");
            go.transform.SetParent(transform, false);
            go.transform.position = p.position;
            go.AddComponent<LightMarker>();
            var comp = go.AddComponent(light2DType);
            SetEnumProperty(comp, "lightType", "Point");
            SetColorProperty(comp, "color", c);
            SetFloatProperty(comp, "intensity", intensity);
            SetFloatProperty(comp, "pointLightOuterRadius", outer);
            SetFloatProperty(comp, "pointLightInnerRadius", inner);
        }
    }

    private void SetEnumProperty(Component comp, string name, string enumValue)
    {
        var prop = light2DType.GetProperty(name);
        if (prop == null || lightTypeEnum == null) return;
        var val = Enum.Parse(lightTypeEnum, enumValue);
        prop.SetValue(comp, val);
    }
    private void SetColorProperty(Component comp, string name, Color value)
    {
        var prop = light2DType.GetProperty(name);
        if (prop == null) return;
        prop.SetValue(comp, value);
    }
    private void SetFloatProperty(Component comp, string name, float value)
    {
        var prop = light2DType.GetProperty(name);
        if (prop == null) return;
        prop.SetValue(comp, value);
    }

    private void BuildFallbackSprites()
    {
        var tex = MakeRadialTexture(256, 0.6f);
        var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        if (enableCeiling) CreateSpriteLights(ceilingPoints, spr, ceilingColor, 3.0f);
        if (enableExitSigns) CreateSpriteLights(exitSignPoints, spr, exitGreenColor, 2.0f);
        if (enableDoorRed) CreateSpriteLights(doorRedPoints, spr, doorRedColor, 3.0f);
        if (enableRestrooms) CreateSpriteLights(restroomPoints, spr, restroomColor, 3.0f);
        if (enablePerimeter) CreateSpriteLights(perimeterPoints, spr, nightBlueColor, 5.0f);
    }

    private void CreateSpriteLights(Transform[] points, Sprite spr, Color c, float scale)
    {
        if (points == null) return;
        int created = 0;
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (p == null) continue;
            var go = new GameObject("LightSprite");
            go.transform.SetParent(transform, false);
            go.transform.position = p.position;
            go.transform.localScale = Vector3.one * scale;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = spr;
            r.color = Color.white;
            var sh = Shader.Find("Custom/AdditiveSprite");
            var mat = new Material(sh != null ? sh : Shader.Find("Legacy Shaders/Particles/Additive"));
            if (sh != null) mat.SetColor("_Color", c);
            r.material = mat;
            r.sortingLayerID = SortingLayer.NameToID(string.IsNullOrEmpty(spriteSortingLayerName) ? "Default" : spriteSortingLayerName);
            r.sortingOrder = spriteSortingOrder;
            go.AddComponent<LightMarker>();
            created++;
        }
        Debug.Log($"fallback光圈创建数量: {created}");
    }

    private Texture2D MakeRadialTexture(int size, float softness)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var cx = size * 0.5f;
        var cy = size * 0.5f;
        var maxR = Mathf.Min(cx, cy);
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - cx;
                var dy = y - cy;
                var d = Mathf.Sqrt(dx * dx + dy * dy) / maxR;
                var a = Mathf.Clamp01(1f - Mathf.SmoothStep(0f, 1f, (d - (1f - softness)) / softness));
                data[y * size + x] = new Color(1, 1, 1, a);
            }
        }
        tex.SetPixels(data);
        tex.Apply();
        return tex;
    }
}
