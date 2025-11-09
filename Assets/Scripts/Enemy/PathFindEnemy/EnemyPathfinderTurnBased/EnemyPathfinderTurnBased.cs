using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyPathfinderTurnBased : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveDuration = 0.2f;
    public float cellSize = 1f;
    public LayerMask obstacleMask;

    [Header("Área de Patrullaje")]
    public Transform minBoundPoint;
    public Transform maxBoundPoint;

    [Header("Daño")]
    public int damage = 3;

    private Transform player;
    private bool isMoving = false;

    private void Start()
    {
        player = PlayerGridMovement.Instance.transform;

        // Suscribirse al movimiento del jugador
        PlayerGridMovement.Instance.OnPlayerMoved += OnPlayerMoved;
    }

    private void OnDestroy()
    {
        if (PlayerGridMovement.Instance != null)
            PlayerGridMovement.Instance.OnPlayerMoved -= OnPlayerMoved;
    }

    private void OnPlayerMoved()
    {
        if (isMoving) return;

        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || minBoundPoint == null || maxBoundPoint == null)
            return;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;

        // Calcular dirección hacia el jugador
        Vector2 dir = (playerPos - enemyPos);
        Vector2 moveDir = Vector2.zero;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            moveDir.x = Mathf.Sign(dir.x); // mover horizontal
        else
            moveDir.y = Mathf.Sign(dir.y); // mover vertical

        Vector2 targetPos = enemyPos + moveDir * cellSize;

        // Respetar bounds
        if (targetPos.x < minBoundPoint.position.x || targetPos.x > maxBoundPoint.position.x ||
            targetPos.y < minBoundPoint.position.y || targetPos.y > maxBoundPoint.position.y)
            return;

        // Revisar obstáculos
        RaycastHit2D hit = Physics2D.Raycast(enemyPos, moveDir, cellSize * 0.9f, obstacleMask);
        if (hit.collider != null) return;

        // Si alcanza al jugador, infligir daño
        if (Vector2.Distance(targetPos, playerPos) < 0.1f)
        {
            PlayerStatsController.Instance.ReceiveDamage(damage);
            return;
        }

        // Mover al siguiente tile
        isMoving = true;
        DOTween.Kill(transform);
        transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => isMoving = false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatsController.Instance.ReceiveDamage(damage);
        }
    }

    // ──────────────────────────────
    //  VISUALIZACIÓN EN SCENE VIEW
    // ──────────────────────────────
    private void OnDrawGizmos()
    {
        if (minBoundPoint == null || maxBoundPoint == null) return;

        Vector2 min = minBoundPoint.position;
        Vector2 max = maxBoundPoint.position;

        Vector2 center = (min + max) / 2f;
        Vector2 size = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));

        Gizmos.color = new Color(0.1f, 0.6f, 1f, 0.2f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(0.1f, 0.6f, 1f, 1f);
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(min, 0.1f);
        Gizmos.DrawSphere(max, 0.1f);
    }
}
