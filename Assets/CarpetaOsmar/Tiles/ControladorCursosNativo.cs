using UnityEngine;

public class ControladorCursorNativo : MonoBehaviour
{
    [Header("Imagen del Cursor")]
    public Texture2D cursorReposo;
    public Vector2 hotspot = Vector2.zero;

    void Awake()
    {
        Cursor.visible = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        Cursor.SetCursor(cursorReposo, hotspot, CursorMode.ForceSoftware);
#else
        Cursor.SetCursor(cursorReposo, hotspot, CursorMode.Auto);
#endif
    }
}