using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class StageTransitionManager : MonoBehaviour
{
    public static StageTransitionManager Instance { get; private set; }

    [Header("Referencias")]
    public Camera mainCamera;
    public Transform player;
    public Image blackScreen;

    [Header("Configuración")]
    public float cameraMoveDuration = 1f;
    public Ease cameraMoveEase = Ease.InOutCubic;
    public float fadeDuration = 0.4f;

    [Header("Stages Registrados")]
    public List<StageData> stages = new List<StageData>();
    private int currentStageIndex = 0;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (blackScreen != null)
            blackScreen.color = new Color(0, 0, 0, 0);
    }

    public void MoveToStage(int targetStageIndex, Vector2 targetSpawnPosition)
    {
        if (isTransitioning) return;
        if (targetStageIndex < 0 || targetStageIndex >= stages.Count)
        {
            Debug.LogWarning($"Stage {targetStageIndex} no existe en la lista de StageTransitionManager.");
            return;
        }

        isTransitioning = true;

        if (PlayerGridMovement.Instance != null)
            PlayerGridMovement.Instance.enabled = false;

        StageData targetStage = stages[targetStageIndex];

        // La cámara se centra en el transform del stage destino
        Vector3 cameraTargetPos = new Vector3(
            targetStage.transform.position.x,
            targetStage.transform.position.y,
            mainCamera.transform.position.z
        );

        Sequence seq = DOTween.Sequence();

        // Fade a negro
        seq.Append(blackScreen.DOFade(1, fadeDuration));

        // Mover cámara al centro del nuevo stage
        seq.Append(mainCamera.transform.DOMove(cameraTargetPos, cameraMoveDuration).SetEase(cameraMoveEase));

        // Teletransportar jugador al punto de spawn definido por la puerta
        seq.AppendCallback(() =>
        {
            player.position = targetSpawnPosition;
            currentStageIndex = targetStageIndex;
        });

        // Fade out
        seq.Append(blackScreen.DOFade(0, fadeDuration));

        // Reactivar control del jugador
        seq.OnComplete(() =>
        {
            if (PlayerGridMovement.Instance != null)
                PlayerGridMovement.Instance.enabled = true;

            isTransitioning = false;
        });
    }

    public void RegisterStage(StageData stage)
    {
        if (!stages.Contains(stage))
            stages.Add(stage);
    }
}
