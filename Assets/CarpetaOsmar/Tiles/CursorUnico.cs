using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CursorUnico : MonoBehaviour
{
    [Header("Sprite del Cursor")]
    [Tooltip("La imagen de la mano que se mostrará en todo momento")]
    public Sprite spriteMano;

    private Image miImagen;

    void Start()
    {
        miImagen = GetComponent<Image>();

        // Le asignamos la imagen desde el principio
        if (spriteMano != null)
        {
            miImagen.sprite = spriteMano;
        }

        // Ocultamos el cursor del sistema al arrancar
        Cursor.visible = false;
    }

    void Update()
    {
        // Forzamos la invisibilidad por si el sistema operativo intenta mostrarlo
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        // Revisamos si el jugador está manteniendo apretado click izquierdo o derecho
        bool holdeandoClic = Input.GetMouseButton(0) || Input.GetMouseButton(1);

        // Solo actualizamos la posición si NO está haciendo clic.
        // Si hace clic, esta línea se ignora y la mano se queda congelada en el lugar.
        if (!holdeandoClic)
        {
            transform.position = Input.mousePosition;
        }
    }
}