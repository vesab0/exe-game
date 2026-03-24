using UnityEngine;
using System.Collections.Generic;

public class MotherboardGenerator : MonoBehaviour
{
    [Header("Arena Size — match your quad")]
    public Vector2 arenaSize = new Vector2(10f, 10f);

    [Header("Settings")]
    public float gridSize = 0.4f;
    public int chipCount = 8;
    public int randomSeed = 77;

    [Header("Visuals")]
    public Sprite squareSprite;
    public Sprite circleSprite;

    // PCB gold trace colors
    readonly Color traceColor     = new Color(0.58f, 0.50f, 0.10f);
    readonly Color traceDim       = new Color(0.42f, 0.36f, 0.07f);
    readonly Color padOuter       = new Color(0.65f, 0.56f, 0.14f);
    readonly Color padHole        = new Color(0.08f, 0.18f, 0.08f);
    readonly Color chipBody       = new Color(0.07f, 0.12f, 0.07f);
    readonly Color chipPin        = new Color(0.55f, 0.48f, 0.10f);

    struct Chip
    {
        public Vector2 center;
        public Vector2 size;
        public List<Vector2> pins;
    }

    List<Chip> chips = new List<Chip>();

    void Start()
    {
        Random.InitState(randomSeed);
        PlaceChips();
        RouteTraces();
        ScatterSolderDots();
    }

    // ── 1. Place chips around the arena ─────────────────────────────────────
    void PlaceChips()
    {
        int attempts = 0;

        while (chips.Count < chipCount && attempts < 200)
        {
            attempts++;

            float w = Snap(Random.Range(0.8f, 2.5f));
            float h = Snap(Random.Range(0.4f, 1.2f));

            Vector2 pos = new Vector2(
                Snap(Random.Range(-arenaSize.x / 2f + w, arenaSize.x / 2f - w)),
                Snap(Random.Range(-arenaSize.y / 2f + h, arenaSize.y / 2f - h))
            );

            // Don't overlap existing chips
            bool overlaps = false;
            foreach (var c in chips)
            {
                if (Mathf.Abs(c.center.x - pos.x) < (c.size.x + w) / 2f + gridSize * 2f &&
                    Mathf.Abs(c.center.y - pos.y) < (c.size.y + h) / 2f + gridSize * 2f)
                {
                    overlaps = true;
                    break;
                }
            }
            if (overlaps) continue;

            Chip chip = new Chip
            {
                center = pos,
                size = new Vector2(w, h),
                pins = new List<Vector2>()
            };

            // Spawn chip body
            SpawnRect(pos, new Vector2(w, h), chipBody, 4);

            // Pins along top and bottom edges
            int pinCount = Mathf.RoundToInt(w / (gridSize * 1.2f));
            pinCount = Mathf.Clamp(pinCount, 2, 8);

            for (int i = 0; i < pinCount; i++)
            {
                float px = pos.x - w / 2f + gridSize + i * (w - gridSize) / Mathf.Max(pinCount - 1, 1);
                px = Snap(px);

                // Bottom pins
                Vector2 bottomPin = new Vector2(px, pos.y - h / 2f - gridSize * 0.5f);
                SpawnRect(bottomPin, new Vector2(gridSize * 0.4f, gridSize * 0.6f), chipPin, 5);
                chip.pins.Add(bottomPin);

                // Top pins
                Vector2 topPin = new Vector2(px, pos.y + h / 2f + gridSize * 0.5f);
                SpawnRect(topPin, new Vector2(gridSize * 0.4f, gridSize * 0.6f), chipPin, 5);
                chip.pins.Add(topPin);
            }

            // Pins along left and right edges
            int sidePins = Mathf.RoundToInt(h / (gridSize * 1.2f));
            sidePins = Mathf.Clamp(sidePins, 1, 4);

            for (int i = 0; i < sidePins; i++)
            {
                float py = pos.y - h / 2f + gridSize + i * (h - gridSize) / Mathf.Max(sidePins - 1, 1);
                py = Snap(py);

                Vector2 leftPin  = new Vector2(pos.x - w / 2f - gridSize * 0.5f, py);
                Vector2 rightPin = new Vector2(pos.x + w / 2f + gridSize * 0.5f, py);
                SpawnRect(leftPin,  new Vector2(gridSize * 0.6f, gridSize * 0.4f), chipPin, 5);
                SpawnRect(rightPin, new Vector2(gridSize * 0.6f, gridSize * 0.4f), chipPin, 5);
                chip.pins.Add(leftPin);
                chip.pins.Add(rightPin);
            }

            chips.Add(chip);
        }
    }

