using System.Collections;
using UnityEngine;

public class LogicaPerder : MonoBehaviour
{
   
   InvertedGravityController Igc;
   BoxCollider boxCollider;
   Rigidbody rigidbody;
[Header("Configuración de Reinicio")]
   [SerializeField] float tiempoReinicio = 1f;
   [SerializeField] Transform puntoReinicio;
   [Header("Configuración de Pinchos y Obstáculos")]
   [SerializeField] string[] tagObstaculos;
   
    void Start()
    {
        Igc = GetComponent<InvertedGravityController>();
        boxCollider = GetComponent<BoxCollider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision other)
    {
        foreach (string tag in tagObstaculos)
        {
            if (other.gameObject.CompareTag(tag))
            {
                Igc.enabled = false;
                boxCollider.enabled = false;
                rigidbody.isKinematic = true;
                StartCoroutine(ReinciarPlayer());
            }
        }
    }

    IEnumerator ReinciarPlayer()
    {
        yield return new WaitForSeconds(tiempoReinicio);
        transform.position = puntoReinicio.position;
        Igc.enabled = true;
        boxCollider.enabled = true;
        rigidbody.isKinematic = false;
    }

    
}
