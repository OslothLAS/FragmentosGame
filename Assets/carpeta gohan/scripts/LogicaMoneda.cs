using UnityEngine;

public class LogicaMoneda : MonoBehaviour
{
   [Header("Tag del Jugador")]
   [SerializeField] string tagJugador;
   void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(tagJugador))
        {
            GameManager.instance.SumarMoneda();
              Destroy(this.gameObject); 
            
        }
      

    }
    
}
