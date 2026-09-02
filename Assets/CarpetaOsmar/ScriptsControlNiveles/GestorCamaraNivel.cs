using UnityEngine;
using System.Collections;

public class GestorCamaraNivel : MonoBehaviour
{
    [Tooltip("El Animator de la cámara en esta escena")]
    public Animator animatorCamara;

    void Start()
    {
        if (ControladorMenu.debeAnimarCamaraAlIniciar)
        {
            if (animatorCamara != null)
            {
                // Nos aseguramos de que el Animator esté prendido para animar
                animatorCamara.enabled = true;
                animatorCamara.SetTrigger("IniciarCamara");

                // Iniciamos la cuenta regresiva para apagarlo
                StartCoroutine(LiberarCamara());
            }
        }
        else if (animatorCamara != null)
        {
            // Si entramos al nivel sin la bandera activada, apagamos el Animator de entrada
            // para que el Parallax funcione desde el segundo cero.
            animatorCamara.enabled = false;
        }
    }

    private IEnumerator LiberarCamara()
    {
        // 1. Esperamos un frame para que Unity procese el Trigger y cambie al estado de animación
        yield return null;

        // 2. Leemos automáticamente cuánto dura la animación en segundos
        float duracionAnimacion = animatorCamara.GetCurrentAnimatorStateInfo(0).length;

        // 3. Pausamos la corrutina hasta que termine la animación
        yield return new WaitForSeconds(duracionAnimacion);

        // 4. Apagamos el Animator. En este exacto momento, el script EfectoParallaxCamara toma el control.
        animatorCamara.enabled = false;
    }
}