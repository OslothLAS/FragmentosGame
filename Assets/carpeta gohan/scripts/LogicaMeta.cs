using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LogicaMeta : MonoBehaviour
{
    [Header("Monedas Totales Necesarias")]
  [SerializeField] int monedasNecesarias;
  [SerializeField] int monedasTotales;
  [Header("Nombre de la siguiente escena")]
  [SerializeField] string nombreSiguienteEscena, nombreEscenaExtra;
  [Header("Tag del Jugador")]
  [SerializeField] string tagJugador;
  [Header("Booleano para el evento extra")]
  [SerializeField] bool eventoExtra;

  BoxCollider collider;

    void Start()
    {
        // Obtener el componente BoxCollider del objeto y configurarlo como trigger y desactivarlo al inicio
        collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.enabled = false;
    }

    
    void Update()
    {
        DesbloquearMeta();
        DesbloquearMetaEventoExtra();
    }
    // Método para desbloquear la meta si se han recolectado suficientes monedas
    void DesbloquearMeta()
    {
        if(GameManager.instance.monedas >= monedasNecesarias)
        {
            eventoExtra = false;
            collider.enabled = true;
        }
      

        

    }
    void DesbloquearMetaEventoExtra()
    {
        if(GameManager.instance.monedas == monedasTotales)
        {
            eventoExtra = true;
        }
       
    }
    // Método que se ejecuta cuando el jugador entra en contacto con la meta
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(tagJugador) && !eventoExtra)
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else if(other.CompareTag(tagJugador) && eventoExtra)
        {
            SceneManager.LoadScene(nombreEscenaExtra);
        }
    }
}
