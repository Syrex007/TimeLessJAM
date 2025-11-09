using UnityEngine;
using DG.Tweening;

public class Hole : MonoBehaviour
{
    [Header("Configuración de caída")]
    public float fallDuration = 0.5f;             // duración de la animación
    public Vector3 targetScale = Vector3.zero;    // escala final para simular caída
    public float fadeTo = 0f;                     // transparencia final

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            FallAndDestroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            FallPlayer(other.gameObject);
        }
    }

    private void FallAndDestroy(GameObject obj)
    {
        // Desactivar el script BreakableBox para que no se pueda empujar
        BreakableBox boxScript = obj.GetComponent<BreakableBox>();
        obj.GetComponent<BoxCollider2D>().enabled = false;
        if (boxScript != null)
            boxScript.enabled = false;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            obj.transform.DOScale(targetScale, fallDuration).SetEase(Ease.InQuad);
            sr.DOFade(fadeTo, fallDuration).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(obj));
        }
        else
        {
            obj.transform.DOScale(targetScale, fallDuration).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(obj));
        }
    }

    private void FallPlayer(GameObject playerObj)
    {
        // Desactivar el movimiento del jugador
        PlayerGridMovement movement = playerObj.GetComponent<PlayerGridMovement>();
        if (movement != null)
            movement.enabled = false;

        SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            playerObj.transform.DOScale(targetScale, fallDuration).SetEase(Ease.InQuad);
            sr.DOFade(fadeTo, fallDuration).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // Aplicar daño al jugador
                    if (PlayerStatsController.Instance != null)
                        PlayerStatsController.Instance.ReceiveDamage(3);

                    // Restaurar escala y opacidad para que no desaparezca permanentemente
                    playerObj.transform.localScale = Vector3.one;
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
                });
        }
        else
        {
            playerObj.transform.DOScale(targetScale, fallDuration).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    if (PlayerStatsController.Instance != null)
                        PlayerStatsController.Instance.ReceiveDamage(3);

                    playerObj.transform.localScale = Vector3.one;
                });
        }
    }
}
