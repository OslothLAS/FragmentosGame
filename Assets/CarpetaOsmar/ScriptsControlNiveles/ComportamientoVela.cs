using UnityEngine;

public class ComportamientoVela : MonoBehaviour
{
    [Header("Configuración de Flotación")]
    [Tooltip("Qué tanto se mueve la vela hacia los lados (Eje X)")]
    public float amplitud = 0.2f;

    [Tooltip("Qué tan rápido se mueve")]
    public float velocidad = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        // Guardamos la posición original de la vela para usarla como ancla del movimiento
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Calculamos la nueva posición X usando el tiempo y la función Seno
        float nuevaX = posicionInicial.z + Mathf.Sin(Time.time * velocidad) * amplitud;

        // Actualizamos la posición del objeto, manteniendo su Y y Z intactas
        transform.position = new Vector3(transform.position.x, transform.position.y, nuevaX);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto que tocó a la vela es el jugador
        if (other.CompareTag("Player"))
        {
            // Hace que la vela desaparezca
            Destroy(gameObject);
        }
    }
}