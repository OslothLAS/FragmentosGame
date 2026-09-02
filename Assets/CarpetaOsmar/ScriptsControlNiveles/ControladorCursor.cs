using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices; // Necesario para interactuar con el Sistema Operativo

[RequireComponent(typeof(Image))]
public class ControladorCursor : MonoBehaviour
{
    [Header("Sprites recortados")]
    public Sprite spriteReposo;
    public Sprite spriteClic;

    [Header("Configuración del Puntero")]
    public Vector2 puntoDePrecision = new Vector2(0f, 1f);

    private Image miImagen;
    private RectTransform miRectTransform;

    // Posición interna del Canvas
    private Vector2 posicionBloqueadaUI;

    // --- INTERFAZ CON LA API DE WINDOWS ---
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private POINT posicionHardware;

    void Start()
    {
        miImagen = GetComponent<Image>();
        miRectTransform = GetComponent<RectTransform>();
        miRectTransform.pivot = puntoDePrecision;

        Cursor.visible = false;
    }

    void Update()
    {
        // Forzamos la invisibilidad por si Windows intenta mostrar su puntero
        if (Cursor.visible) Cursor.visible = false;

        bool holdeandoDerecho = Input.GetMouseButton(1);
        bool holdeandoIzquierdo = Input.GetMouseButton(0);

        // --- LÓGICA DE BLOQUEO DE SISTEMA ---
        if (Input.GetMouseButtonDown(1))
        {
            // 1. Guardamos la posición visual para el Canvas
            posicionBloqueadaUI = transform.position;

            // 2. Leemos la posición en memoria del hardware (coordenadas absolutas del monitor)
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            GetCursorPos(out posicionHardware);
#endif

            // 3. Le decimos a Unity que atrape el hardware. Ahora tu mouse físico no choca con los bordes.
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            // 1. Liberamos el mouse
            Cursor.lockState = CursorLockMode.None;

            // 2. Al hacer esto, Unity centró el mouse. Inmediatamente usamos nuestra llamada a bajo nivel 
            // para devolver el cursor de Windows a la coordenada original que guardamos.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            SetCursorPos(posicionHardware.X, posicionHardware.Y);
#endif
        }

        // --- LÓGICA VISUAL EN PANTALLA ---
        if (holdeandoDerecho)
        {
            // ESTADO: CLICK DERECHO (LOCKED)
            miImagen.sprite = spriteClic;
            transform.position = posicionBloqueadaUI; // Dejamos el dibujo clavado
        }
        else if (holdeandoIzquierdo)
        {
            // ESTADO: CLICK IZQUIERDO (LIBRE)
            miImagen.sprite = spriteClic;
            transform.position = Input.mousePosition;
        }
        else
        {
            // ESTADO: REPOSO
            miImagen.sprite = spriteReposo;
            transform.position = Input.mousePosition;
        }
    }
}