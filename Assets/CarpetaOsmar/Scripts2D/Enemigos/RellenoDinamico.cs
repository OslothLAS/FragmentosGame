using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(MeshFilter), typeof(MeshRenderer))]
public class RellenoDinamico2D : MonoBehaviour
{
    [Header("Configuración")]
    public Transform[] circulos;
    public float grosor = 1f;
    public Material materialComun;

    [Header("Apariencia de Sprite")]
    [Tooltip("Elige el color exacto y la transparencia (Alfa) del relleno")]
    public Color colorSprite = Color.white;

    [Header("Orden de Renderizado (2D)")]
    public string sortingLayerName = "Default";
    public int sortingOrder = -1;

    private LineRenderer lineRenderer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh meshTriangulo;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Configuración de Línea (Bordes)
        lineRenderer.material = materialComun;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = grosor;
        lineRenderer.endWidth = grosor;
        lineRenderer.numCapVertices = 15;
        lineRenderer.numCornerVertices = 15;

        // Integración con capas 2D
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;

        // Configuración de Malla (Relleno de triángulo)
        meshRenderer.material = materialComun;
        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;

        meshTriangulo = new Mesh();
        meshFilter.mesh = meshTriangulo;
    }

    void Update()
    {
        if (circulos == null || circulos.Length < 2) return;

        // 0. Aplicar el color estilo Sprite
        lineRenderer.startColor = colorSprite;
        lineRenderer.endColor = colorSprite;

        // 1. Dibujar líneas
        lineRenderer.positionCount = circulos.Length;
        for (int i = 0; i < circulos.Length; i++)
        {
            lineRenderer.SetPosition(i, circulos[i].position);
        }

        lineRenderer.loop = (circulos.Length == 3);

        // 2. Rellenar malla
        if (circulos.Length == 3)
        {
            Vector3[] vertices = new Vector3[3];
            vertices[0] = transform.InverseTransformPoint(circulos[0].position);
            vertices[1] = transform.InverseTransformPoint(circulos[1].position);
            vertices[2] = transform.InverseTransformPoint(circulos[2].position);

            meshTriangulo.vertices = vertices;
            meshTriangulo.triangles = new int[] { 0, 1, 2, 0, 2, 1 };

            // Teñir los vértices de la malla para que el shader Sprites/Default los lea
            meshTriangulo.colors = new Color[] { colorSprite, colorSprite, colorSprite };

            meshTriangulo.RecalculateBounds();
        }
        else
        {
            meshTriangulo.Clear();
        }
    }
}