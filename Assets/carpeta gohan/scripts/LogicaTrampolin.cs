using UnityEngine;

public class LogicaTrampolin : MonoBehaviour
{
    [Header("Fuerza del trampolin")]
   [SerializeField] float fuerzaTrampolin;
   [Header("Tag del Jugador")]
   [SerializeField] string tagJugador;
   

   void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(tagJugador))
        {
           

              if (collision.contacts[0].normal.z < -0.5f)
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward * fuerzaTrampolin, ForceMode.Impulse);
        }
    }
}
