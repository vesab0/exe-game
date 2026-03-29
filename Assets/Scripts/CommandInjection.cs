using UnityEngine;
using System.Collections;
using TMPro;

public class CommandInjection : TerminalAttack
{
    [Header("Command Injection")]
    public float typingInterval = 0.08f;
    public GameObject cursorPrefab;
    public int projectilesPerSide = 8;
    public float wallOffset = 0.3f;

    private Vector2 textPosition => new Vector2(
        -arenaSize.x / 2f + 1.5f,
         arenaSize.y / 2f - 1f
    );

    protected override IEnumerator RunAttack()
    {
        isRunning = true;

        string command = RandomLine();

        GameObject textObj = SpawnText(textPosition, arenaSize.x - 1f, 2f);
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
        tmp.text = "";
        tmp.color = warningColor;

        yield return new WaitForSeconds(warningPause);

        GameObject cursorObj = null;

        for (int i = 0; i < command.Length; i++)
        {
            string typed = command.Substring(0, i + 1);
            tmp.text = typed;

            Vector2 cursorPos = GetCursorWorldPos(textObj, textPosition);

            if (cursorObj == null)
                cursorObj = SpawnCursor(cursorPos, cursorPrefab);
            else
                cursorObj.transform.position = cursorPos;

            if ((float)i / command.Length > 0.75f)
            {
                tmp.color = dangerColor;
                SpriteRenderer sr = cursorObj != null ? cursorObj.GetComponent<SpriteRenderer>() : null;
                if (sr != null) sr.color = dangerColor;
            }

            yield return new WaitForSeconds(typingInterval);
        }

        tmp.text = command + " EXECUTING...";
        yield return new WaitForSeconds(0.4f);
        FireFromAllWalls(projectilesPerSide, wallOffset);

        // Blink cursor while bullets fly
        float travelTime = arenaSize.x / projectileSpeed;
        SpriteRenderer cursorSr = cursorObj != null ? cursorObj.GetComponent<SpriteRenderer>() : null;
        yield return StartCoroutine(BlinkSprite(cursorSr, travelTime, typingInterval));

        if (cursorObj != null) Destroy(cursorObj);
        yield return new WaitForSeconds(0.1f);
        Destroy(textObj);

        isRunning = false;
    }
}