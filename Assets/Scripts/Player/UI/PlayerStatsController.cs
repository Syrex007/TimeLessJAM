using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerStatsController : MonoBehaviour
{
    public static PlayerStatsController Instance { get; private set; }

    [Header("Estadísticas del Jugador")]
    public int maxHealth = 3;
    public int currentHealth;
    public int attackDamage = 1;

    [Header("HUD de Vida")]
    public List<Image> lifeImages = new List<Image>(); // Asigna las imágenes desde el Canvas
    public float heartVanishDuration = 0.4f;
    public Ease heartVanishEase = Ease.InBack;

    [Header("Pantalla Negra (Fade)")]
    public Image blackScreen; // Asigna una imagen negra en todo el canvas
    public float fadeDuration = 0.8f;

    private bool isDead = false;
    private List<Vector3> originalScales = new List<Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentHealth = maxHealth;

        // Guardar la escala original de cada corazón (respetando tu diseño)
        foreach (var img in lifeImages)
        {
            if (img != null)
                originalScales.Add(img.transform.localScale);
            else
                originalScales.Add(Vector3.one);
        }
    }

    private void Start()
    {
        // Fade-in suave de pantalla negra
        if (blackScreen != null)
        {
            blackScreen.color = Color.black;
            blackScreen.DOFade(0, fadeDuration);
        }
    }

    public void ReceiveDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateLifeHUD();

        Debug.Log($"Jugador recibió {damage} de daño. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateLifeHUD()
    {
        for (int i = 0; i < lifeImages.Count; i++)
        {
            var img = lifeImages[i];
            if (img == null) continue;

            img.transform.DOKill();

            if (i < currentHealth)
            {
                // Restaurar a su escala original si estaba reducido antes
                img.transform.localScale = originalScales[i];
                img.color = Color.white;
            }
            else
            {
                // Animación solo al perder vida
                img.transform
                    .DOScale(0, heartVanishDuration)
                    .SetEase(heartVanishEase);
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Jugador ha muerto. Reiniciando escena...");

        if (blackScreen != null)
        {
            blackScreen.DOFade(1, fadeDuration)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    });
                });
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}
