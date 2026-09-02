using UnityEngine;
using System.Collections;

public class ComportamientoAgua : MonoBehaviour
{
    [Tooltip("Segundos que tarda en achicarse y desaparecer")]
    public float tiempoDesaparicion = 2f;

    private bool tocandoSuelo = false;

    // Detecta si choca contra un suelo sólido
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Log para ver con qué chocó de forma física
        Debug.Log($"[FÍSICA] El agua chocó con: {collision.gameObject.name} | Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Suelo") && !tocandoSuelo)
        {
            Debug.Log("-> ¡Tag 'Suelo' detectado! Iniciando rutina de achicarse.");
            StartCoroutine(AchicarYDestruir());
        }
    }

    // Detecta si pasa por un suelo configurado como Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log para ver si entró en un área Trigger
        Debug.Log($"[TRIGGER] El agua entró en el trigger de: {other.gameObject.name} | Tag: {other.gameObject.tag}");

        if (other.CompareTag("Suelo") && !tocandoSuelo)
        {
            Debug.Log("-> ¡Tag 'Suelo' detectado en Trigger! Iniciando rutina de achicarse.");
            StartCoroutine(AchicarYDestruir());
        }
    }

    private IEnumerator AchicarYDestruir()
    {
        // Bloqueamos para que no se ejecute dos veces
        tocandoSuelo = true;

        Vector3 escalaInicial = transform.localScale;
        float tiempoTranscurrido = 0f;

        // Bucle que se ejecuta frame a frame durante los 2 segundos
        while (tiempoTranscurrido < tiempoDesaparicion)
        {
            tiempoTranscurrido += Time.deltaTime;

            // Calculamos de 0.0 a 1.0 qué tan avanzado está el tiempo
            float porcentaje = tiempoTranscurrido / tiempoDesaparicion;

            // Interpolamos la escala progresivamente
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, porcentaje);

            // Esperamos al siguiente frame para continuar el bucle
            yield return null;
        }

        Debug.Log("-> El agua terminó de achicarse y se destruye el objeto.");
        // Una vez terminados los 2 segundos, destruimos el objeto
        Destroy(gameObject);
    }
}