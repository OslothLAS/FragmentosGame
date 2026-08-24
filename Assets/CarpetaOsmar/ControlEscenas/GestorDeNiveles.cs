using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorDeNiveles : MonoBehaviour
{
    [Header("Tus 6 Niveles")]
    [Tooltip("Arrastrá acá los 6 GameObjects en orden (del 0 al 5)")]
    public GameObject[] niveles;

    [Header("Navegación")]
    [Tooltip("El nombre exacto de tu escena del Menú Principal")]
    public string nombreEscenaMenu = "MenuPrincipal";

    void Start()
    {
        foreach (GameObject nivel in niveles)
        {
            nivel.SetActive(false);
        }
        int indice = ControladorMenu.nivelSeleccionado;

        if (indice >= 0 && indice < niveles.Length)
        {
            niveles[indice].SetActive(true);
            Debug.Log($"Se cargó exitosamente el nivel con índice: {indice}");
        }
        else
        {
            Debug.LogError("Error: El índice del nivel seleccionado está fuera de rango.");
        }
    }

    public void VolverAlMenu()
    {
        Debug.Log("Volviendo al menú principal...");

        // 1. Liberamos y mostramos el cursor ANTES de cambiar de escena
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void ReiniciarNivel()
    {
        Debug.Log("Reiniciando el nivel...");

        // Hacemos lo mismo acá por si el jugador reinicia mientras rota una pieza
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}