using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class GridSystem : MonoBehaviour
{
    [Header("Configuración de la Grilla")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1f;
    public Vector3 originPosition = Vector3.zero;

    private Dictionary<Vector2Int, GameObject> gridObjects = new Dictionary<Vector2Int, GameObject>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellPos = GetWorldPosition(x, y);
                Gizmos.DrawWireCube(cellPos + new Vector3(cellSize / 2f, cellSize / 2f, 0), Vector3.one * cellSize);
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return originPosition + new Vector3(x * cellSize, y * cellSize, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - originPosition.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - originPosition.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        Vector2Int gridPos = GetGridPosition(worldPos);
        gridPos.x = Mathf.Clamp(gridPos.x, 0, width - 1);
        gridPos.y = Mathf.Clamp(gridPos.y, 0, height - 1);
        return GetWorldPosition(gridPos.x, gridPos.y);
    }

    public bool IsInBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }
}
