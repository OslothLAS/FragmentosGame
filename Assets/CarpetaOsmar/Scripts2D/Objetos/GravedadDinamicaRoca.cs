using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class GravedadDinamicaRoca : MonoBehaviour
{
    [Header("Detección por UV")]
    [Tooltip("La cámara ortográfica 2D que filma los objetos")]
    public Camera camaraRender;

    private DetectorUV[] todosLosDetectores;
    private Transform fragmentoActual;
    private Dictionary<Transform, Quaternion> memoriaRotaciones = new Dictionary<Transform, Quaternion>();

    [Header("Configuración de Gravedad y Velocidad")]
    public float fuerzaGravedad = 20f;
    public float margenGrados = 20f;

    // NUEVO: Límite de velocidad
    [Tooltip("La velocidad máxima absoluta a la que puede viajar la roca.")]
    public float velocidadMaxima = 15f;

    public bool invertirGravedadX = false;
    public bool invertirGravedadY = false;

    [Header("Giro Visual y Inercia")]
    [Tooltip("El objeto hijo que contiene el Sprite. Solo rotará la imagen.")]
    public Transform spriteVisual;

    [Tooltip("Multiplicador del giro.")]
    public float multiplicadorGiro = 150f;

    [Tooltip("Qué tan rápido frena la bola por la resistencia general. Valores bajos (0.2 a 0.8) hacen que conserve mucho la inercia.")]
    public float inerciaFrenado = 0.4f;

    [Tooltip("Si la velocidad X es menor a esto, frena el giro visual para que no tiemble.")]
    public float umbralMovimiento = 0.02f;

    private Vector2 vectorGravedad = Vector2.down;
    private Rigidbody2D rb;

    void Start()
    {
        gameObject.tag = "Roca";

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // --- INERCIA SUAVE ---
        rb.linearDamping = inerciaFrenado;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            PhysicsMaterial2D materialRoca = new PhysicsMaterial2D("MaterialRoca");
            materialRoca.friction = 0f;
            materialRoca.bounciness = 0.1f;
            col.sharedMaterial = materialRoca;
        }

        if (spriteVisual == null)
        {
            Debug.LogWarning("Falta asignar el 'Sprite Visual' en el Inspector.");
        }

        todosLosDetectores = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude);
        foreach (DetectorUV detector in todosLosDetectores)
        {
            memoriaRotaciones.Add(detector.transform, detector.transform.rotation);
        }
    }

    void Update()
    {
        if (spriteVisual != null)
        {
            float velX = rb.linearVelocity.x;

            if (Mathf.Abs(velX) < umbralMovimiento)
            {
                velX = 0f;
            }

            float giroVisual = velX * multiplicadorGiro;
            spriteVisual.Rotate(0f, 0f, giroVisual * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        DetectarGravedadPorUV();

        // 1. Aplicamos la gravedad constante
        rb.AddForce(vectorGravedad * (fuerzaGravedad * rb.mass), ForceMode2D.Force);

        // 2. NUEVO: Bloqueo estricto de velocidad máxima
        if (rb.linearVelocity.magnitude > velocidadMaxima)
        {
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, velocidadMaxima);
        }
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
}