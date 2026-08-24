using UnityEngine;

public class Carrousel : MonoBehaviour
{
    [Header("Configuración del Tutorial")]
    public GameObject panelContenedor;
    public GameObject[] pasosTutorial;

    [Tooltip("El nombre interno para guardar que ya se vio este tutorial")]
    public string claveGuardado = "TutorialCompletado";

    private int pasoActual = 0;

    void Start()
    {
        // 1. CHEQUEO: Preguntamos si en el disco duro ya quedó guardado el valor '1' (Completado)
        if (PlayerPrefs.GetInt(claveGuardado, 0) == 1)
        {
            // Ya lo completó antes. Apagamos el panel y cortamos el Start acá mismo.
            if (panelContenedor != null) panelContenedor.SetActive(false);
            return;
        }

        // Si no lo completó (da 0), hacemos la secuencia normal:
        if (panelContenedor != null)
        {
            panelContenedor.SetActive(true);
        }

        pasoActual = 0;
        ActualizarPantallas();
    }

    public void SiguientePaso()
    {
        pasoActual++;

        if (pasoActual < pasosTutorial.Length)
        {
            ActualizarPantallas();
        }
        else
        {
            // 2. GUARDADO: Como pasamos el último cartel, guardamos un '1' de forma permanente
            PlayerPrefs.SetInt(claveGuardado, 1);
            PlayerPrefs.Save();

            if (panelContenedor != null)
            {
                panelContenedor.SetActive(false);
            }
        }
    }

    private void ActualizarPantallas()
    {
        for (int i = 0; i < pasosTutorial.Length; i++)
        {
            if (pasosTutorial[i] != null)
            {
                pasosTutorial[i].SetActive(i == pasoActual);
            }
        }
    }

    // --- TRUCO PARA TESTEO ---
    // Esto te permite hacer clic derecho sobre el script en el Inspector y resetear la variable
    [ContextMenu("Resetear Tutorial (Solo para Pruebas)")]
    public void ResetearTutorial()
    {
        PlayerPrefs.DeleteKey(claveGuardado);
        Debug.Log("Tutorial reseteado. Volverá a aparecer la próxima vez que des Play.");
    }
}