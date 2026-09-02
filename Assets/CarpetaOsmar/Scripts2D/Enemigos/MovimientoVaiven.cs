using UnityEngine;

public class MovimientoVaiven : MonoBehaviour
{
    [Header("Dirección del Movimiento")]
    [Tooltip("Activalo para que se mueva de lado a lado")]
    public bool moverHorizontal = true;

    [Tooltip("Activalo para que se mueva de arriba hacia abajo")]
    public bool moverVertical = false;
    // Si ambos están activos, se moverá en diagonal a 45 grados.

    [Header("Ajustes de Velocidad y Distancia")]
    public float velocidad = 3f;
    [Tooltip("Qué tan lejos llega antes de pegar la vuelta")]
    public float distancia = 2f;

    private Vector3 posicionInicial;
    private Vector3 vectorDireccion;

    void Start()
    {
        // Guardamos el punto de origen
        posicionInicial = transform.position;

        // Armamos la dirección base según lo que tildaste en el Inspector
        vectorDireccion = Vector3.zero;

        if (moverHorizontal) vectorDireccion.x = 1f;
        if (moverVertical) vectorDireccion.y = 1f;

        // Normalizamos el vector. Esto asegura que si va en diagonal (1, 1), 
        // la velocidad total siga siendo 1 y no se acelere.
        vectorDireccion.Normalize();
    }

    void Update()
    {
        // Mathf.Sin genera una onda que va de -1 a 1 fluidamente con el tiempo
        float oscilacion = Mathf.Sin(Time.time * velocidad) * distancia;

        // Actualizamos la posición sumando la oscilación en la dirección elegida
        transform.position = posicionInicial + (vectorDireccion * oscilacion);
    }
}