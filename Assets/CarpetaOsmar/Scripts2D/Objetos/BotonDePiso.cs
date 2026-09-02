using UnityEngine;
using System.Collections.Generic;

public class BotonDePiso : MonoBehaviour
{
    // Definimos los canales disponibles
    public enum CanalBoton { Canal1, Canal2, Canal3, Canal4, Canal5 }

    // Memoria global compartida entre todos los scripts para leer el estado de los canales
    public static Dictionary<CanalBoton, bool> EstadoCanales = new Dictionary<CanalBoton, bool>()
    {
        { CanalBoton.Canal1, false },
        { CanalBoton.Canal2, false },
        { CanalBoton.Canal3, false },
        { CanalBoton.Canal4, false },
        { CanalBoton.Canal5, false }
    };

    [Header("Comunicación")]
    [Tooltip("¿A qué canal envía la señal este botón?")]
    public CanalBoton canalAsignado = CanalBoton.Canal1;

    [Header("Configuración de Peso")]
    public float pesoRequerido = 5f;

    [Header("Estado del Mecanismo")]
    public bool presionadoDelTodo = false;

    [Header("Debug")]
    [Tooltip("Muestra en el Inspector si el canal de este botón está enviando señal")]
    public bool canalActivado; // NUEVO BOOLEANO PARA VER EL ESTADO

    [Header("Movimiento del Sprite")]
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
            Debug.LogWarning("Falta asignar el Sprite en el script.");
            spriteDelBoton = transform;
        }
        objetivoY = yArriba;

        // FIX: Encajamos el sprite en la posición inicial exacta de forma instantánea 
        Vector3 posInicial = spriteDelBoton.localPosition;
        posInicial.y = yArriba;
        spriteDelBoton.localPosition = posInicial;

        EstadoCanales[canalAsignado] = false;
    }

    void Update()
    {
        Vector3 posLocal = spriteDelBoton.localPosition;
        posLocal.y = Mathf.MoveTowards(posLocal.y, objetivoY, velocidadHundimiento * Time.deltaTime);
        spriteDelBoton.localPosition = posLocal;

        presionadoDelTodo = Mathf.Abs(spriteDelBoton.localPosition.y - yAbajo) < 0.01f;

        // Actualizamos el cerebro global con el estado actual de este botón
        EstadoCanales[canalAsignado] = presionadoDelTodo;

        // Reflejamos el estado en el booleano visible del Inspector
        canalActivado = EstadoCanales[canalAsignado];
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
        cuerposEncima.RemoveAll(rb => rb == null);

        if (cuerposEncima.Count == 0)
        {
            objetivoY = yArriba;
            return;
        }

        float pesoActual = 0f;
        foreach (Rigidbody2D rb in cuerposEncima) pesoActual += rb.mass;

        if (pesoActual >= pesoRequerido) objetivoY = yAbajo;
        else objetivoY = yMitad;
    }
}