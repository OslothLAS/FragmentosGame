using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))] // Modificado para requerir AudioSource
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

    [Tooltip("Si la inclinación es menor a este ángulo, la gravedad apuntará directo abajo. Ponlo en 0 para que la roca ruede con la más mínima inclinación.")]
    public float margenGrados = 2f;

    [Tooltip("La velocidad máxima absoluta a la que puede viajar la roca.")]
    public float velocidadMaxima = 15f;

    public bool invertirGravedadX = false;
    public bool invertirGravedadY = false;

    [Header("Giro Visual e Inercia")]
    [Tooltip("El objeto hijo que contiene el Sprite. Solo rotará la imagen.")]
    public Transform spriteVisual;

    [Tooltip("Multiplicador del giro.")]
    public float multiplicadorGiro = 150f;

    [Tooltip("Qué tan rápido frena la bola. Valores bajos (0.2 a 0.8) conservan mucho la inercia.")]
    public float inerciaFrenado = 0.4f;

    [Tooltip("Si la velocidad X es menor a esto, frena el giro visual para que no tiemble.")]
    public float umbralMovimiento = 0.02f;

    [Header("Audio (Rodar)")]
    [Tooltip("Clip de sonido que se reproducirá mientras la roca ruede")]
    public AudioClip sonidoRodar;

    [Tooltip("Volumen máximo que alcanzará el sonido al ir rápido")]
    [Range(0f, 1f)] public float volumenMaximo = 1f;

    [Tooltip("A qué velocidad debe ir la roca para que el sonido suene al máximo de su volumen")]
    public float velocidadParaVolumenMaximo = 10f;

    private Vector2 vectorGravedad = Vector2.down;
    private Rigidbody2D rb;
    private AudioSource audioSourceRoca;

    void Start()
    {
        gameObject.tag = "Roca";

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
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

        // Configuración inicial del Audio
        audioSourceRoca = GetComponent<AudioSource>();
        audioSourceRoca.playOnAwake = false; // Evitamos que suene de golpe al iniciar

        if (sonidoRodar != null)
        {
            audioSourceRoca.clip = sonidoRodar;
            audioSourceRoca.loop = true; // Fundamental para que el sonido no se corte
            audioSourceRoca.volume = 0f;
            audioSourceRoca.Play(); // Lo reproducimos silenciado y subiremos el volumen en el Update
        }
        else
        {
            Debug.LogWarning("Falta asignar el 'Sonido Rodar' en el Inspector.");
        }

        todosLosDetectores = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude);
        foreach (DetectorUV detector in todosLosDetectores)
        {
            memoriaRotaciones.Add(detector.transform, detector.transform.rotation);
        }
    }

    void Update()
    {
        float velocidadActual = rb.linearVelocity.magnitude;
        float velX = rb.linearVelocity.x;

        if (Mathf.Abs(velX) < umbralMovimiento)
        {
            velX = 0f;
        }

        if (spriteVisual != null)
        {
            float giroVisual = velX * multiplicadorGiro;
            spriteVisual.Rotate(0f, 0f, giroVisual * Time.deltaTime);
        }

        // --- GESTIÓN DINÁMICA DEL AUDIO ---
        if (audioSourceRoca != null && audioSourceRoca.isPlaying)
        {
            if (velocidadActual > umbralMovimiento)
            {
                // Calculamos qué porcentaje del volumen máximo deberíamos estar aplicando (0 a 1)
                float porcentajeVolumen = Mathf.Clamp01(velocidadActual / velocidadParaVolumenMaximo);
                float volumenObjetivo = porcentajeVolumen * volumenMaximo;

                // Suavizamos la transición del volumen con Lerp para que no sea brusco
                audioSourceRoca.volume = Mathf.Lerp(audioSourceRoca.volume, volumenObjetivo, Time.deltaTime * 10f);

                // Opcional: Variamos ligeramente el pitch (tono) para dar sensación de pesadez al acelerar
                audioSourceRoca.pitch = Mathf.Lerp(0.8f, 1.2f, porcentajeVolumen);
            }
            else
            {
                // Si la roca se detiene, bajamos el volumen a 0 suavemente
                audioSourceRoca.volume = Mathf.Lerp(audioSourceRoca.volume, 0f, Time.deltaTime * 15f);
            }
        }
    }

    void FixedUpdate()
    {
        DetectarGravedadPorUV();

        // Aplicamos la gravedad compuesta constante
        rb.AddForce(vectorGravedad * (fuerzaGravedad * rb.mass), ForceMode2D.Force);

        // Bloqueo estricto de velocidad máxima
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

        if (Mathf.Abs(anguloY) <= margenGrados)
        {
            anguloY = 0f;
        }

        float anguloRadianes = anguloY * Mathf.Deg2Rad;

        float gravedadX = -Mathf.Sin(anguloRadianes);
        float gravedadY = -Mathf.Cos(anguloRadianes);

        Vector2 nuevaGravedad = new Vector2(gravedadX, gravedadY).normalized;

        AplicarGravedad(nuevaGravedad);
    }

    private void AplicarGravedad(Vector2 nuevaGravedad)
    {
        if (invertirGravedadX) nuevaGravedad.x = -nuevaGravedad.x;
        if (invertirGravedadY) nuevaGravedad.y = -nuevaGravedad.y;

        vectorGravedad = nuevaGravedad;
    }
}