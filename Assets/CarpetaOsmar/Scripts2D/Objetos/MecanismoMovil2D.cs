using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MecanismoMovil2D : MonoBehaviour
{
    [Header("Conexión con el Botón")]
    [Tooltip("Arrastrá acá el objeto que tiene el script BotonDePiso2D")]
    public BotonDePiso botonActivador;

    [Header("Dirección y Distancia")]
    [Tooltip("Tildalo para que se mueva hacia abajo. Destildalo para que suba.")]
    public bool moverHaciaAbajo = false;

    [Tooltip("Cuántas unidades se va a desplazar desde su posición inicial")]
    public float distancia = 3f;

    [Tooltip("Qué tan rápido se mueve el objeto")]
    public float velocidad = 5f;

    // --- NUEVO: Propiedad que lee el jugador para heredar la velocidad ---
    public Vector2 VelocidadActual { get; private set; }

    private Vector2 posicionOriginal;
    private Vector2 posicionDestino;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Nos aseguramos de que sea Kinematic para que la gravedad no lo haga caer
        rb.bodyType = RigidbodyType2D.Kinematic;

        posicionOriginal = transform.position;

        float direccion = moverHaciaAbajo ? -1f : 1f;
        posicionDestino = posicionOriginal + new Vector2(0f, distancia * direccion);
    }

    // Cambiamos Update por FixedUpdate para trabajar sincronizados con las físicas
    void FixedUpdate()
    {
        if (botonActivador == null)
        {
            VelocidadActual = Vector2.zero;
            return;
        }

        Vector2 objetivoActual = botonActivador.presionadoDelTodo ? posicionDestino : posicionOriginal;

        // Calculamos cuál va a ser la nueva posición este frame
        Vector2 nuevaPosicion = Vector2.MoveTowards(rb.position, objetivoActual, velocidad * Time.fixedDeltaTime);

        // Calculamos la velocidad a la que nos estamos moviendo (Nueva Posición - Posición Actual)
        VelocidadActual = (nuevaPosicion - rb.position) / Time.fixedDeltaTime;

        // Movemos el objeto físicamente
        rb.MovePosition(nuevaPosicion);
    }
}