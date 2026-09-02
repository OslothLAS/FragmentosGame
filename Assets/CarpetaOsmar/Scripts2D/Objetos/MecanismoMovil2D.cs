using UnityEngine;

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

    private Vector3 posicionOriginal;
    private Vector3 posicionDestino;

    void Start()
    {
        // Guardamos dónde arranca el objeto
        posicionOriginal = transform.position;

        // Calculamos matemáticamente su punto de llegada
        float direccion = moverHaciaAbajo ? -1f : 1f;
        posicionDestino = posicionOriginal + new Vector3(0f, distancia * direccion, 0f);
    }

    void Update()
    {
        if (botonActivador == null) return;

        // Si el botón está pisado a fondo, el objetivo es la posición final. Si no, es la original.
        Vector3 objetivoActual = botonActivador.presionadoDelTodo ? posicionDestino : posicionOriginal;

        // Movemos el objeto paso a paso hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, objetivoActual, velocidad * Time.deltaTime);
    }
}