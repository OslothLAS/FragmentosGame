using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DetectorUV : MonoBehaviour
{
    private Vector2[] uvs;
    private int[] triangulos;

    void Start()
    {
        // Extraemos la malla única que creó tu script ProyectarUVFragmento
        Mesh malla = GetComponent<MeshFilter>().mesh;
        uvs = malla.uv;
        triangulos = malla.triangles;
    }

    // El personaje va a llamar a esta función pasándole su posición en la pantalla (0 a 1)
    public bool ContieneAlPersonaje(Vector2 uvPersonaje)
    {
        if (uvs == null || uvs.Length == 0) return false;

        // Revisamos cada triángulo de la malla de este fragmento
        for (int i = 0; i < triangulos.Length; i += 3)
        {
            Vector2 p1 = uvs[triangulos[i]];
            Vector2 p2 = uvs[triangulos[i + 1]];
            Vector2 p3 = uvs[triangulos[i + 2]];

            // Verificamos si el UV del personaje cae adentro de este triángulo
            if (PuntoEnTriangulo(uvPersonaje, p1, p2, p3))
            {
                return true;
            }
        }
        return false;
    }

    // Matemática estándar para saber si un punto 2D está dentro de un triángulo 2D
    private bool PuntoEnTriangulo(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1 = Signo(pt, v1, v2);
        float d2 = Signo(pt, v2, v3);
        float d3 = Signo(pt, v3, v1);

        bool tieneNegativo = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool tienePositivo = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(tieneNegativo && tienePositivo);
    }

    private float Signo(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}