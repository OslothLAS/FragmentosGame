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
        // 1. Por seguridad, apagamos todos los niveles primero
        foreach (GameObject nivel in niveles)
        {
            nivel.SetActive(false);
        }

        // 2. Leemos la variable estática que seteó el menú
        int indice = ControladorMenu.nivelSeleccionado;

        // 3. Validamos que el índice exista en el array y prendemos ese GameObject
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

    // --- Función para volver al inicio ---
    public void VolverAlMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // --- NUEVA: Función para reiniciar el nivel actual ---
    public void ReiniciarNivel()
    {
        Debug.Log("Reiniciando el nivel...");
        // SceneManager.GetActiveScene().name obtiene el nombre de la escena en la que estás ahora
        // Al cargarla de nuevo, todo vuelve a su estado inicial de fábrica
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}