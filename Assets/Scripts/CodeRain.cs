using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CodeRain : TerminalAttack
{
    [Header("Code Rain")]
    public float fallInterval = 0.06f;
    public float holdDuration = 2f;
    public int linesPerColumn = 12;

    private float columnWidth => arenaSize.x / 3f;

    protected override IEnumerator RunAttack()
    {
        isRunning = true;

        int column = Random.Range(0, 3);
        float colCenterX = -arenaSize.x / 2f + columnWidth * column + columnWidth / 2f;
        float colLeft    = -arenaSize.x / 2f + columnWidth * column;

        // Warning flash
        GameObject warning = SpawnWarningOverlay(
            new Vector2(colCenterX, 0f),
            new Vector2(columnWidth, arenaSize.y)
        );
        yield return new WaitForSeconds(warningPause);
        Destroy(warning);

        // Spawn falling lines
        float lineHeight = arenaSize.y / linesPerColumn;
        float startY = arenaSize.y / 2f - lineHeight / 2f;

        List<GameObject> spawnedLines = new List<GameObject>();
        List<GameObject> spawnedCols  = new List<GameObject>();

        for (int i = 0; i < linesPerColumn; i++)
        {
            float y = startY - i * lineHeight;

            GameObject lineObj = SpawnText(
                new Vector2(colLeft, y),
                columnWidth,
                lineHeight
            );
            TextMeshPro tmp = lineObj.GetComponent<TextMeshPro>();
            tmp.text = RandomLine();
            spawnedLines.Add(lineObj);

            GameObject col = SpawnColliderStrip(
                new Vector2(colCenterX, y),
                new Vector2(columnWidth, lineHeight)
            );
            spawnedCols.Add(col);

            yield return new WaitForSeconds(fallInterval);
        }

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Clear
        foreach (var obj in spawnedLines) if (obj != null) Destroy(obj);
        foreach (var obj in spawnedCols)  if (obj != null) Destroy(obj);

        isRunning = false;
    }
}