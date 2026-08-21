using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FragmentoUV : MonoBehaviour
{
    private Vector2[] uvs;
    private int[] triangulos;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            // Guardamos en memoria los UVs y triángulos para consultarlos rápido sin gastar recursos
            uvs = mf.mesh.uv;
            triangulos = mf.mesh.triangles;
        }
    }

    // Verifica si la posición del sprite cae adentro de la forma de este fragmento
    public bool ContieneUV(Vector2 puntoUV)
    {
        if (uvs == null || triangulos == null) return false;

        for (int i = 0; i < triangulos.Length; i += 3)
        {
            Vector2 a = uvs[triangulos[i]];
            Vector2 b = uvs[triangulos[i + 1]];
            Vector2 c = uvs[triangulos[i + 2]];

            if (PuntoEnTriangulo(puntoUV, a, b, c))
            {
                return true;
            }
        }
        return false;
    }

    // Devuelve la dirección de gravedad en el plano ZX según la rotación de la pieza
    public Vector3 ObtenerDireccionAbajoZX()
    {
        // Vector local hacia abajo (-Z) transformado por la rotación del fragmento
        Vector3 abajoMundo = transform.rotation * Vector3.back;

        // Proyectamos exclusivamente sobre el plano ZX (descartamos altura Y)
        Vector3 abajoZX = new Vector3(abajoMundo.x, 0f, abajoMundo.z);

        return abajoZX.normalized;
    }

    private bool PuntoEnTriangulo(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);

        bool tieneNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool tienePos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(tieneNeg && tienePos);
    }

    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}