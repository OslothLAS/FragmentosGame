using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EfectoParallaxCamara : MonoBehaviour
{
    [Header("Configuración de Rotación (Efecto 3D)")]
    [Tooltip("Cuánto inclina la cabeza hacia arriba/abajo")]
    public float rotacionMaxX = 2f;

    [Tooltip("Cuánto gira la cabeza hacia los lados")]
    public float rotacionMaxY = 2f;

    [Header("Configuración de Posición (Sobre la mesa)")]
    [Tooltip("Cuánto se mueve físicamente hacia arriba/abajo de la pantalla (Tu Eje X)")]
    public float movimientoMaxX = 0.2f;

    [Tooltip("Cuánto se mueve físicamente hacia la izquierda/derecha de la pantalla (Tu Eje Z)")]
    public float movimientoMaxZ = 0.2f;

    [Header("Efecto de Acercamiento (Clic)")]
    [Tooltip("Cuánto se acerca la cámara al hacer clic (Zoom)")]
    public float intensidadZoom = 5f;

    [Tooltip("Velocidad con la que hace el acercamiento")]
    public float suavizadoZoom = 8f;

    [Header("General")]
    [Tooltip("Velocidad a la que la cámara sigue al mouse (mayor = más rígido)")]
    public float suavizado = 5f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    // NUEVO: Variables globales para que la cámara "recuerde" dónde se quedó al hacer clic
    private Vector3 posicionObjetivo;
    private Quaternion rotacionObjetivo;

    private Camera miCamara;
    private float zoomInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;

        // Inicializamos los objetivos en el centro para el primer frame
        posicionObjetivo = posicionInicial;
        rotacionObjetivo = rotacionInicial;

        miCamara = GetComponent<Camera>();
        if (miCamara.orthographic)
            zoomInicial = miCamara.orthographicSize;
        else
            zoomInicial = miCamara.fieldOfView;
    }

    void Update()
    {
        // 1. Revisamos si el jugador está manteniendo presionado el clic izquierdo o derecho
        bool haciendoClic = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        // 2. NUEVO: SOLO actualizamos el objetivo si NO está haciendo clic
        if (!haciendoClic)
        {
            float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
            float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

            mouseX = Mathf.Clamp(mouseX, -1f, 1f);
            mouseY = Mathf.Clamp(mouseY, -1f, 1f);

            posicionObjetivo = posicionInicial + new Vector3(mouseY * movimientoMaxX, 0f, -mouseX * movimientoMaxZ);

            Quaternion rotacionExtra = Quaternion.Euler(-mouseY * rotacionMaxX, -mouseX * rotacionMaxY, 0f);
            rotacionObjetivo = rotacionInicial * rotacionExtra;
        }

        // 3. APLICAMOS EL PARALLAX
        // Esto se ejecuta SIEMPRE (incluso al hacer clic) para que el frenado de la cámara sea suave y no un golpe brusco.
        transform.localPosition = Vector3.Lerp(transform.localPosition, posicionObjetivo, suavizado * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, rotacionObjetivo, suavizado * Time.deltaTime);

        // 4. LÓGICA DE ZOOM AL HACER CLIC
        float zoomObjetivo = haciendoClic ? (zoomInicial - intensidadZoom) : zoomInicial;

        if (miCamara.orthographic)
        {
            miCamara.orthographicSize = Mathf.Lerp(miCamara.orthographicSize, zoomObjetivo, suavizadoZoom * Time.deltaTime);
        }
        else
        {
            miCamara.fieldOfView = Mathf.Lerp(miCamara.fieldOfView, zoomObjetivo, suavizadoZoom * Time.deltaTime);
        }
    }
}