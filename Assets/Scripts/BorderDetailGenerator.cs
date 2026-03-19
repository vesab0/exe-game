using UnityEngine;
using System.Collections.Generic;
public class BorderDetailGenerator : MonoBehaviour
{
    [Header("Must match BorderGenerator exactly")]
    public Vector2 innerSize = new Vector2(10f, 10f);
    public Vector2 outerSize = new Vector2(14f, 14f);
    public float gridSize = 0.4f;

    [Header("Detail Settings")]
    public int randomSeed = 99;

    [Header("Counts")]
    public int bracketCount = 60;
    public int microRectCount = 80;
    public int dotClusterCount = 20;
    public int floatingLineCount = 50;
    public int notchedRectCount = 30;

    public Sprite squareSprite;
    public Sprite circleSprite;

    Color[] darks = new Color[]
    {
        new Color(0.08f, 0.16f, 0.16f),
        new Color(0.12f, 0.22f, 0.20f),
        new Color(0.16f, 0.28f, 0.26f),
    };
    Color accent = new Color(0.0f, 0.50f, 0.45f, 0.5f);

    void Start()
    {
        Random.InitState(randomSeed);
        SpawnBrackets();
        SpawnMicroRects();
        SpawnDotClusters();
        SpawnFloatingLines();
        SpawnNotchedRects();
        ClipToInnerEdge();
    }

    void SpawnBrackets()
    {
        for (int i = 0; i < bracketCount; i++)
        {
            Vector2 pos = RandomBorderPos();
            float s = gridSize * Random.Range(0.5f, 1.4f);
            float t = gridSize * 0.15f;
            Color c = Random.value < 0.1f ? accent : darks[2];

            SpawnRect(pos, new Vector2(s, t), c, 5);
            SpawnRect(pos, new Vector2(t, s), c, 5);

            if (Random.value < 0.3f)
                SpawnRect(new Vector2(pos.x + s - t, pos.y), new Vector2(t, s * 0.5f), c, 5);
        }
    }

    void SpawnMicroRects()
    {
        for (int i = 0; i < microRectCount; i++)
        {
            Vector2 pos = RandomBorderPos();
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;

            float w = gridSize * Random.Range(0.3f, 1.0f);
            float h = gridSize * Random.Range(0.15f, 0.5f);
            if (Random.value < 0.5f) { float tmp = w; w = h; h = tmp; }

            Color c = darks[Random.Range(0, darks.Length)];
            if (Random.value < 0.08f) c = accent;

            SpawnRect(pos, new Vector2(w, h), c, 5);
        }
    }

    void SpawnDotClusters()
    {
        for (int i = 0; i < dotClusterCount; i++)
        {
            Vector2 center = RandomBorderPos();
            int cols = Random.Range(2, 5);
            int rows = Random.Range(1, 3);
            float spacing = gridSize * 0.55f;
            float dotSize = gridSize * 0.18f;
            Color c = Random.value < 0.15f ? accent : darks[2];

            for (int col = 0; col < cols; col++)
                for (int row = 0; row < rows; row++)
                    SpawnCircle(center + new Vector2(col * spacing, row * spacing), dotSize, c, 6);
        }
    }

    void SpawnFloatingLines()
    {
        float thickness = gridSize * 0.12f;
        for (int i = 0; i < floatingLineCount; i++)
        {
            Vector2 pos = RandomBorderPos();
            pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
            pos.y = Mathf.Round(pos.y / gridSize) * gridSize;

            float len = gridSize * Random.Range(1f, 4f);
            bool horizontal = Random.value < 0.5f;
            Vector2 size = horizontal ? new Vector2(len, thickness) : new Vector2(thickness, len);

            Color c = darks[2];
            if (Random.value < 0.06f) c = accent;

            SpawnRect(pos, size, c, 5);
        }
    }

    void SpawnNotchedRects()
    {
        for (int i = 0; i < notchedRectCount; i++)
        {
            Vector2 pos = RandomBorderPos();
            float w = gridSize * Random.Range(0.8f, 2.0f);
            float h = gridSize * Random.Range(0.8f, 2.0f);
            Color c = darks[Random.Range(0, darks.Length)];

            SpawnRect(pos, new Vector2(w, h), c, 3);

            float nw = w * Random.Range(0.2f, 0.45f);
            float nh = h * Random.Range(0.2f, 0.45f);

            int corner = Random.Range(0, 4);
            Vector2 notchPos = corner switch
            {
                0 => pos,
                1 => new Vector2(pos.x + w - nw, pos.y),
                2 => new Vector2(pos.x, pos.y + h - nh),
                _ => new Vector2(pos.x + w - nw, pos.y + h - nh),
            };

            SpawnRect(notchPos, new Vector2(nw, nh), new Color(0.04f, 0.09f, 0.09f), 4);
        }
    }

    void ClipToInnerEdge()
    {
        float halfIW = innerSize.x / 2f;
        float halfIH = innerSize.y / 2f;

        List<Transform> toDestroy = new List<Transform>();

        foreach (Transform child in transform)
        {
            Vector3 pos = child.position;
            Vector3 scale = child.lossyScale;

            float left   = pos.x - Mathf.Abs(scale.x) / 2f;
            float right  = pos.x + Mathf.Abs(scale.x) / 2f;
            float bottom = pos.y - Mathf.Abs(scale.y) / 2f;
            float top    = pos.y + Mathf.Abs(scale.y) / 2f;

            bool crossesX = right > -halfIW && left < halfIW;
            bool crossesY = top   > -halfIH && bottom < halfIH;

            if (crossesX && crossesY)
                toDestroy.Add(child);
        }

        foreach (Transform t in toDestroy)
            Destroy(t.gameObject);
    }

    void SpawnRect(Vector2 pos, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject("Detail");
        go.transform.parent = transform;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;
        sr.color = color;
        sr.sortingOrder = order;
    }

    void SpawnCircle(Vector2 pos, float size, Color color, int order)
    {
        GameObject go = new GameObject("Dot");
        go.transform.parent = transform;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size, size, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite != null ? circleSprite : squareSprite;
        sr.color = color;
        sr.sortingOrder = order;
    }

    Vector2 RandomBorderPos()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = Random.Range(-outerSize.x / 2f, outerSize.x / 2f);
            float y = Random.Range(-outerSize.y / 2f, outerSize.y / 2f);
            bool insideOuter = Mathf.Abs(x) < outerSize.x / 2f && Mathf.Abs(y) < outerSize.y / 2f;
            bool insideInner = Mathf.Abs(x) < (innerSize.x / 2f) - gridSize && Mathf.Abs(y) < (innerSize.y / 2f) - gridSize;
            if (insideOuter && !insideInner) return new Vector2(x, y);
        }
        return Vector2.zero;
    }
}