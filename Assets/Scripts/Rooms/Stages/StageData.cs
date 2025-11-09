using UnityEngine;

public class StageData : MonoBehaviour
{
    public int stageIndex;

    private void Awake()
    {
        if (StageTransitionManager.Instance != null)
            StageTransitionManager.Instance.RegisterStage(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(14f, 9f, 0));
    }
}
