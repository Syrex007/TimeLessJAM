using UnityEngine;
using DG.Tweening;

public class PlayerGridMovement : MonoBehaviour
{
    public static PlayerGridMovement Instance { get; private set; }

    [Header("Configuración de movimiento")]
    public float cellSize = 1f;
    public float moveDuration = 0.2f;
    public float holdMoveDelay = 0.15f;
    public LayerMask obstacleLayer;
    public LayerMask pathPointLayer;
    public LayerMask boxLayer;
    public GridSystem gridSystem;

    [Header("Animaciones")]
    public Animator animator;

    private bool isMoving = false;
    private float holdTimer = 0f;
    private Vector2 currentDirection = Vector2.zero;
    private PathPoint currentPathPoint;

    public Vector2 intendedDirection { get; private set; } = Vector2.zero;
    public System.Action OnPlayerMoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

    }

    private void Update()
    {
        if (PlayerStatsController.Instance != null && PlayerStatsController.Instance.IsDead())
            return;

        UpdateCurrentPathPoint();
        HandleInput();
    }

    private void UpdateCurrentPathPoint()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position, pathPointLayer);
        currentPathPoint = hit ? hit.GetComponent<PathPoint>() : null;
    }

  private string lastPlayedAnimation = "";
private float lastAnimNormalizedTime = 0f;

