using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenu : MonoBehaviour
{
    public static int nivelSeleccionado = 0;

    // --- NUEVA VARIABLE ESTÁTICA ---
    public static bool debeAnimarCamaraAlIniciar = false;

    [Header("Configuración de Niveles")]
    public string nombreEscenaJuego = "Enviroment_main";

    [Header("Paneles de la Interfaz")]
    public GameObject panelMenuPrincipal;
    public GameObject panelSeleccionNiveles;
    public GameObject panelCreditos;
    public GameObject panelControles;

    void Start()
    {

        MostrarMenuPrincipal();
    }

    // --- FUNCIÓN INDEPENDIENTE PARA LA CÁMARA ---
    public void IniciarAnimacionCamara()
    {
        // Encendemos la bandera. La animación ocurrirá cuando cargue la otra escena.
        debeAnimarCamaraAlIniciar = true;
    }

    // --- NAVEGACIÓN ENTRE PANTALLAS ---
    public void IrASeleccionNiveles()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelSeleccionNiveles != null) panelSeleccionNiveles.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
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

    public void AbrirCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    public void AbrirControles()
    {
        if (panelControles != null) panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        if (panelControles != null) panelControles.SetActive(false);
    }
}