    // ── 2. Route traces between chip pins ───────────────────────────────────
    void RouteTraces()
    {
        // Connect pins between nearby chips with L-shaped routes
        for (int i = 0; i < chips.Count; i++)
        {
            for (int j = i + 1; j < chips.Count; j++)
            {
                float dist = Vector2.Distance(chips[i].center, chips[j].center);
                if (dist > arenaSize.x * 0.7f) continue;

                // Connect 1-3 pin pairs between these chips
                int connections = Random.Range(1, 4);
                int pinsA = chips[i].pins.Count;
                int pinsB = chips[j].pins.Count;

                for (int c = 0; c < connections; c++)
                {
                    if (pinsA == 0 || pinsB == 0) break;
                    Vector2 pinA = chips[i].pins[Random.Range(0, pinsA)];
                    Vector2 pinB = chips[j].pins[Random.Range(0, pinsB)];

                    bool isBundle = Random.value < 0.3f;
                    Color col = isBundle ? traceColor : traceDim;
                    float width = gridSize * (isBundle ? 0.14f : 0.09f);

                    RouteL(pinA, pinB, width, col);

                    // Bundle: draw 1-2 parallel traces offset slightly
                    if (isBundle)
                    {
                        float offset = gridSize * 0.22f;
                        RouteL(pinA + Vector2.up * offset, pinB + Vector2.up * offset, width * 0.7f, traceDim);
                        if (Random.value < 0.5f)
                            RouteL(pinA - Vector2.up * offset, pinB - Vector2.up * offset, width * 0.7f, traceDim);
                    }
                }
            }
        }
    }

    // Route an L-shaped trace from a to b (one horizontal + one vertical segment)
    void RouteL(Vector2 a, Vector2 b, float width, Color color)
    {
        // Snap both endpoints
        a = new Vector2(Snap(a.x), Snap(a.y));
        b = new Vector2(Snap(b.x), Snap(b.y));

        // Corner point — go horizontal first, then vertical
        bool hFirst = Random.value < 0.5f;
        Vector2 corner = hFirst
            ? new Vector2(b.x, a.y)
            : new Vector2(a.x, b.y);

        if (Vector2.Distance(a, corner) > 0.01f)
            SpawnSegment(a, corner, width, color);

        if (Vector2.Distance(corner, b) > 0.01f)
            SpawnSegment(corner, b, width, color);

        // Small pad at the corner
        SpawnCircle(corner, width * 2.2f, padOuter, 4);
    }

    // ── 3. Scatter solder dots that aren't on chips ──────────────────────────
    void ScatterSolderDots()
    {
        int count = 60;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = new Vector2(
                Snap(Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f)),
                Snap(Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f))
            );

            // Skip if inside a chip body
            bool onChip = false;
            foreach (var c in chips)
            {
                if (Mathf.Abs(pos.x - c.center.x) < c.size.x / 2f &&
                    Mathf.Abs(pos.y - c.center.y) < c.size.y / 2f)
                { onChip = true; break; }
            }
            if (onChip) continue;

            float size = gridSize * Random.Range(0.20f, 0.38f);
            SpawnCircle(pos, size * 1.9f, padOuter, 2);
            SpawnCircle(pos, size * 0.85f, padHole, 3);
        }
    }

    // ── Spawn helpers ────────────────────────────────────────────────────────
    float Snap(float v) => Mathf.Round(v / gridSize) * gridSize;

    void SpawnSegment(Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 mid = (a + b) / 2f;
        float len = Vector2.Distance(a, b);
        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

        GameObject go = new GameObject("Trace");
        go.transform.parent = transform;
        go.transform.position = new Vector3(mid.x, mid.y, 0f);
        go.transform.localScale = new Vector3(len, width, 1f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = squareSprite;
        sr.color = color;
        sr.sortingOrder = 2;
    }

    void SpawnRect(Vector2 pos, Vector2 size, Color color, int order)
    {
        GameObject go = new GameObject("Rect");
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
        GameObject go = new GameObject("Circle");
        go.transform.parent = transform;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = new Vector3(size, size, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite != null ? circleSprite : squareSprite;
        sr.color = color;
        sr.sortingOrder = order;
    }
}