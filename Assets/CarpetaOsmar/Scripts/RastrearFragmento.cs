using UnityEngine;

public class RastreadorDeFragmentos : MonoBehaviour
{
    [Header("Referencias de Renderizado")]
    [Tooltip("La cámara que graba a este personaje (La que tiene el Render Texture)")]
    public Camera camaraRender;
    [Tooltip("El Quad base/invisible de la mesa que usaste como referencia para los UVs")]
    public Transform pantallaReferencia;

    private GameObject fragmentoActual;

    void Update()
    {
        RastrearFragmento();
    }

    private void RastrearFragmento()
    {
        if (camaraRender == null || pantallaReferencia == null) return;

        // 1. Calculamos dónde está el personaje en la cámara de renderizado
        Vector3 viewportPos = camaraRender.WorldToViewportPoint(transform.position);

        // 2. Traducimos esa posición 2D al plano 3D de la mesa
        Vector3 posicionRelativa = new Vector3(viewportPos.x - 0.5f, viewportPos.y - 0.5f, 0f);
        Vector3 posicionEnMesa = pantallaReferencia.TransformPoint(posicionRelativa);

        // 3. Tiramos un rayo desde arriba hacia abajo en la mesa
        Vector3 origenRayo = posicionEnMesa + Vector3.up * 5f;

        // Dibuja una línea roja en la ventana Scene para que puedas ver exactamente dónde está midiendo
        Debug.DrawRay(origenRayo, Vector3.down * 10f, Color.red);

        if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 10f))
        {
            if (hit.collider.CompareTag("Fragmento"))
            {
                GameObject nuevoFragmento = hit.collider.gameObject;

                // Solo disparamos el log si el fragmento cambió
                if (nuevoFragmento != fragmentoActual)
                {
                    fragmentoActual = nuevoFragmento;
                    Debug.Log($"Personaje renderizado en el fragmento: {fragmentoActual.name}");
                }
            }
        }
        else
        {
            // Si el rayo no toca nada con el tag "Fragmento", limpiamos el registro
            if (fragmentoActual != null)
            {
                Debug.Log("El personaje ya no está sobre ningún fragmento.");
                fragmentoActual = null;
            }
        }
    }
}