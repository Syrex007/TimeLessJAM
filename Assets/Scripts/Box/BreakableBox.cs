using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class BreakableBox : MonoBehaviour
{
    [Header("Configuración")]
    public float moveDuration = 0.15f;          // Movimiento rápido y fluido
    public float shakeDuration = 0.3f;
    public float shakeStrength = 0.4f;
    public float respawnDelay = 0.8f;
    public LayerMask enemyLayer;
    public string enemyTag = "Enemy";

    private Vector3 initialPosition;
    private bool isBroken = false;
    private SpriteRenderer sr;
    private Tween moveTween;

    private void Start()
    {
        initialPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
            Break();
    }

    private void Break()
    {
        if (isBroken) return;
        isBroken = true;

        // Cancelar tweens activos
        DOTween.Kill(transform);
        if (moveTween != null && moveTween.IsActive())
            moveTween.Kill();

        // Efecto rápido de ruptura
        transform.DOScale(0.8f, 0.08f)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                sr.enabled = false;
                transform.localScale = Vector3.one;

                // Respawn tras breve delay
                DOVirtual.DelayedCall(respawnDelay, () =>
                {
                    transform.position = initialPosition;
                    sr.enabled = true;

                    // Pequeño shake al reaparecer
                    transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90)
                        .OnComplete(() => isBroken = false);
                });
            });
    }

    /// <summary>
    /// Empuje desde el jugador (dirección exacta, 1 unidad por casilla)
    /// </summary>
    public bool TryPush(Vector2 direction, LayerMask obstacleLayer, LayerMask boxLayer, float cellSize)
{
    if (isBroken) return false;

    // Normalizar dirección (solo un eje)
    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        direction = new Vector2(Mathf.Sign(direction.x), 0);
    else
        direction = new Vector2(0, Mathf.Sign(direction.y));

    Vector2 startPos = transform.position;
    Vector2 targetPos = startPos + direction * cellSize;

    // ✅ Detectar obstáculos sólidos
    if (Physics2D.OverlapCircle(targetPos, 0.25f, obstacleLayer))
        return false;

    // ✅ Detectar otras cajas bloqueando el camino
    Collider2D hitOtherBox = Physics2D.OverlapCircle(targetPos, 0.25f, boxLayer);
    if (hitOtherBox != null && hitOtherBox.gameObject != gameObject)
        return false;

    // Cancelar tween previo
    if (moveTween != null && moveTween.IsActive())
        moveTween.Kill();

    // Movimiento con DOTween
    moveTween = transform.DOMove(targetPos, moveDuration)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            transform.position = new Vector3(
                Mathf.Round(targetPos.x * 100f) / 100f,
                Mathf.Round(targetPos.y * 100f) / 100f,
                transform.position.z
            );
        });

    return true;
}

}
