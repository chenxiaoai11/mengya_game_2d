using System.Collections.Generic;
using UnityEngine;

public class ProximityOutline : MonoBehaviour
{
    public Color outlineColor = Color.yellow;
    public int outlinePixelThickness = 2;
    public bool initiallyEnabled = false;

    private PlayerMovement player;
    private SpriteRenderer src;
    private Transform outlineRoot;
    private readonly List<SpriteRenderer> clones = new List<SpriteRenderer>();
    private bool isActive;
    private Collider2D col;

    void Awake()
    {
        src = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerMovement>();
        CreateOutline();
        SetActive(initiallyEnabled);
    }

    void Update()
    {
        if (player == null || src == null) return;
        var d = Vector3.Distance(player.transform.position, transform.position);
        var inRadius = d <= player.pickUpRadius;
        var hover = inRadius && IsMouseHovering();
        var shouldEnable = hover;
        if (shouldEnable != isActive) SetActive(shouldEnable);
        if (outlineRoot != null && src != null)
        {
            for (int i = 0; i < clones.Count; i++)
            {
                var r = clones[i];
                if (r == null) continue;
                if (r.sprite != src.sprite) r.sprite = src.sprite;
                r.sortingLayerID = src.sortingLayerID;
                r.sortingOrder = src.sortingOrder - 1;
                r.material = src.sharedMaterial;
            }
        }
    }

    private void CreateOutline()
    {
        if (src == null || src.sprite == null) return;
        if (outlineRoot != null) return;
        outlineRoot = new GameObject("Outline").transform;
        outlineRoot.SetParent(transform);
        outlineRoot.localPosition = Vector3.zero;
        outlineRoot.localRotation = Quaternion.identity;
        outlineRoot.localScale = Vector3.one;

        var ppu = Mathf.Max(1, src.sprite.pixelsPerUnit);
        var step = outlinePixelThickness / (float)ppu;
        var dirs = new Vector2[]
        {
            new Vector2(-1, 0), new Vector2(1, 0),
            new Vector2(0, -1), new Vector2(0, 1),
            new Vector2(-1, -1), new Vector2(-1, 1),
            new Vector2(1, -1), new Vector2(1, 1)
        };
        for (int i = 0; i < dirs.Length; i++)
        {
            var dir = dirs[i];
            var g = new GameObject("o");
            g.transform.SetParent(outlineRoot);
            g.transform.localPosition = new Vector3(dir.x * step, dir.y * step, 0);
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            var r = g.AddComponent<SpriteRenderer>();
            r.sprite = src.sprite;
            r.color = outlineColor;
            r.sortingLayerID = src.sortingLayerID;
            r.sortingOrder = src.sortingOrder - 1;
            r.material = src.sharedMaterial;
            clones.Add(r);
        }
    }

    private void SetActive(bool enable)
    {
        isActive = enable;
        if (outlineRoot == null) return;
        outlineRoot.gameObject.SetActive(enable);
    }

    private bool IsMouseHovering()
    {
        var cam = Camera.main;
        if (cam == null) return false;
        var mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        if (col != null)
        {
            var hits = Physics2D.OverlapPointAll(mouseWorld);
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.transform == transform) return true;
                if (h.transform.IsChildOf(transform)) return true;
            }
            return false;
        }
        else
        {
            return src.bounds.Contains(mouseWorld);
        }
    }
}
