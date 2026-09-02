using UnityEngine;
using System.Collections.Generic;

public class BotonDePiso : MonoBehaviour
{
    [Header("Configuración de Peso")]
    [Tooltip("La masa mínima total (Rigidbody2D.mass) para que el botón baje al fondo.")]
    public float pesoRequerido = 5f;

    [Header("Estado del Mecanismo (Lectura Pública)")]
    [Tooltip("True únicamente cuando el botón bajó completamente. Úsalo desde otros scripts.")]
    public bool presionadoDelTodo = false;

    [Header("Movimiento del Sprite")]
    [Tooltip("Arrastrá acá el GameObject HIJO que tiene el dibujo (SpriteRenderer) del botón.")]
    public Transform spriteDelBoton;
    public float yArriba = 0f;
    public float yMitad = -0.15f;
    public float yAbajo = -0.3f;
    public float velocidadHundimiento = 5f;

    private float objetivoY;
    private List<Rigidbody2D> cuerposEncima = new List<Rigidbody2D>();

    void Start()
    {
        if (spriteDelBoton == null)
        {
            Debug.LogWarning("Falta asignar el Sprite en el script. Si se mueve todo el objeto, la física puede fallar.");
            spriteDelBoton = transform;
        }
        objetivoY = yArriba;
    }

    void Update()
    {
        // 1. Movimiento suave del SPRITE hacia abajo o arriba
        Vector3 posLocal = spriteDelBoton.localPosition;
        posLocal.y = Mathf.MoveTowards(posLocal.y, objetivoY, velocidadHundimiento * Time.deltaTime);
        spriteDelBoton.localPosition = posLocal;

        // 2. Activamos el booleano si el sprite visualmente llegó al fondo
        presionadoDelTodo = Mathf.Abs(spriteDelBoton.localPosition.y - yAbajo) < 0.01f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && !cuerposEncima.Contains(rb))
        {
            cuerposEncima.Add(rb);
            RecalcularPeso();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && cuerposEncima.Contains(rb))
        {
            cuerposEncima.Remove(rb);
            RecalcularPeso();
        }
    }

    private void RecalcularPeso()
    {
        // Limpiamos la lista por si destruiste una caja mientras estaba encima
        cuerposEncima.RemoveAll(rb => rb == null);

        if (cuerposEncima.Count == 0)
        {
            objetivoY = yArriba;
            return;
        }

        float pesoActual = 0f;
        foreach (Rigidbody2D rb in cuerposEncima)
        {
            pesoActual += rb.mass;
        }

        // Definimos hasta dónde tiene que bajar el sprite
        if (pesoActual >= pesoRequerido)
        {
            objetivoY = yAbajo;
        }
        else
        {
            objetivoY = yMitad;
        }
    }
}