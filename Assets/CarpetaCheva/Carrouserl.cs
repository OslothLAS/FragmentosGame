using UnityEngine;

public class Carrousel : MonoBehaviour
{
    [Header("Configuración del Tutorial")]
    [Tooltip("El panel principal que envuelve todo (el que se va a apagar al final)")]
    public GameObject panelContenedor;

    [Tooltip("Arrastrá acá tus 4 GameObjects en orden (Paso 1, Paso 2, etc.)")]
    public GameObject[] pasosTutorial;

    private int pasoActual = 0;

    void Start()
    {
        // Nos aseguramos de que el contenedor esté prendido
        if (panelContenedor != null)
        {
            panelContenedor.SetActive(true);
        }

        // Reseteamos el contador a 0 y mostramos solo el primer cartel
        pasoActual = 0;
        ActualizarPantallas();
    }

    // Esta es la función que vas a llamar desde tu botón "Siguiente"
    public void SiguientePaso()
    {
        pasoActual++; // Sumamos 1 al contador

        // Si todavía nos quedan pasos por mostrar...
        if (pasoActual < pasosTutorial.Length)
        {
            ActualizarPantallas();
        }
        else
        {
            // Si ya pasamos el último elemento, apagamos el contenedor principal
            if (panelContenedor != null)
            {
                panelContenedor.SetActive(false);
            }
        }
    }

    private void ActualizarPantallas()
    {
        // Recorremos todos los elementos de la lista
        for (int i = 0; i < pasosTutorial.Length; i++)
        {
            if (pasosTutorial[i] != null)
            {
                // Solo se prende el que coincide con nuestro número de 'pasoActual', el resto se apaga
                pasosTutorial[i].SetActive(i == pasoActual);
            }
        }
    }
}