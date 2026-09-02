using UnityEngine;

public class ComportamientoFuego : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Agua"))
        {
            Destroy(gameObject);
        }
    }


}