using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class GravedadDinamicaObjeto : MonoBehaviour
{
    [Header("Detección por UV")]
    [Tooltip("La cámara ortográfica 2D que filma los objetos")]
    public Camera camaraRender;

    private DetectorUV[] todosLosDetectores;
    private Transform fragmentoActual;
    private Dictionary<Transform, Quaternion> memoriaRotaciones = new Dictionary<Transform, Quaternion>();

    [Header("Configuración de Gravedad")]
    public float fuerzaGravedad = 15f;
    public float margenGrados = 20f;
    public bool invertirGravedadX = false;
    public bool invertirGravedadY = false;

    [Header("Cohesión del Agua")]
    [Tooltip("Fuerza necesaria para que dos gotas unidas se separen")]
    public float fuerzaParaRomper = 300f;
    [Tooltip("Qué tan rígida o gelatinosa es la unión")]
    public float elasticidad = 3f;

    private Vector2 vectorGravedad = Vector2.down;
    private Rigidbody2D rb;

    void Start()
    {
        gameObject.tag = "Agua";

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            PhysicsMaterial2D materialAgua = new PhysicsMaterial2D("MaterialAgua");
            // Le damos una leve fricción por código para que el Rigidbody la haga rodar al tocar superficies
            materialAgua.friction = 0.2f;
            materialAgua.bounciness = 0f;
            col.sharedMaterial = materialAgua;
        }
        else
        {
            Debug.LogWarning("La gota de agua necesita un Collider2D.");
        }

        todosLosDetectores = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude);
        foreach (DetectorUV detector in todosLosDetectores)
        {
            memoriaRotaciones.Add(detector.transform, detector.transform.rotation);
        }
    }

    void FixedUpdate()
    {
        DetectarGravedadPorUV();

        // El Rigidbody se encarga al 100% de la rotación y las físicas
        rb.AddForce(vectorGravedad * (fuerzaGravedad * rb.mass), ForceMode2D.Force);
    }

    private void DetectarGravedadPorUV()
    {
        if (camaraRender == null || todosLosDetectores.Length == 0) return;

        Vector2 uvObjeto = camaraRender.WorldToViewportPoint(transform.position);
        bool encontrado = false;

        foreach (DetectorUV detector in todosLosDetectores)
        {
            if (detector.ContieneAlPersonaje(uvObjeto))
            {
                encontrado = true;
                Transform fragmentoEncontrado = detector.transform;
                ArrastrarPiezaXZ scriptArrastre = fragmentoEncontrado.GetComponent<ArrastrarPiezaXZ>();

                if (fragmentoActual != fragmentoEncontrado)
                {
                    fragmentoActual = fragmentoEncontrado;
                }

                if (scriptArrastre != null && scriptArrastre.EstaSiendoManipulada) return;

                CalcularNuevaGravedad(fragmentoActual);
                break;
            }
        }

        if (!encontrado && fragmentoActual != null)
        {
            fragmentoActual = null;
        }
    }

    private void CalcularNuevaGravedad(Transform fragmento)
    {
        Quaternion rotacionOriginal = memoriaRotaciones[fragmento];
        Vector3 ejeLocalDerecha = Quaternion.Inverse(rotacionOriginal) * Vector3.right;
        Quaternion diferenciaRotacion = fragmento.rotation * Quaternion.Inverse(rotacionOriginal);

        Vector3 derechaActual = fragmento.rotation * ejeLocalDerecha;
        derechaActual.y = 0;
        if (derechaActual.sqrMagnitude > 0.001f) derechaActual.Normalize();

        float anguloY = Vector3.SignedAngle(Vector3.right, derechaActual, Vector3.up);

        Vector3 normalRelativa = diferenciaRotacion * Vector3.up;
        if (normalRelativa.y < 0)
        {
            anguloY = -anguloY;
        }

        Vector2 nuevaGravedad = Vector2.down;

        if (Mathf.Abs(anguloY) > margenGrados)
        {
            if (anguloY > margenGrados && anguloY <= 135f) nuevaGravedad = Vector2.left;
            else if (anguloY < -margenGrados && anguloY >= -135f) nuevaGravedad = Vector2.right;
            else if (Mathf.Abs(anguloY) > 135f) nuevaGravedad = Vector2.up;
        }

        AplicarGravedad(nuevaGravedad);
    }

    private void AplicarGravedad(Vector2 nuevaGravedad)
    {
        if (invertirGravedadX)
        {
            if (nuevaGravedad == Vector2.left) nuevaGravedad = Vector2.right;
            else if (nuevaGravedad == Vector2.right) nuevaGravedad = Vector2.left;
        }

        if (invertirGravedadY)
        {
            if (nuevaGravedad == Vector2.down) nuevaGravedad = Vector2.up;
            else if (nuevaGravedad == Vector2.up) nuevaGravedad = Vector2.down;
        }

        vectorGravedad = nuevaGravedad;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Agua"))
        {
            Rigidbody2D otroRb = collision.rigidbody;

            if (otroRb != null && !YaEstanConectados(otroRb))
            {
                UnirGotas(otroRb);
            }
        }
    }

    private void UnirGotas(Rigidbody2D otroRb)
    {
        SpringJoint2D union = gameObject.AddComponent<SpringJoint2D>();
        union.connectedBody = otroRb;

        // --- SOLUCIÓN AL BUG ORIGINAL DEL GIRO INFINITO ---
        union.autoConfigureConnectedAnchor = false;
        union.anchor = Vector2.zero;
        union.connectedAnchor = Vector2.zero;
        union.autoConfigureDistance = true;

        union.dampingRatio = 0.8f;
        union.frequency = elasticidad;
        union.breakForce = fuerzaParaRomper;
    }

    private bool YaEstanConectados(Rigidbody2D otroRb)
    {
        SpringJoint2D[] uniones = GetComponents<SpringJoint2D>();
        foreach (SpringJoint2D union in uniones)
        {
            if (union.connectedBody == otroRb) return true;
        }
        return false;
    }
}