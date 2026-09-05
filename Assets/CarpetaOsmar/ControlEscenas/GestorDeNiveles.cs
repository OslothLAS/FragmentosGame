using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorDeNiveles : MonoBehaviour
{
    [Header("Tus 6 Niveles")]
    [Tooltip("Arrastrá acá los 6 GameObjects en orden (del 0 al 5)")]
    public GameObject[] niveles;

    [Header("Fondo Simple por Nivel")]
    [Tooltip("El material que usa tu SG_GlassCanvas")]
    public Material materialFondo;

    [Tooltip("Arrastrá acá la imagen correspondiente a cada nivel")]
    public Texture2D[] texturasDeFondo;

    [Tooltip("El 'Reference' exacto de la textura en tu Shader Graph")]
    public string referenciaTexturaShader = "_FondoNivel";

    [Header("Navegación")]
    public string nombreEscenaMenu = "MenuPrincipal";

    void Start()
    {
        // 1. Apagamos todos los niveles por seguridad
        foreach (GameObject nivel in niveles)
        {
            nivel.SetActive(false);
        }

        // 2. Obtenemos el nivel elegido desde el menú
        int indice = ControladorMenu.nivelSeleccionado;

        if (indice >= 0 && indice < niveles.Length)
        {
            // Prendemos solo el nivel correspondiente
            niveles[indice].SetActive(true);

            // 3. Inyectamos SOLAMENTE la textura, sin cálculos de escala ni ajustes extra
            if (materialFondo != null && indice < texturasDeFondo.Length)
            {
                if (texturasDeFondo[indice] != null)
                {
                    materialFondo.SetTexture(referenciaTexturaShader, texturasDeFondo[indice]);
                }
            }

            Debug.Log($"Se cargó exitosamente el nivel con índice: {indice}");
        }
        else
        {
            Debug.LogError("Error: El índice del nivel seleccionado está fuera de rango.");
        }
    }

    public void VolverAlMenu()
    {
        Cursor.visible = true;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void ReiniciarNivel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}