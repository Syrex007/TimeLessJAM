using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(LineRenderer))]
public class LaserShooter : MonoBehaviour
{
    [Header("Configuración del láser")]
    public float laserLength = 10f;
    public float laserWidth = 0.3f;
    public Gradient laserGradient;
    public LayerMask enemyLayer;
    public LayerMask obstacleLayer;
    public float fadeDuration = 0.5f;

    [Header("Pixel Shake")]
    public float playerShakeStrength = 0.25f; // tamaño del salto en unidades
    public float cameraShakeStrength = 0.3f;
    public int shakeSteps = 4; // cuántos movimientos bruscos
    public float shakeDuration = 0.1f; // duración total del shake

    private bool hasFired = false;
    private LineRenderer lineRenderer;
    private Vector3 originalPlayerPos;
    private Vector3 originalCameraPos;
    private Camera mainCamera;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.colorGradient = laserGradient;
        lineRenderer.enabled = false;

        // Pixel art style
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.numCapVertices = 0;
        lineRenderer.numCornerVertices = 0;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!hasFired && Input.GetKeyDown(KeyCode.E))
        {
            FireLaser();
        }
    }

    private void FireLaser()
    {
        hasFired = true;
        originalPlayerPos = transform.position;
        originalCameraPos = mainCamera.transform.position;

        Vector2 direction = GetDirectionFromAnimation();
        Vector3 startPos = transform.position;

        // Determinar distancia máxima considerando paredes
        RaycastHit2D hitWall = Physics2D.Raycast(startPos, direction, laserLength, obstacleLayer);
        float actualLength = laserLength;
        if (hitWall.collider != null)
        {
            actualLength = hitWall.distance;
        }

        Vector3 endPos = startPos + (Vector3)(direction.normalized * actualLength);

        // Shake tipo pixel art del jugador
        PixelShake(transform, originalPlayerPos, playerShakeStrength, shakeSteps, shakeDuration);

        // Shake tipo pixel art de la cámara
        PixelShake(mainCamera.transform, originalCameraPos, cameraShakeStrength, shakeSteps, shakeDuration);

        // Aparece el rayo de golpe
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.enabled = true;

        // Detectar enemigos
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, actualLength, enemyLayer);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
            }
        }

        // Fade del láser
        DOTween.To(() => 1f, x => SetLaserAlpha(x), 0f, fadeDuration)
            .OnComplete(() => lineRenderer.enabled = false);
    }

    private Vector2 GetDirectionFromAnimation()
    {
        if (PlayerGridMovement.Instance.animator == null)
            return Vector2.up;

        AnimatorStateInfo state = PlayerGridMovement.Instance.animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("UpPlayer")) return Vector2.up;
        if (state.IsName("DownPlayer")) return Vector2.down;
        if (state.IsName("LeftPlayer")) return Vector2.left;
        if (state.IsName("RightPlayer")) return Vector2.right;

        return Vector2.up;
    }

    private void SetLaserAlpha(float alpha)
    {
        Gradient g = new Gradient();
        GradientColorKey[] colorKeys = laserGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorKeys.Length];

        for (int i = 0; i < colorKeys.Length; i++)
        {
            alphaKeys[i] = new GradientAlphaKey(alpha, colorKeys[i].time);
        }

        g.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = g;
    }

    private void PixelShake(Transform target, Vector3 originalPos, float strength, int steps, float duration)
    {
        float stepTime = duration / steps;
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < steps; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f
            );
            seq.Append(DOTween.To(() => 0f, x => target.position = originalPos + offset, 1f, stepTime)
                        .SetEase(Ease.OutSine));
        }
        seq.OnComplete(() => target.position = originalPos);
    }
}
