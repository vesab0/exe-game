using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BorderGenerator : MonoBehaviour
{
    [Header("Room Bounds")]
    public Vector2 innerSize = new Vector2(10f, 10f);
    public Vector2 outerSize = new Vector2(14f, 14f);

    [Header("Circuit Settings")]
    public float gridSize = 0.4f;
    public int traceCount = 40;
    public int maxTraceLength = 18;
    public int randomSeed = 42;

    [Header("Visuals")]
    public Sprite squareSprite;
    public Sprite circleSprite;

    Color[] palette = new Color[]
    {
        new Color(0.06f, 0.13f, 0.13f),
        new Color(0.10f, 0.20f, 0.20f),
        new Color(0.15f, 0.30f, 0.28f),
        new Color(0.00f, 0.55f, 0.50f, 0.6f),
    };

    HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    void Start()
    {
        Random.InitState(randomSeed);
        GenerateCircuitBorder();
        ClipToInnerEdge();
    }

    void GenerateCircuitBorder()
    {
        SpawnBaseFill();
        for (int i = 0; i < traceCount; i++)
        {
            Vector2Int startCell = GetRandomBorderCell();
            TracePath(startCell);
        }
    }

    void SpawnBaseFill()
    {
        float step = gridSize * 2f;
        float halfIW = innerSize.x / 2f;
        float halfIH = innerSize.y / 2f;
        float halfOW = outerSize.x / 2f;
        float halfOH = outerSize.y / 2f;

        for (float x = -halfOW; x < halfOW; x += step)
        {
            for (float y = -halfOH; y < halfOH; y += step)
            {
                if (Mathf.Abs(x) < halfIW && Mathf.Abs(y) < halfIH)
                    continue;

                float w = Random.Range(step * 0.8f, step * 2.2f);
                float h = Random.Range(step * 0.8f, step * 2.2f);

                // Clamp width so it doesn't cross the inner edge
                if (x < -halfIW && x + w > -halfIW) w = -halfIW - x;
                if (x >= halfIW && x - w < halfIW) w = x - halfIW;

                // Clamp height so it doesn't cross the inner edge
                if (y < -halfIH && y + h > -halfIH) h = -halfIH - y;
                if (y >= halfIH && y - h < halfIH) h = y - halfIH;

                if (w <= 0 || h <= 0) continue;

                SpawnRect(
                    new Vector2(x + w / 2f, y + h / 2f),
                    new Vector2(w, h),
                    palette[Random.Range(0, 2)],
                    Random.Range(0, 2)
                );
            }
        }
    }

    void TracePath(Vector2Int startCell)
    {
        Vector2Int cell = startCell;
        bool isAccent = Random.value < 0.08f;
        Color traceColor = isAccent ? palette[3] : palette[2];
        float traceWidth = isAccent ? gridSize * 0.25f : gridSize * 0.18f;

        SpawnPad(CellToWorld(cell), traceColor);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int lastDir = dirs[Random.Range(0, 4)];
        int length = Random.Range(3, maxTraceLength);

        for (int step = 0; step < length; step++)
        {
            Vector2Int dir;
            if (Random.value < 0.70f)
                dir = lastDir;
            else
            {
                if (lastDir.x != 0)
                    dir = Random.value < 0.5f ? Vector2Int.up : Vector2Int.down;
                else
                    dir = Random.value < 0.5f ? Vector2Int.left : Vector2Int.right;
            }

            Vector2Int nextCell = cell + dir;

            if (!IsInBorderZone(nextCell)) break;

            Vector2 nextWorld = CellToWorld(nextCell);
            if (Mathf.Abs(nextWorld.x) >= innerSize.x / 2f || Mathf.Abs(nextWorld.y) >= innerSize.y / 2f)
                break;

            SpawnSegment(CellToWorld(cell), nextWorld, traceWidth, traceColor);

            if (dir != lastDir || step == length - 1)
                SpawnPad(nextWorld, traceColor);

            occupiedCells.Add(nextCell);
            lastDir = dir;
            cell = nextCell;
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

    void SpawnRect(Vector2 pos, Vector2 size, Color color, int sortOrder)
    {
        GameObject go = new GameObject("Base");
        go.transform.parent = transform;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;
        sr.color = color;
        sr.sortingOrder = sortOrder;
    }

    void SpawnSegment(Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 mid = (a + b) / 2f;
        Vector2 delta = b - a;
        float len = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GameObject go = new GameObject("Trace");
        go.transform.parent = transform;
        go.transform.position = new Vector3(mid.x, mid.y, 0f);
        go.transform.localScale = new Vector3(len, width, 1f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;
        sr.color = color;
        sr.sortingOrder = 3;
    }

    void SpawnPad(Vector2 pos, Color color)
    {
        GameObject go = new GameObject("Pad");
        go.transform.parent = transform;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        float size = gridSize * 0.45f;
        go.transform.localScale = new Vector3(size, size, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite != null ? circleSprite : squareSprite;
        sr.color = color;
        sr.sortingOrder = 4;
    }

    Vector2 CellToWorld(Vector2Int cell)
    {
        return new Vector2(cell.x * gridSize, cell.y * gridSize);
    }

    Vector2Int GetRandomBorderCell()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = Random.Range(-(int)(outerSize.x / 2f / gridSize), (int)(outerSize.x / 2f / gridSize));
            int y = Random.Range(-(int)(outerSize.y / 2f / gridSize), (int)(outerSize.y / 2f / gridSize));
            Vector2Int cell = new Vector2Int(x, y);
            if (IsInBorderZone(cell)) return cell;
        }
        return Vector2Int.zero;
    }

    bool IsInBorderZone(Vector2Int cell)
    {
        Vector2 w = CellToWorld(cell);
        bool insideOuter = Mathf.Abs(w.x) < outerSize.x / 2f && Mathf.Abs(w.y) < outerSize.y / 2f;
        bool insideInner = Mathf.Abs(w.x) < (innerSize.x / 2f) - gridSize && Mathf.Abs(w.y) < (innerSize.y / 2f) - gridSize;
        return insideOuter && !insideInner;
    }
}