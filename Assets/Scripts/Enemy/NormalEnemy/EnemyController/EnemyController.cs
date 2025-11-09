using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    public enum MoveType { WAY, PINGPONG, BUCLE }
    public MoveType moveType = MoveType.WAY;

    [Header("Waypoints (PathPoints asignados)")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Configuración")]
    public float moveDuration = 0.4f;
    public Ease moveEase = Ease.InOutQuad;

    private int currentIndex = 1;
    private bool isMovingForward = true;
    private bool isMoving = false;
    private bool isPaused = false;

    [HideInInspector] public PathPoint currentPoint;
    [HideInInspector] public PathPoint nextPoint;

    private EnemyStatsController enemyStats;

    public System.Action OnEnemyClash;

    private void Start()
    {
        if (waypoints.Count < 2)
        {
            Debug.LogWarning($"{name}: se necesitan al menos 2 waypoints.");
            enabled = false;
            return;
        }

        // Estado inicial
        transform.position = waypoints[0].position;

        currentPoint = waypoints[0].GetComponent<PathPoint>();
        if (currentPoint != null)
        {
            currentPoint.state = PathPoint.EnemyState.EnemyHere;
            currentPoint.ownerEnemy = this;
        }

        nextPoint = waypoints[1].GetComponent<PathPoint>();
        if (nextPoint != null)
        {
            nextPoint.state = PathPoint.EnemyState.EnemyNext;
            nextPoint.ownerEnemy = this;
        }

        // 🔹 Referencia al EnemyStatsController
        enemyStats = GetComponent<EnemyStatsController>();

        if (PlayerGridMovement.Instance != null)
            PlayerGridMovement.Instance.OnPlayerMoved += HandlePlayerMove;
    }

    private void OnDestroy()
    {
        if (PlayerGridMovement.Instance != null)
            PlayerGridMovement.Instance.OnPlayerMoved -= HandlePlayerMove;
    }

    private void HandlePlayerMove()
    {
        var player = PlayerGridMovement.Instance;
        if (player == null) return;

        // Si estamos pausados, reanudamos solo cuando el jugador se haya ido de nuestro nextPoint
        if (isPaused)
        {
            if (!IsPlayerOnNextPoint())
            {
                isPaused = false;
                if (!isMoving)
                    MoveToNextWaypoint();
            }
            return;
        }

        // Evitar movernos si el jugador está sobre nuestro nextPoint y mirando hacia nosotros
        if (nextPoint != null)
        {
            bool playerOnNext = Vector2.Distance(player.transform.position, nextPoint.transform.position) < 0.05f;

            if (playerOnNext)
            {
                Vector2 intent = player.intendedDirection;
                if (intent != Vector2.zero)
                {
                    Vector2 dirToEnemy = ((Vector2)currentPoint.transform.position - (Vector2)player.transform.position).normalized;
                    float dot = Vector2.Dot(dirToEnemy, intent.normalized);

                    // Si el jugador intenta moverse hacia el enemigo (dot > 0.9)
                    if (dot > 0.9f)
                    {
                        // Detenemos y hacemos shake
                        DoShake();
                        PauseMovement(true);
                        OnEnemyClashes();
                        return;
                    }
                }
            }
        }

        // Si no estamos moviendo ni pausados, mover normalmente
        if (!isMoving)
            MoveToNextWaypoint();
    }

    public void MoveToNextWaypoint()
    {
        if (isMoving || isPaused) return;
        if (waypoints.Count < 2) return;

        isMoving = true;
        Vector3 targetPos = waypoints[currentIndex].position;

        transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                isMoving = false;

                if (currentPoint != null)
                    currentPoint.state = PathPoint.EnemyState.NoneEnemy;

                if (nextPoint != null)
                {
                    nextPoint.state = PathPoint.EnemyState.EnemyHere;
                    currentPoint = nextPoint;
                }

                UpdateIndex();

                var newNext = waypoints[currentIndex].GetComponent<PathPoint>();
                if (newNext != null)
                {
                    newNext.state = PathPoint.EnemyState.EnemyNext;
                    newNext.ownerEnemy = this;
                    nextPoint = newNext;
                }
            });
    }

    private void UpdateIndex()
    {
        switch (moveType)
        {
            case MoveType.WAY:
                if (currentIndex < waypoints.Count - 1)
                    currentIndex++;
                break;
            case MoveType.PINGPONG:
                if (isMovingForward)
                {
                    currentIndex++;
                    if (currentIndex >= waypoints.Count - 1)
                        isMovingForward = false;
                }
                else
                {
                    currentIndex--;
                    if (currentIndex <= 0)
                        isMovingForward = true;
                }
                break;
            case MoveType.BUCLE:
                currentIndex++;
                if (currentIndex >= waypoints.Count)
                    currentIndex = 0;
                break;
        }
    }

    private bool IsPlayerOnNextPoint()
    {
        if (nextPoint == null || PlayerGridMovement.Instance == null)
            return false;

        return Vector2.Distance(PlayerGridMovement.Instance.transform.position, nextPoint.transform.position) < 0.1f;
    }

    public void PauseMovement(bool pause)
    {
        isPaused = pause;

        if (pause && isMoving)
        {
            DOTween.Kill(transform);
            isMoving = false;
        }
        else if (!pause && !isMoving)
        {
            MoveToNextWaypoint();
        }
    }

    public void DoShake()
    {
        transform.DOShakePosition(0.05f, 0.05f, 10, 90);
    }

public void OnEnemyClashes()
{
    // El enemigo recibe daño del jugador
    if (enemyStats != null && PlayerStatsController.Instance != null)
    {
        enemyStats.ReceiveDamage(PlayerStatsController.Instance.attackDamage);
    }
}

}
