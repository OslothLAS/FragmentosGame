using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GestorNivel : MonoBehaviour
{
    [Header("Interfaz de Usuario - HUD")]
    public TextMeshProUGUI textoContador;
    [Tooltip("Arrastrá acá el texto que mostrará el tiempo mientras juegas")]
    public TextMeshProUGUI textoTiempoHUD;

    [Header("Interfaz de Usuario - Victoria")]
    public GameObject pantallaVictoria;
    public TextMeshProUGUI textoTiempoFinal;

    [Tooltip("Arrastrá acá las 5 imágenes de las velas de la pantalla de victoria, en orden.")]
    public GameObject[] iconosVelasVictoria;

    private int contadorVela = 0;
    private const int velasRequeridas = 3;
    private const int totalVelas = 5;
    private bool puedeTerminarJuego = false;

    // Variables para el cronómetro
    private float tiempoTranscurrido = 0f;
    private bool cronometroActivo = true;

    void Start()
    {
        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);

        foreach (GameObject icono in iconosVelasVictoria)
        {
            if (icono != null) icono.SetActive(false);
        }

        ActualizarPantalla();
    }

    void Update()
    {
        if (cronometroActivo)
        {
            tiempoTranscurrido += Time.deltaTime;
            ActualizarTiempoHUD(); // Actualizamos el reloj visualmente cada frame
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Lógica para recoger las velas
        if (other.CompareTag("Vela"))
        {
            if (contadorVela < totalVelas)
            {
                contadorVela++;
                ActualizarPantalla();
                Destroy(other.gameObject);

                if (contadorVela == velasRequeridas)
                {
                    puedeTerminarJuego = true;
                    Debug.Log("¡Tienes las 3 velas necesarias! Puerta desbloqueada.");
                }
            }
        }

        // 2. Lógica para salir por la puerta y ganar
        else if (other.CompareTag("Puerta"))
        {
            if (puedeTerminarJuego)
            {
                cronometroActivo = false; // Frenamos el cronómetro interno y el del HUD
                MostrarPantallaVictoria();
            }
            else
            {
                Debug.Log($"No podés salir todavía. Te faltan velas ({contadorVela}/{velasRequeridas}).");
            }
        }
    }

    private void ActualizarPantalla()
    {
        if (textoContador != null)
        {
            textoContador.text = $"{contadorVela}/{velasRequeridas}";
        }
    }

    // --- NUEVO: Función para actualizar el reloj en el HUD ---
    private void ActualizarTiempoHUD()
    {
        if (textoTiempoHUD != null)
        {
            int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);

            // Lo mostramos con formato clásico de cronómetro (00:00)
            textoTiempoHUD.text = $"{minutos:00}:{segundos:00}";
        }
    }

    private void MostrarPantallaVictoria()
    {
        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(true);

            int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);

            if (textoTiempoFinal != null)
            {
                textoTiempoFinal.text = $"Tiempo: {minutos:00}:{segundos:00}";
            }

            // --- LÓGICA DE LOS ICONOS DE VELAS ---
            for (int i = 0; i < iconosVelasVictoria.Length; i++)
            {
                if (iconosVelasVictoria[i] != null)
                {
                    iconosVelasVictoria[i].SetActive(i < contadorVela);
                }
            }
        }
    }

    public void SiguienteNivel()
    {
        ControladorMenu.nivelSeleccionado++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}