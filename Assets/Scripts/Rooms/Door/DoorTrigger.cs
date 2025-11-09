using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    [Header("Configuración de Puerta")]
    public bool doorOpen = true;

    [Tooltip("Índice del stage al que lleva esta puerta.")]
    public int targetStageIndex;

    [Tooltip("Posición exacta donde el jugador aparecerá en el stage destino.")]
    public Vector2 targetSpawnPosition;

    [Tooltip("Dirección general de la puerta (informativo).")]
    public string direction = "Right"; // "Left", "Right", "Up", "Down"

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && doorOpen)
        {
            StageTransitionManager.Instance.MoveToStage(targetStageIndex, targetSpawnPosition);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Dibujar collider de la puerta
        Gizmos.color = doorOpen ? Color.cyan : Color.red;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawWireCube(transform.position, col.bounds.size);

        // Dibujar destino
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(targetSpawnPosition, 0.15f);

        // Línea de conexión entre puerta y destino
        Gizmos.color = new Color(1f, 0f, 1f, 0.6f);
        Gizmos.DrawLine(transform.position, targetSpawnPosition);
    }
#endif
}

#if UNITY_EDITOR
// 🔧 Este editor personalizado agrega un handle en la escena
[CustomEditor(typeof(DoorTrigger))]
public class DoorTriggerEditor : Editor
{
    private void OnSceneGUI()
    {
        DoorTrigger door = (DoorTrigger)target;

        // Mostrar un handle para mover el punto de spawn en la escena
        EditorGUI.BeginChangeCheck();
        Vector3 newPos = Handles.PositionHandle(door.targetSpawnPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(door, "Mover Target Spawn Position");
            door.targetSpawnPosition = newPos;
            EditorUtility.SetDirty(door);
        }

        // Etiqueta de texto encima
        Handles.color = Color.magenta;
        Handles.Label(door.targetSpawnPosition + Vector2.up * 0.2f, $"Spawn → Stage {door.targetStageIndex}", EditorStyles.boldLabel);
    }
}
#endif
