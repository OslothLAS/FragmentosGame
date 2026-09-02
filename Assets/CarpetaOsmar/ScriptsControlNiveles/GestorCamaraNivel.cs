using UnityEngine;

public class GestorCamaraNivel : MonoBehaviour
{
    [Tooltip("El Animator de la cámara en esta escena")]
    public Animator animatorCamara;

    void Start()
    {
        // Verificamos si en el menú anterior se presionó el botón que activó la bandera
        if (ControladorMenu.debeAnimarCamaraAlIniciar)
        {
            if (animatorCamara != null)
            {
                animatorCamara.SetTrigger("IniciarCamara");
            }


        }
    }
}