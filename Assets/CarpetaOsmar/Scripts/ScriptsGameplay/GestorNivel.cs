using UnityEngine;
using TMPro; // Necesario para controlar textos modernos en Unity

public class GestorNivel : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
    [Tooltip("Arrastrá acá el objeto de texto desde tu Canvas")]
    public TextMeshProUGUI textoContador;

    private int contadorVela = 0;
    private const int maxVelas = 3;
    private bool puedeTerminarJuego = false;

    void Start()
    {
        // Inicializamos el texto apenas arranca el nivel para que no diga "New Text"
        ActualizarPantalla();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Lógica para recoger las velas
        if (other.CompareTag("Vela"))
        {
            if (contadorVela < maxVelas)
            {
                contadorVela++;

                // Actualizamos el número en la pantalla al instante
                ActualizarPantalla();

                Debug.Log($"Vela recogida. Llevas {contadorVela} de {maxVelas}.");
                Destroy(other.gameObject);

                if (contadorVela >= maxVelas)
                {
                    puedeTerminarJuego = true;
                    Debug.Log("¡Tienes todas las velas! La puerta está desbloqueada.");
                }
            }
        }

        // 2. Lógica para intentar salir por la puerta
        else if (other.CompareTag("Puerta"))
        {
            if (puedeTerminarJuego)
            {
                // Acá a futuro iría tu lógica real para cambiar de escena o ganar
                Debug.Log("Terminar el juego");
            }
            else
            {
                Debug.Log($"No podés salir todavía. Te faltan velas ({contadorVela}/{maxVelas}).");
            }
        }
    }

    // Centralizamos la actualización de la UI en un solo lugar
    private void ActualizarPantalla()
    {
        if (textoContador != null)
        {
            textoContador.text = $"{contadorVela} / {maxVelas}";
        }
        else
        {
            Debug.LogWarning("¡Te olvidaste de asignar el Texto en el Inspector del GestorNivel!");
        }
    }
}