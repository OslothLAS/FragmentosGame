using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ControladorMenu : MonoBehaviour
{
    public static int nivelSeleccionado = 0;
    public static bool debeAnimarCamaraAlIniciar = false;

    [Header("Configuración de Niveles")]
    public string nombreEscenaJuego = "Enviroment_main";

    [Tooltip("Arrastra aquí los 6 BOTONES de la interfaz gráfica en orden (del Nivel 0 al 5)")]
    public Button[] botonesDeNiveles;

    [Header("Botón Principal (Jugar / Continuar)")]
    [Tooltip("Arrastra aquí el objeto de Texto (TextMeshPro) que está adentro de tu botón de Jugar")]
    public TextMeshProUGUI textoBotonJugar;

    [Header("Paneles de la Interfaz")]
    public GameObject panelMenuPrincipal;
    public GameObject panelSeleccionNiveles;
    public GameObject panelCreditos;
    public GameObject panelControles;

    void Start()
    {
        MostrarMenuPrincipal();
        ActualizarTextoBotonJugar(); // Chequeamos si debe decir Jugar o Continuar
    }

    public void JugarOContinuar()
    {
        // --- NUEVO LÓGICA DE ANIMACIÓN ---
        // Si NO hay nivel guardado, es porque el botón dice "Jugar". Activamos la animación.
        if (!PlayerPrefs.HasKey("UltimoNivelJugado"))
        {
            debeAnimarCamaraAlIniciar = true;
        }
        else
        {
            // Si ya hay nivel guardado, el botón dice "Continuar". Apagamos la animación por seguridad.
            debeAnimarCamaraAlIniciar = false;
        }

        // Leemos el último nivel jugado. Si no existe (primera vez), arranca en 0
        int nivelACargar = PlayerPrefs.GetInt("UltimoNivelJugado", 0);

        SeleccionarYCargarNivel(nivelACargar);
    }

    private void ActualizarTextoBotonJugar()
    {
        if (textoBotonJugar != null)
        {
            if (PlayerPrefs.HasKey("UltimoNivelJugado"))
            {
                textoBotonJugar.text = "Continuar";
            }
            else
            {
                textoBotonJugar.text = "Jugar";
            }
        }
    }

    public void IniciarAnimacionCamara()
    {
        // Le agregamos la misma validación por si seguís usando esta función suelta en algún botón
        if (!PlayerPrefs.HasKey("UltimoNivelJugado"))
        {
            debeAnimarCamaraAlIniciar = true;
        }
    }

    public void IrASeleccionNiveles()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelSeleccionNiveles != null) panelSeleccionNiveles.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);

        ActualizarBloqueoDeBotones();
    }

    public void MostrarMenuPrincipal()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelSeleccionNiveles != null) panelSeleccionNiveles.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
    }

    public void SeleccionarYCargarNivel(int indiceNivel)
    {
        nivelSeleccionado = indiceNivel;

        PlayerPrefs.SetInt("UltimoNivelJugado", indiceNivel);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    private void ActualizarBloqueoDeBotones()
    {
        int nivelMaximoDesbloqueado = PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);

        for (int i = 0; i < botonesDeNiveles.Length; i++)
        {
            if (botonesDeNiveles[i] != null)
            {
                botonesDeNiveles[i].interactable = (i <= nivelMaximoDesbloqueado);
            }
        }
    }

    public void AbrirCreditos() { if (panelCreditos != null) panelCreditos.SetActive(true); }
    public void CerrarCreditos() { if (panelCreditos != null) panelCreditos.SetActive(false); }
    public void AbrirControles() { if (panelControles != null) panelControles.SetActive(true); }
    public void CerrarControles() { if (panelControles != null) panelControles.SetActive(false); }


    // ==========================================
    // HERRAMIENTAS DE DESARROLLADOR (DEV TOOLS)
    // ==========================================

    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey("NivelMaximoDesbloqueado");
        PlayerPrefs.DeleteKey("UltimoNivelJugado");

        ActualizarBloqueoDeBotones();
        ActualizarTextoBotonJugar();

        Debug.Log("<color=red>[DEV]</color> Progreso borrado. Todos los niveles bloqueados excepto el 0.");
    }

    public void DesbloquearTodosLosNiveles()
    {
        // Desbloqueamos hasta el último índice de tu arreglo de botones
        int nivelMaximo = botonesDeNiveles.Length - 1;

        PlayerPrefs.SetInt("NivelMaximoDesbloqueado", nivelMaximo);
        PlayerPrefs.Save();

        ActualizarBloqueoDeBotones();

        Debug.Log("<color=green>[DEV]</color> ¡Truco activado! Todos los niveles desbloqueados.");
    }
}