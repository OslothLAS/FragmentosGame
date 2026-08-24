using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // NUEVO: Necesario para usar IEnumerator y Corrutinas

public class GestorNivel : MonoBehaviour
{
    public GameObject objetoTutorial;
    public GameObject objetoNivel2;
    public GameObject objetoNivel4;
    public GameObject cartelClickDerecho;

    public TextMeshProUGUI textoContador;
    public TextMeshProUGUI textoTiempoHUD;

    public GameObject[] iconosVidas;

    public GameObject pantallaVictoria;
    public TextMeshProUGUI textoTiempoFinal;
    public GameObject[] iconosVelasVictoria;

    private int contadorVela = 0;
    private const int velasRequeridas = 3;
    private const int totalVelas = 5;
    private bool puedeTerminarJuego = false;

    private int vidas = 3;
    private Vector3 posicionInicial;

    private float tiempoTranscurrido = 0f;
    private bool cronometroActivo = true;

    private float tiempoClickDerecho = 0f;
    private bool cartelEspecialMostrado = false;

    void Start()
    {
        posicionInicial = transform.position;

        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (cartelClickDerecho != null) cartelClickDerecho.SetActive(false);

        foreach (GameObject icono in iconosVelasVictoria)
        {
            if (icono != null) icono.SetActive(false);
        }

        int indiceNivel = ControladorMenu.nivelSeleccionado;

        if (objetoTutorial != null) objetoTutorial.SetActive(indiceNivel == 0);
        if (objetoNivel2 != null) objetoNivel2.SetActive(indiceNivel == 1);

        // --- NUEVO: Lógica del cartel temporal para el Nivel 4 ---
        // Asumiendo que el Tutorial es 0, Nivel 2 es 1, Nivel 3 es 2, y Nivel 4 es 3.
        if (objetoNivel4 != null)
        {
            if (indiceNivel == 4)
            {
                objetoNivel4.SetActive(true);
                StartCoroutine(DesactivarCartelNivel4()); // Iniciamos el contador de 10 segundos
            }
            else
            {
                objetoNivel4.SetActive(false); // Nos aseguramos de que esté apagado en otros niveles
            }
        }

        ActualizarPantalla();
        ActualizarVidasUI();
    }

    void Update()
    {
        if (cronometroActivo)
        {
            tiempoTranscurrido += Time.deltaTime;
            ActualizarTiempoHUD();
        }

        if (ControladorMenu.nivelSeleccionado == 1 && !cartelEspecialMostrado)
        {
            if (Input.GetMouseButton(1))
            {
                tiempoClickDerecho += Time.deltaTime;

                if (tiempoClickDerecho >= 0.5f)
                {
                    MostrarCartelEspecial();
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                tiempoClickDerecho = 0f;
            }
        }
    }

    // --- NUEVO: Corrutina que espera 10 segundos asincrónicamente ---
    private IEnumerator DesactivarCartelNivel4()
    {
        // Suspende la ejecución de esta función durante 10 segundos de tiempo de juego
        yield return new WaitForSeconds(10f);

        if (objetoNivel4 != null)
        {
            objetoNivel4.SetActive(false);
            Debug.Log("Pasaron los 10 segundos. Cartel del nivel 4 desactivado.");
        }
    }

    private void MostrarCartelEspecial()
    {
        cartelEspecialMostrado = true;

        if (cartelClickDerecho != null) cartelClickDerecho.SetActive(true);
        if (objetoTutorial != null) objetoTutorial.SetActive(false);
        if (objetoNivel2 != null) objetoNivel2.SetActive(false);

        Debug.Log("Se mantuvo el click derecho 0.5s: Cartel especial activado.");
    }

    private void OnTriggerEnter(Collider other)
    {
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
        else if (other.CompareTag("Puerta"))
        {
            if (puedeTerminarJuego)
            {
                cronometroActivo = false;
                MostrarPantallaVictoria();
            }
            else
            {
                Debug.Log($"No podés salir todavía. Te faltan velas ({contadorVela}/{velasRequeridas}).");
            }
        }
        else if (other.CompareTag("Limite"))
        {
            PerderVida();
        }
    }

    private void PerderVida()
    {
        vidas--;
        ActualizarVidasUI();

        if (vidas > 0)
        {
            transform.position = posicionInicial;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            Debug.Log($"Perdiste una vida. Te quedan: {vidas}");
        }
        else
        {
            Debug.Log("¡Te quedaste sin vidas! Game Over.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void ActualizarPantalla()
    {
        if (textoContador != null)
        {
            textoContador.text = $"{contadorVela}/{velasRequeridas}";
        }
    }

    private void ActualizarVidasUI()
    {
        for (int i = 0; i < iconosVidas.Length; i++)
        {
            if (iconosVidas[i] != null)
            {
                iconosVidas[i].SetActive(i < vidas);
            }
        }
    }

    private void ActualizarTiempoHUD()
    {
        if (textoTiempoHUD != null)
        {
            int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);
            textoTiempoHUD.text = $"{minutos:00}:{segundos:00}";
        }
    }

    private void MostrarPantallaVictoria()
    {
        if (pantallaVictoria != null)
        {
            if (cartelClickDerecho != null) cartelClickDerecho.SetActive(false);
            if (objetoNivel2 != null) objetoNivel2.SetActive(false);
            if (objetoTutorial != null) objetoTutorial.SetActive(false);
            if (objetoNivel4 != null) objetoNivel4.SetActive(false); // Nos aseguramos de apagarlo también al ganar
            pantallaVictoria.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60F);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60F);

            if (textoTiempoFinal != null)
            {
                textoTiempoFinal.text = $"Tiempo: {minutos:00}:{segundos:00}";
            }

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