using UnityEngine;

public class ControladorCursorNativo : MonoBehaviour
{
    [Header("Imagen del Cursor")]
    public Texture2D cursorReposo;
    public Vector2 hotspot = Vector2.zero;

    // Cambiamos a Start para darle tiempo al sistema de Input a inicializarse
    void Start()
    {
        Cursor.SetCursor(cursorReposo, hotspot, CursorMode.Auto);
    }

}