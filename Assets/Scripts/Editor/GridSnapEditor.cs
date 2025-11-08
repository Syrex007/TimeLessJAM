using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSnappable))]
[CanEditMultipleObjects]
public class GridSnapEditor : Editor
{
    private void OnSceneGUI()
    {
        GridSnappable snappable = (GridSnappable)target;
        if (snappable.gridSystem == null)
            snappable.gridSystem = FindObjectOfType<GridSystem>();

        if (snappable.gridSystem == null) return;

        GridSystem grid = snappable.gridSystem;
        Transform objTransform = snappable.transform;

        if (Tools.current == Tool.Move)
        {
            Undo.RecordObject(objTransform, "Grid Snap");

            //  Posición de celda
            Vector3 snappedCell = grid.GetSnappedPosition(objTransform.position);

            // Offset de la esquina inferior izquierda del objeto
            Vector3 offset = snappable.GetBottomLeftOffset();

            // Nueva posición: esquina inferior izquierda del objeto = esquina inferior izquierda de celda
            objTransform.position = snappedCell + offset;
        }
    }
}
