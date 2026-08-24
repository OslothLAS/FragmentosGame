using UnityEngine;

public class OscilacionX : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    [Tooltip("Qué tan rápido se moverá el objeto.")]
    public float velocidad = 2f;

    [Tooltip("Distancia máxima a la que llegará desde su punto de origen.")]
    public float amplitud = 3f;

    private Vector3 posicionInicial;

    void Start()
    {
        // Guardamos la posición exacta donde pusiste el objeto en el editor
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Mathf.Sin crea una onda que sube y baja suavemente entre -1 y 1
        // Lo multiplicamos por el tiempo y la velocidad, y luego por la distancia deseada
        float desplazamiento = Mathf.Sin(Time.time * velocidad) * amplitud;

        // Aplicamos el movimiento únicamente en el eje X, manteniendo Y y Z intactos
        transform.position = new Vector3(
            posicionInicial.x ,
            posicionInicial.y,
            posicionInicial.z + desplazamiento
        );
    }
}