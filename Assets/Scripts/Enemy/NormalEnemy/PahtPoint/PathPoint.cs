using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public enum EnemyState { NoneEnemy, EnemyHere, EnemyNext }
    public EnemyState state = EnemyState.NoneEnemy;

    public EnemyController ownerEnemy; // referencia al enemigo que lo usa

    private void OnDrawGizmos()
    {
        switch (state)
        {
            case EnemyState.EnemyHere:
                Gizmos.color = Color.red;
                break;
            case EnemyState.EnemyNext:
                Gizmos.color = Color.yellow;
                break;
            default:
                Gizmos.color = Color.gray;
                break;
        }

        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}
