using UnityEngine;

public class ControladorCursorNativo : MonoBehaviour
{
    [Header("Imagen del Cursor")]
    public Texture2D cursorReposo;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        // 1. Forzamos a que el cursor sea visible
        Cursor.visible = true;
        // 3. Le asignamos tu dibujo
        Cursor.SetCursor(cursorReposo, hotspot, CursorMode.Auto);
    }
}