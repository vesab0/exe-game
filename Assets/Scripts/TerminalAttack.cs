using UnityEngine;
using System.Collections;
using TMPro;

public abstract class TerminalAttack : MonoBehaviour
{
    [Header("Arena")]
    public Vector2 arenaSize = new Vector2(17f, 10f);

    [Header("Timing")]
    public float attackCooldown = 4f;
    public float warningPause = 0.6f;

    [Header("Visuals")]
    public TMP_FontAsset terminalFont;
    public Color warningColor = new Color(1f, 0.9f, 0.2f, 1f);
    public Color dangerColor  = new Color(1f, 0.2f, 0.1f);
    public Color textColor    = new Color(0.2f, 1f, 0.3f);
    public float fontSize = 1.8f;

    [Header("Projectiles")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;

    protected bool isRunning = false;

    void Start()
    {
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackCooldown);
            if (!isRunning)
                yield return StartCoroutine(RunAttack());
        }
    }

    // ── Override this in each child ──────────────────────────────────────────
    protected abstract IEnumerator RunAttack();

    // ── Shared: fire projectile ──────────────────────────────────────────────
    protected void FireProjectile(Vector2 pos, Vector2 dir)
    {
        GameObject p = Instantiate(projectilePrefab, pos, Quaternion.identity);
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * projectileSpeed;
        Destroy(p, 6f);
    }

    // ── Shared: fire from all 4 walls ────────────────────────────────────────
    protected void FireFromAllWalls(int projectilesPerSide, float wallOffset = 0.3f)
    {
        float halfW = arenaSize.x / 2f - wallOffset;
        float halfH = arenaSize.y / 2f - wallOffset;

        float spacingH = arenaSize.x / (projectilesPerSide + 1);
        float spacingV = arenaSize.y / (projectilesPerSide + 1);

        for (int i = 1; i <= projectilesPerSide; i++)
        {
            float xPos = -arenaSize.x / 2f + i * spacingH;
            float yPos = -arenaSize.y / 2f + i * spacingV;

            FireProjectile(new Vector2(xPos,  halfH), Vector2.down);
            FireProjectile(new Vector2(xPos, -halfH), Vector2.up);
            FireProjectile(new Vector2(-halfW, yPos), Vector2.right);
            FireProjectile(new Vector2( halfW, yPos), Vector2.left);
        }
    }

    // ── Shared: spawn text ───────────────────────────────────────────────────
    protected GameObject SpawnText(Vector2 pos, float width, float height)
    {
        GameObject obj = new GameObject("AttackText");
        obj.transform.position = new Vector3(pos.x, pos.y, 0f);

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.sortingOrder = 11;
        tmp.color = textColor;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Truncate;
        if (terminalFont != null) tmp.font = terminalFont;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);
        rt.pivot = new Vector2(0f, 0.5f);

        return obj;
    }

    // ── Shared: spawn warning overlay ────────────────────────────────────────
    protected GameObject SpawnWarningOverlay(Vector2 center, Vector2 size)
    {
        GameObject go = new GameObject("WarningOverlay");
        go.transform.position = new Vector3(center.x, center.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakePixelSprite();
        sr.color = warningColor;
        sr.sortingOrder = 20;

        return go;
    }

    // ── Shared: spawn collider strip ─────────────────────────────────────────
    protected GameObject SpawnColliderStrip(Vector2 pos, Vector2 size, string tag = "EnemyBullet")
    {
        GameObject go = new GameObject("AttackCollider");
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (!string.IsNullOrEmpty(tag)) go.tag = tag;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = true;

        return go;
    }

    // ── Shared: spawn cursor ─────────────────────────────────────────────────
    protected GameObject SpawnCursor(Vector2 pos, GameObject prefab)
    {
        GameObject obj;

        if (prefab != null)
        {
            obj = Instantiate(prefab, pos, Quaternion.identity);
        }
        else
        {
            obj = new GameObject("Cursor");
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(0.15f, 0.4f, 1f);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.color = warningColor;
            sr.sortingOrder = 10;
        }

        if (obj.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            col.isTrigger = true;
        }

        return obj;
    }

    // ── Shared: blink a sprite ───────────────────────────────────────────────
    protected IEnumerator BlinkSprite(SpriteRenderer sr, float duration, float interval)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        if (sr != null) sr.enabled = true;
    }

    // ── Shared: get cursor world pos from TMP ────────────────────────────────
    protected Vector2 GetCursorWorldPos(GameObject textObj, Vector2 fallbackPos)
    {
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
        tmp.ForceMeshUpdate();

        if (tmp.textInfo.characterCount == 0)
            return fallbackPos;

        TMP_CharacterInfo lastChar = tmp.textInfo.characterInfo[tmp.textInfo.characterCount - 1];
        Vector3 worldPos = textObj.transform.TransformPoint(
            new Vector3(lastChar.topRight.x + 0.2f, 0f, 0f)
        );

        return new Vector2(worldPos.x, fallbackPos.y);
    }

    // ── Shared: random terminal line ─────────────────────────────────────────
    protected string RandomLine()
    {
        return TerminalLines.lines[Random.Range(0, TerminalLines.lines.Count)];
    }

    // ── Shared: 1x1 white pixel sprite ──────────────────────────────────────
    protected Sprite MakePixelSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
}