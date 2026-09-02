using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DetectorUV : MonoBehaviour
{
    [Header("Diagnóstico")]
    public bool mostrarDebugsDeDeteccion = false;

    [Header("Datos en Vivo del Personaje")]
    [Tooltip("Nos dice si en este frame exacto el personaje está pisando este pedazo")]
    public bool personajeEstaPisando = false;
    public Vector2 ultimaPosicionUV;

    private Vector2[] uvs;
    private int[] triangulos;

    void Start()
    {
        Mesh malla = GetComponent<MeshFilter>().mesh;
        uvs = malla.uv;
        triangulos = malla.triangles;
    }

    public bool ContieneAlPersonaje(Vector2 uvPersonaje)
    {
        if (uvs == null || uvs.Length == 0) return false;

        for (int i = 0; i < triangulos.Length; i += 3)
        {
            Vector2 p1 = uvs[triangulos[i]];
            Vector2 p2 = uvs[triangulos[i + 1]];
            Vector2 p3 = uvs[triangulos[i + 2]];

            if (PuntoEnTriangulo(uvPersonaje, p1, p2, p3))
            {
                personajeEstaPisando = true;
                ultimaPosicionUV = uvPersonaje;

                if (mostrarDebugsDeDeteccion)
                {
                    Debug.Log($"<color=green>[DetectorUV] Personaje ubicado en UV: X {ultimaPosicionUV.x:F3}, Y {ultimaPosicionUV.y:F3} del {gameObject.name}</color>");
                }
                return true;
            }
        }

        personajeEstaPisando = false;
        return false;
    }

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