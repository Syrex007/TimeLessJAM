using UnityEngine;

[ExecuteInEditMode]
public class GridSnap : MonoBehaviour
{
    public float cellSize = 1f; // 1 unidad = 16 píxeles si tu Pixels Per Unit = 16

    void Update()
    {
        if (!Application.isPlaying)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Floor(pos.x / cellSize) * cellSize;
            pos.y = Mathf.Floor(pos.y / cellSize) * cellSize;
            transform.position = pos;
        }
    }
}
