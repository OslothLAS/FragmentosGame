using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ProyectarUVFragmento : MonoBehaviour
{
    [Header("Referencia de la Pantalla Original")]
    [Tooltip("El objeto que representa la pantalla entera (Ej: un Quad o Plane escalado a 16:9)")]
    public Transform pantallaReferencia;

    [Header("Configuración de Proyección")]
    [Tooltip("Marcá esto si tus piezas están acostadas sobre el tablero (Ejes X y Z). Desmarcalo si están paradas (Ejes X e Y).")]
    public bool proyectarEnXZ = true;

    void Start()
    {
        BakeUVs();
    }

    [ContextMenu("Proyectar UVs Ahora (Para probar en el Editor)")]
    public void BakeUVs()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshFilter filtroReferencia = pantallaReferencia.GetComponent<MeshFilter>();

        if (meshFilter == null || pantallaReferencia == null || filtroReferencia == null)
        {
            Debug.LogWarning("Faltan referencias para calcular los UVs.");
            return;
        }

        // 1. Accedemos a los límites locales de la pantalla original
        Bounds limitesPantalla = filtroReferencia.sharedMesh.bounds;

        // 2. Instanciamos la malla del fragmento para no afectar al modelo original del proyecto
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        // 3. Recorremos cada vértice del fragmento deforme
        for (int i = 0; i < vertices.Length; i++)
        {
            // Convertimos la posición del vértice del fragmento a su posición real en el mundo 3D
            Vector3 posicionMundo = transform.TransformPoint(vertices[i]);

            // Convertimos esa posición del mundo a la posición relativa "adentro" de la pantalla
            Vector3 posLocalEnPantalla = pantallaReferencia.InverseTransformPoint(posicionMundo);

            float u, v;

            // 4. Calculamos las coordenadas UV (de 0 a 1) mapeando la posición
            if (proyectarEnXZ)
            {
                // Mapeo horizontal (X) y profundidad (Z)
                u = Mathf.InverseLerp(limitesPantalla.min.x, limitesPantalla.max.x, posLocalEnPantalla.x);
                v = Mathf.InverseLerp(limitesPantalla.min.z, limitesPantalla.max.z, posLocalEnPantalla.z);
            }
            else
            {
                // Mapeo horizontal (X) y vertical (Y)
                u = Mathf.InverseLerp(limitesPantalla.min.x, limitesPantalla.max.x, posLocalEnPantalla.x);
                v = Mathf.InverseLerp(limitesPantalla.min.y, limitesPantalla.max.y, posLocalEnPantalla.y);
            }

            uvs[i] = new Vector2(u, v);
        }

        // 5. Aplicamos los nuevos UVs a la malla del fragmento
        mesh.uv = uvs;
    }
}