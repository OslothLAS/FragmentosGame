using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // NECESARIO PARA USAR 'Button'

public class ControladorMenu : MonoBehaviour
{
    public static int nivelSeleccionado = 0;
    public static bool debeAnimarCamaraAlIniciar = false;

    [Header("Configuración de Niveles")]
    public string nombreEscenaJuego = "Enviroment_main";

    [Tooltip("Arrastra aquí los 6 BOTONES de la interfaz gráfica en orden (del Nivel 0 al 5)")]
    public Button[] botonesDeNiveles; // <--- NUEVO: Para bloquear/desbloquear botones

    [Header("Paneles de la Interfaz")]
    public GameObject panelMenuPrincipal;
    public GameObject panelSeleccionNiveles;
    public GameObject panelCreditos;
    public GameObject panelControles;

    void Start()
    {
        MostrarMenuPrincipal();
    }

    public void IniciarAnimacionCamara()
    {
        debeAnimarCamaraAlIniciar = true;
    }

    public void IrASeleccionNiveles()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelSeleccionNiveles != null) panelSeleccionNiveles.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);

        // --- NUEVO: Actualizamos qué botones están bloqueados antes de mostrarlos ---
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
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    private void ActualizarBloqueoDeBotones()
    {
        // Leemos hasta qué nivel llegó el jugador (por defecto 0 si es la primera vez que juega)
        int nivelMaximoDesbloqueado = PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);

        for (int i = 0; i < botonesDeNiveles.Length; i++)
        {
            if (botonesDeNiveles[i] != null)
            {
                // Si el índice del botón es menor o igual al nivel desbloqueado, se puede hacer click
                botonesDeNiveles[i].interactable = (i <= nivelMaximoDesbloqueado);
            }
        }
    }

    // --- FUNCIONES EXTRA ---
    public void AbrirCreditos() { if (panelCreditos != null) panelCreditos.SetActive(true); }
    public void CerrarCreditos() { if (panelCreditos != null) panelCreditos.SetActive(false); }
    public void AbrirControles() { if (panelControles != null) panelControles.SetActive(true); }
    public void CerrarControles() { if (panelControles != null) panelControles.SetActive(false); }

    // Función útil por si quieres poner un botón de "Borrar Progreso" en opciones
    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey("NivelMaximoDesbloqueado");
        ActualizarBloqueoDeBotones();
        Debug.Log("Progreso borrado. Todos los niveles bloqueados excepto el 0.");
    }
}