private void HandleInput()
{
    Vector2 input = Vector2.zero;
    float horizontal = 0f;
    float vertical = 0f;

    if (Input.GetKey(KeyCode.W)) vertical += 1f;
    if (Input.GetKey(KeyCode.S)) vertical -= 1f;
    if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
    if (Input.GetKey(KeyCode.D)) horizontal += 1f;

    // Priorizar una dirección si hay diagonal
    if (Mathf.Abs(horizontal) > 0 && Mathf.Abs(vertical) > 0)
    {
        if (currentDirection.x != 0)
            vertical = 0;
        else
            horizontal = 0;
    }

    input = new Vector2(horizontal, vertical);

    // Si no hay input, detenemos movimiento
    if (input == Vector2.zero)
    {
        currentDirection = Vector2.zero;
        intendedDirection = Vector2.zero;
        return;
    }

    // Animación según dirección
    if (animator != null)
    {
        string animToPlay = "";

        if (input == Vector2.up) animToPlay = "UpPlayer";
        else if (input == Vector2.down) animToPlay = "DownPlayer";
        else if (input == Vector2.left) animToPlay = "LeftPlayer";
        else if (input == Vector2.right) animToPlay = "RightPlayer";

        if (animToPlay != "")
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = state.normalizedTime % 1f;
            if (animToPlay != lastPlayedAnimation || normalizedTime >= 0.98f)
            {
                animator.Play(animToPlay, 0, 0f);
                lastPlayedAnimation = animToPlay;
            }
        }
    }

    // Solo intentar mover si no estamos moviéndonos
    if (!isMoving && input != currentDirection)
    {
        currentDirection = input;
        intendedDirection = currentDirection;
        TryMove(currentDirection);
        intendedDirection = Vector2.zero;
    }
}




    public void TryMove(Vector2 direction)
    {
        if (isMoving) return;
        if (direction == Vector2.zero) return;

        Vector3 startPos = transform.position;
        Vector2 targetPos = startPos + (Vector3)direction * cellSize;
        bool moved = false;

        // ===== Validar pared =====
        RaycastHit2D hitWall = Physics2D.Raycast(startPos, direction, cellSize, obstacleLayer);
        if (hitWall.collider != null)
        {
            if (currentPathPoint != null &&
                currentPathPoint.state == PathPoint.EnemyState.EnemyNext &&
                currentPathPoint.ownerEnemy != null)
            {
                EnemyController enemy = currentPathPoint.ownerEnemy;
                enemy.DoShake();
                enemy.PauseMovement(true);
                DoStableShake(startPos, 0.1f, 0.1f);
                OnPlayerEnemyClash();
            }

            InvokePlayerMoved();
            return;
        }

        // ===== Caja frente al jugador =====
        RaycastHit2D hitBox = Physics2D.Raycast(startPos, direction, cellSize, boxLayer);
        if (hitBox.collider != null)
        {
            BreakableBox box = hitBox.collider.GetComponent<BreakableBox>();
            if (box != null)
            {
                bool boxMoved = box.TryPush(direction, obstacleLayer, boxLayer, cellSize);
                if (!boxMoved)
                {
                    InvokePlayerMoved();
                    return;
                }
            }
        }

        // ===== Límites del grid =====
        if (gridSystem != null)
        {
            Vector2Int gridPos = gridSystem.GetGridPosition(targetPos);
            if (gridPos.x < 0 || gridPos.x >= gridSystem.width || gridPos.y < 0 || gridPos.y >= gridSystem.height)
            {
                InvokePlayerMoved();
                return;
            }
        }

        // ===== PathPoint destino =====
        Collider2D destHit = Physics2D.OverlapPoint(targetPos, pathPointLayer);
        PathPoint targetPoint = destHit ? destHit.GetComponent<PathPoint>() : null;

        // ==== Jugador está en EnemyNext ====
        if (currentPathPoint != null &&
            currentPathPoint.state == PathPoint.EnemyState.EnemyNext &&
            currentPathPoint.ownerEnemy != null)
        {
            EnemyController enemy = currentPathPoint.ownerEnemy;
            PathPoint enemyHere = enemy.currentPoint;

            if (enemyHere != null)
            {
                Vector2 dirToEnemy = ((Vector2)enemyHere.transform.position - (Vector2)startPos).normalized;
                float dot = Vector2.Dot(direction.normalized, dirToEnemy);

                if (dot > 0.9f)
                {
                    InvokePlayerMoved();
                    isMoving = true;
                    DoStableShake(startPos, 0.1f, 0.1f);
                    enemy.DoShake();
                    enemy.PauseMovement(true);
                    OnPlayerEnemyClash();
                    enemy.OnEnemyClashes();
                    return;
                }
                else
                {
                    enemy.PauseMovement(false);
                }
            }
        }

        // ==== Intento entrar a EnemyNext ====
        if (targetPoint != null &&
            targetPoint.state == PathPoint.EnemyState.EnemyNext &&
            targetPoint.ownerEnemy != null)
        {
            int enemyDmg = targetPoint.ownerEnemy.GetComponent<EnemyStatsController>().damage;
            PlayerStatsController.Instance.ReceiveDamage(enemyDmg);
            InvokePlayerMoved();

            isMoving = true;
            Vector3 pushPos = (Vector3)startPos + (Vector3)direction * 0.25f;

            DOTween.Kill(transform);
            transform.DOMove(pushPos, moveDuration * 0.35f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    transform.DOMove(startPos, moveDuration * 0.35f)
                        .SetEase(Ease.InOutQuad)
                        .OnComplete(() => isMoving = false);
                });
            return;
        }

        // ===== Movimiento normal =====
        moved = true;
        InvokePlayerMoved();
        isMoving = true;

        DOTween.Kill(transform);
        transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isMoving = false;
                transform.position = new Vector3(
                    Mathf.Round(transform.position.x * 100f) / 100f,
                    Mathf.Round(transform.position.y * 100f) / 100f,
                    transform.position.z
                );
            });
    }

    private void DoStableShake(Vector3 startPos, float duration, float strength)
    {
        DOTween.Kill(transform);

        float timer = 0f;
        Vector3 original = startPos;

        DOTween.To(() => 0f, x => timer = x, 1f, duration)
            .OnUpdate(() =>
            {
                Vector2 offset = Random.insideUnitCircle * strength * 0.08f;
                transform.position = original + (Vector3)offset;
            })
            .OnComplete(() =>
            {
                transform.position = original;
                isMoving = false;
            });
    }

    private void InvokePlayerMoved() => OnPlayerMoved?.Invoke();

    public void OnPlayerEnemyClash()
    {
        if (currentPathPoint != null && currentPathPoint.ownerEnemy != null)
        {
            EnemyController enemy = currentPathPoint.ownerEnemy;
            EnemyStatsController enemyStats = enemy.GetComponent<EnemyStatsController>();

            if (enemyStats != null && PlayerStatsController.Instance != null && enemyStats.currentHealth > 0)
            {
                PlayerStatsController.Instance.ReceiveDamage(enemyStats.damage);
            }
        }
    }
}
