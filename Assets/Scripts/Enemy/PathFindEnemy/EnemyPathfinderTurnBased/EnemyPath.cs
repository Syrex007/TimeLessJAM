using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Collections;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class EnemyPath : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveDuration = 0.2f;
    public LayerMask obstacleLayer;
    public int damage = 3;

    [Header("Grilla")]
    public Vector2 gridOrigin;
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 1f;

    [Header("Debug")]
    public bool showGrid = true;

    [Header("Player (Inspector)")]
    public Transform player;

    public List<Vector2> nodes = new List<Vector2>();
    private List<Vector2> currentPath = new List<Vector2>();

    private bool isMoving = false;

    private void Update()
    {
        if (isMoving) return;

        // detectar input W/A/S/D
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            GenerateNodes();
            CalculatePath();
            MoveOneStep();
        }
    }

    // ───────────────
    // GENERAR NODOS
    // ───────────────
    public void GenerateNodes()
    {
        nodes.Clear();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 pos = gridOrigin + new Vector2(x * cellSize + cellSize / 2f, y * cellSize + cellSize / 2f);
                nodes.Add(pos);
            }
        }
    }

    // ───────────────
    // PATHFINDING ORTOGONAL BFS
    // ───────────────
    private void CalculatePath()
    {
        currentPath.Clear();
        if (player == null || nodes.Count == 0) return;

        Vector2 start = GetClosestNode(transform.position);
        Vector2 target = GetClosestNode(player.position);

        Queue<Vector2> frontier = new Queue<Vector2>();
        Dictionary<Vector2, Vector2?> cameFrom = new Dictionary<Vector2, Vector2?>();
        frontier.Enqueue(start);
        cameFrom[start] = null;

        bool found = false;

        while (frontier.Count > 0)
        {
            Vector2 current = frontier.Dequeue();
            if (Vector2.Distance(current, target) < 0.01f)
            {
                found = true;
                target = current;
                break;
            }

            foreach (Vector2 neighbor in GetOrthogonalNeighbors(current))
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }
        }

        if (!found) return;

        // reconstruir camino
        Stack<Vector2> stack = new Stack<Vector2>();
        Vector2 temp = target;
        while (!Mathf.Approximately(Vector2.Distance(temp, start), 0f))
        {
            stack.Push(temp);
            temp = cameFrom[temp].Value;
        }

        currentPath.AddRange(stack);
    }

    private Vector2 GetClosestNode(Vector2 pos)
    {
        Vector2 closest = nodes[0];
        float minDist = Vector2.Distance(pos, closest);
        foreach (var n in nodes)
        {
            float dist = Vector2.Distance(pos, n);
            if (dist < minDist)
            {
                minDist = dist;
                closest = n;
            }
        }
        return closest;
    }

    private List<Vector2> GetOrthogonalNeighbors(Vector2 node)
    {
        List<Vector2> neighbors = new List<Vector2>();
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        foreach (var dir in dirs)
        {
            Vector2 neighbor = node + dir * cellSize;
            if (nodes.Contains(neighbor))
            {
                if (!Physics2D.OverlapBox(neighbor, Vector2.one * 0.9f * cellSize, 0, obstacleLayer))
                    neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    // ───────────────
    // MOVERSE 1 NODO
    // ───────────────
    private void MoveOneStep()
    {
        if (currentPath.Count == 0 || isMoving) return;

        Vector2 nextNode = currentPath[0];
        currentPath.RemoveAt(0);

        isMoving = true;
        DOTween.Kill(transform);
        transform.DOMove(nextNode, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isMoving = false;
                if (Vector2.Distance(transform.position, player.position) < 0.1f)
                    PlayerStatsController.Instance.ReceiveDamage(damage);
            });
    }


    // ───────────────
    // GIZMOS
    // ───────────────
    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        // dibujar grilla azul
        Gizmos.color = Color.blue;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellCenter = gridOrigin + new Vector2(x * cellSize + cellSize / 2f, y * cellSize + cellSize / 2f);
                Gizmos.DrawWireCube(cellCenter, Vector2.one * cellSize);
            }
        }

        // nodos
        Vector2 playerNode = player ? GetClosestNode(player.position) : Vector2.zero;

        foreach (var n in nodes)
        {
            bool occupied = Physics2D.OverlapBox(n, Vector2.one * 0.9f * cellSize, 0, obstacleLayer);
            if (n == playerNode)
                Gizmos.color = Color.green;
            else
                Gizmos.color = occupied ? Color.red : Color.cyan;

            Gizmos.DrawSphere(n, 0.1f);
        }

        // camino
        if (currentPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Vector2 prev = GetClosestNode(transform.position);
            foreach (var n in currentPath)
            {
                Gizmos.DrawLine(prev, n);
                prev = n;
            }
        }
    }
    
        private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerStatsController.Instance.ReceiveDamage(3);
        }
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(EnemyPath))]
    public class EnemyPathEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EnemyPath script = (EnemyPath)target;
            if (GUILayout.Button("Generar nodos ahora"))
                script.GenerateNodes();
        }
    }
#endif
}
