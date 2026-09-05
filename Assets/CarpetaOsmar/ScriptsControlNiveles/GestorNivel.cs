using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorNivel : MonoBehaviour
{
    public GameObject objetoTutorial;
    public GameObject objetoNivel2;
    public GameObject objetoNivel4;
    public GameObject cartelClickDerecho;

    [Header("Interfaz Puerta")]
    public GameObject cartelEntrarPuerta;

    [Header("Animación Inicial")]
    [Tooltip("El tiempo en segundos que dura la animación de la cámara")]
    public float tiempoAnimacionCamara = 5f;

    [Header("Sistema de Daño")]
    [Tooltip("Arrastrá acá el SpriteRenderer de tu personaje para que pueda parpadear")]
    public SpriteRenderer spritePersonaje;
    public float tiempoInvulnerabilidad = 1.5f;
    public float velocidadParpadeo = 0.15f;
    private bool esInvulnerable = false;

    // --- NUEVO: PANTALLA DE DERROTA ---
    [Header("Sistema de Derrota")]
    public GameObject pantallaDerrota;

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

    private bool enZonaPuerta = false;

    private int vidas = 3;
    private Vector3 posicionInicial;

    private float tiempoTranscurrido = 0f;

    // Lo iniciamos en false para que el tiempo no corra durante la animación
    private bool cronometroActivo = false;

    private float tiempoClickDerecho = 0f;
    private bool cartelEspecialMostrado = false;

    void Start()
    {
        // Restauramos el tiempo por si venimos de un reinicio post-derrota
        Time.timeScale = 1f;

        posicionInicial = transform.position;

        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (pantallaDerrota != null) pantallaDerrota.SetActive(false); // Apagamos derrota al inicio
        if (cartelClickDerecho != null) cartelClickDerecho.SetActive(false);
        if (cartelEntrarPuerta != null) cartelEntrarPuerta.SetActive(false);

        foreach (GameObject icono in iconosVelasVictoria)
        {
            if (icono != null) icono.SetActive(false);
        }

        // Verificamos si venimos del botón que activa la animación
        if (ControladorMenu.debeAnimarCamaraAlIniciar)
        {
            StartCoroutine(IniciarNivelPostAnimacion(tiempoAnimacionCamara));
            ControladorMenu.debeAnimarCamaraAlIniciar = false;
        }
        else
        {
            StartCoroutine(IniciarNivelPostAnimacion(0f));
        }

        ActualizarPantalla();
        ActualizarVidasUI();
    }

    private IEnumerator IniciarNivelPostAnimacion(float demora)
    {
        yield return new WaitForSeconds(demora);

        cronometroActivo = true;

        int indiceNivel = ControladorMenu.nivelSeleccionado;

        if (objetoTutorial != null)
        {
            bool activo = (indiceNivel == 0);
            objetoTutorial.SetActive(activo);
            if (activo) StartCoroutine(DesactivarObjetoDespuesDeTiempo(objetoTutorial, 10f));
        }

        if (objetoNivel2 != null)
        {
            bool activo = (indiceNivel == 1);
            objetoNivel2.SetActive(activo);
            if (activo) StartCoroutine(DesactivarObjetoDespuesDeTiempo(objetoNivel2, 10f));
        }

        if (objetoNivel4 != null)
        {
            bool activo = (indiceNivel == 4);
            objetoNivel4.SetActive(activo);
            if (activo) StartCoroutine(DesactivarObjetoDespuesDeTiempo(objetoNivel4, 10f));
        }
    }

    void Update()
    {
        if (cronometroActivo)
        {
            tiempoTranscurrido += Time.deltaTime;
            ActualizarTiempoHUD();
        }

        if (enZonaPuerta && puedeTerminarJuego && Input.GetKeyDown(KeyCode.E))
        {
            cronometroActivo = false;
            if (cartelEntrarPuerta != null) cartelEntrarPuerta.SetActive(false);
            MostrarPantallaVictoria();
        }

        if (ControladorMenu.nivelSeleccionado == 1 && !cartelEspecialMostrado && cronometroActivo)
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

    private IEnumerator DesactivarObjetoDespuesDeTiempo(GameObject objetoTemporal, float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);

        if (objetoTemporal != null)
        {
            objetoTemporal.SetActive(false);
        }
    }

    private void MostrarCartelEspecial()
    {
        cartelEspecialMostrado = true;

        if (cartelClickDerecho != null)
        {
            cartelClickDerecho.SetActive(true);
            StartCoroutine(DesactivarObjetoDespuesDeTiempo(cartelClickDerecho, 10f));
        }

        if (objetoTutorial != null) objetoTutorial.SetActive(false);
        if (objetoNivel2 != null) objetoNivel2.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
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
                }
            }
        }
        else if (other.CompareTag("Puerta"))
        {
            if (puedeTerminarJuego)
            {
                enZonaPuerta = true;
                if (cartelEntrarPuerta != null) cartelEntrarPuerta.SetActive(true);
            }
        }
        else if (other.CompareTag("Enemigo"))
        {
            if (!esInvulnerable)
            {
                PerderVida();
                if (vidas > 0)
                {
                    StartCoroutine(EfectoParpadeo());
                }
            }
        }
        else if (other.CompareTag("Limite"))
        {
            if (!esInvulnerable)
            {
                PerderVida();
                if (vidas > 0)
                {
                    StartCoroutine(EfectoParpadeo());
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Puerta"))
        {
            enZonaPuerta = false;
            if (cartelEntrarPuerta != null) cartelEntrarPuerta.SetActive(false);
        }
    }

    private void PerderVida()
    {
        vidas--;
        ActualizarVidasUI();

        if (vidas > 0)
        {
            transform.position = posicionInicial;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // En vez de recargar la escena de golpe, mostramos la pantalla
            MostrarPantallaDerrota();
        }
    }

    // --- NUEVO: MOSTRAR GAME OVER ---
    private void MostrarPantallaDerrota()
    {

        if (cartelClickDerecho != null) cartelClickDerecho.SetActive(false);
        if (objetoNivel2 != null) objetoNivel2.SetActive(false);
        if (objetoTutorial != null) objetoTutorial.SetActive(false);
        if (objetoNivel4 != null) objetoNivel4.SetActive(false);

        cronometroActivo = false;

        if (pantallaDerrota != null)
        {
            pantallaDerrota.SetActive(true);
        }

        // Pausamos el tiempo para detener enemigos, físicas, etc.
        Time.timeScale = 0f;


    }

    // --- NUEVO: REINICIAR NIVEL DESDE EL BOTÓN ---
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator EfectoParpadeo()
    {
        esInvulnerable = true;

        if (spritePersonaje != null)
        {
            float tiempoFin = Time.time + tiempoInvulnerabilidad;

            while (Time.time < tiempoFin)
            {
                spritePersonaje.enabled = !spritePersonaje.enabled;
                yield return new WaitForSeconds(velocidadParpadeo);
            }

            spritePersonaje.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(tiempoInvulnerabilidad);
        }

        esInvulnerable = false;
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
            if (objetoNivel4 != null) objetoNivel4.SetActive(false);

            pantallaVictoria.SetActive(true);

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
        int proximoNivel = ControladorMenu.nivelSeleccionado + 1;
        int recordActual = PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);

        if (proximoNivel > recordActual)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", proximoNivel);
        }

        PlayerPrefs.SetInt("UltimoNivelJugado", proximoNivel);
        PlayerPrefs.Save();

        ControladorMenu.nivelSeleccionado = proximoNivel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}