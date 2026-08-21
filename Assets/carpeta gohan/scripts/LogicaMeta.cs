using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LogicaMeta : MonoBehaviour
{
    [Header("Monedas Totales Necesarias")]
  [SerializeField] int monedasNecesarias;
  [Header("Nombre de la siguiente escena")]
  [SerializeField] string nombreSiguienteEscena;
  [Header("Tag del Jugador")]
  [SerializeField] string tagJugador;
  
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
    }
    // Método para desbloquear la meta si se han recolectado suficientes monedas
    void DesbloquearMeta()
    {
        if(GameManager.instance.monedas >= monedasNecesarias)
        {
            collider.enabled = true;
        }
        

    }
    // Método que se ejecuta cuando el jugador entra en contacto con la meta
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(tagJugador))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
    }
}
