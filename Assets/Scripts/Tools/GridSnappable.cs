using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class GridSnappable : MonoBehaviour
{
    [HideInInspector] public GridSystem gridSystem;

    private void OnEnable()
    {
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();
    }

    // 🔹 Calcula el offset desde el centro hasta la esquina inferior izquierda
    public Vector3 GetBottomLeftOffset()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Bounds b = r.bounds;
            Vector3 bottomLeft = new Vector3(b.min.x, b.min.y, transform.position.z);
            return transform.position - bottomLeft;
        }

        SpriteRenderer s = GetComponent<SpriteRenderer>();
        if (s != null)
        {
            Bounds b = s.bounds;
            Vector3 bottomLeft = new Vector3(b.min.x, b.min.y, transform.position.z);
            return transform.position - bottomLeft;
        }

        // Si no tiene renderizador, no hay offset
        return Vector3.zero;
    }
}
