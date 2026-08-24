using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenu : MonoBehaviour
{
    // Variable estática para recordar el nivel al cambiar de escena
    public static int nivelSeleccionado = 0;

    [Header("Configuración de Niveles")]
    [Tooltip("El nombre exacto de tu escena de juego")]
    public string nombreEscenaJuego = "Enviroment_main";

    [Header("Interfaz de Usuario")]
    [Tooltip("Arrastrá acá el Panel que contiene los créditos")]
    public GameObject panelCreditos;

    void Start()
    {

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
        }
    }

    // --- LÓGICA DE NIVELES (Botones 1 al 6) ---
    public void SeleccionarYCargarNivel(int indiceNivel)
    {
        nivelSeleccionado = indiceNivel;
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // --- LÓGICA DE CRÉDITOS (7mo Botón y Botón de Cerrar) ---

    public void AbrirCreditos()
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Falta asignar el Panel de Créditos en el Inspector.");
        }
    }

    public void CerrarCreditos()
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
        }
    }
}