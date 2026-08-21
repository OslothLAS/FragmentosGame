using UnityEngine;

public class Desaparecer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Verifica si el objeto contra el que chocamos tiene el tag "Fragmento"
        if (collision.gameObject.CompareTag("Fragmento"))
        {
            // Destruye el objeto que tiene este script asignado
            Destroy(gameObject);
        }
    }
}