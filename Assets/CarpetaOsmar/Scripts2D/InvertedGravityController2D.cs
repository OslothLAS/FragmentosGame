using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class InvertedGravityController2D : MonoBehaviour
{
    [Header("Detección por UV (Lógica Pura)")]
    [Tooltip("La cámara ortográfica 2D que filma al personaje")]
    public Camera camaraRender;

    // Lista de todos los detectores en la escena
    private DetectorUV[] todosLosDetectores;
    private Transform fragmentoActual;

    // MEMORIA DE ROTACIONES: Guarda cómo estaba cada pieza al iniciar el nivel
    private Dictionary<Transform, Quaternion> memoriaRotaciones = new Dictionary<Transform, Quaternion>();

    [Header("Mecánica de Inclinación")]
    public float margenGrados = 20f;
    public bool invertirGravedadX = false;
    public bool invertirGravedadY = false;

    [Header("Velocidad General")]
    public float velocidadMax = 8f;

    [Header("Suavizado en Suelo/Aire")]
    public float aceleracionSuelo = 60f;
    public float desaceleracionSuelo = 60f;
    public float aceleracionAire = 15f;
    public float desaceleracionAire = 5f;

    [Header("Salto y Gravedad")]
    public float fuerzaSalto = 10f;
    public float fuerzaGravedad = 15f;
    public float velocidadEscalada = 5f;

    [Header("Animación y Visuales")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Vectores Dinámicos")]
    public Vector2 vectorGravedad = Vector2.down;
    public Vector2 vectorSalto = Vector2.up;
    public Vector2 vectorDerecha = Vector2.right;

    private Rigidbody2D rb;
    private bool estaEnSuelo;
    private bool enEscalera;
    private bool escalando;
    private Quaternion rotacionSpriteInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) rotacionSpriteInicial = spriteRenderer.transform.localRotation;

        todosLosDetectores = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude);

        // --- FOTOGRAFÍA INICIAL ---
        // Guardamos la rotación exacta de cada fragmento al arrancar el juego
        foreach (DetectorUV detector in todosLosDetectores)
        {
            memoriaRotaciones.Add(detector.transform, detector.transform.rotation);
        }
    }

    void Update()
    {
        ManejarSalto();
        ManejarAnimacionesVisuales();
    }

    void FixedUpdate()
    {
        DetectarGravedadPorUV();

        if (!escalando)
        {
            rb.AddForce(vectorGravedad * (fuerzaGravedad * rb.mass), ForceMode2D.Force);
        }

        ManejarMovimiento();
        estaEnSuelo = false;
    }

    private void DetectarGravedadPorUV()
    {
        if (camaraRender == null || todosLosDetectores.Length == 0) return;

        Vector2 uvPersonaje = camaraRender.WorldToViewportPoint(transform.position);
        bool encontrado = false;

        foreach (DetectorUV detector in todosLosDetectores)
        {
            if (detector.ContieneAlPersonaje(uvPersonaje))
            {
                encontrado = true;
                Transform fragmentoEncontrado = detector.transform;

                ArrastrarPiezaXZ scriptArrastre = fragmentoEncontrado.GetComponent<ArrastrarPiezaXZ>();

                if (fragmentoActual != fragmentoEncontrado)
                {
                    fragmentoActual = fragmentoEncontrado;
                    Debug.Log($"<color=cyan>[Gravedad] Personaje entró al fragmento (Por UV): {fragmentoActual.name}</color>");
                }

                if (scriptArrastre != null && scriptArrastre.EstaSiendoManipulada) return;

                // Ahora calculamos la rotación internamente sin depender de otros scripts
                CalcularNuevaGravedad(fragmentoActual);

                break;
            }
        }

        if (!encontrado && fragmentoActual != null)
        {
            Debug.Log("<color=red>[Gravedad] El personaje salió del mapa UV (Vacío).</color>");
            fragmentoActual = null;
        }
    }

    // --- EL CEREBRO MATEMÁTICO 3D A 2D ---
    private void CalcularNuevaGravedad(Transform fragmento)
    {
        // 1. Recuperamos cómo estaba la pieza al principio
        Quaternion rotacionOriginal = memoriaRotaciones[fragmento];
        Vector3 ejeLocalDerecha = Quaternion.Inverse(rotacionOriginal) * Vector3.right;
        Quaternion diferenciaRotacion = fragmento.rotation * Quaternion.Inverse(rotacionOriginal);

        // 2. ¿Hacia dónde apunta ahora la Derecha de la pieza?
        Vector3 derechaActual = fragmento.rotation * ejeLocalDerecha;
        derechaActual.y = 0; // Lo aplanamos contra la mesa
        if (derechaActual.sqrMagnitude > 0.001f) derechaActual.Normalize();

        // 3. Medimos el ángulo de giro en la mesa
        float anguloY = Vector3.SignedAngle(Vector3.right, derechaActual, Vector3.up);

        // 4. Detector de Panqueque (¿Lo dimos vuelta boca abajo?)
        Vector3 normalRelativa = diferenciaRotacion * Vector3.up;
        if (normalRelativa.y < 0)
        {
            anguloY = -anguloY; // Invertimos la lectura
        }

        // 5. Traducción a 2D (Por defecto la gravedad cae hacia abajo)
        Vector2 nuevaGravedad = Vector2.down;

        if (Mathf.Abs(anguloY) > margenGrados)
        {
            if (anguloY > margenGrados && anguloY <= 135f) nuevaGravedad = Vector2.left;
            else if (anguloY < -margenGrados && anguloY >= -135f) nuevaGravedad = Vector2.right;
            else if (Mathf.Abs(anguloY) > 135f) nuevaGravedad = Vector2.up;
        }

        // Pasamos el resultado a la función original que maneja las inversiones
        AplicarGravedad(nuevaGravedad, fragmento.name);
    }

    private void AplicarGravedad(Vector2 nuevaGravedad, string nombrePieza)
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

        if (vectorGravedad != nuevaGravedad)
        {
            Debug.Log($"<color=green>[Gravedad] Cambio de gravedad UV en {nombrePieza}! Dirección: {nuevaGravedad}</color>");
        }

        vectorGravedad = nuevaGravedad;
        vectorSalto = -nuevaGravedad;
        vectorDerecha = new Vector2(-vectorGravedad.y, vectorGravedad.x);
    }

    private void ManejarMovimiento()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        if (enEscalera && Mathf.Abs(inputVertical) > 0.1f) escalando = true;

        float velLateralActual = Vector2.Dot(rb.linearVelocity, vectorDerecha);
        float velCaidaActual = Vector2.Dot(rb.linearVelocity, vectorGravedad);

        if (escalando)
        {
            float nuevaVelSalto = inputVertical * velocidadEscalada;
            float velObjetivoX = inputHorizontal * velocidadMax;
            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, velObjetivoX, aceleracionAire * Time.fixedDeltaTime);
            rb.linearVelocity = (vectorDerecha * nuevaVelLateral) + (vectorSalto * nuevaVelSalto);
        }
        else
        {
            float targetSpeed = inputHorizontal * velocidadMax;
            float tasaDeCambio = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
            if (Mathf.Abs(inputHorizontal) < 0.01f) tasaDeCambio = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;

            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, targetSpeed, tasaDeCambio * Time.fixedDeltaTime);
            rb.linearVelocity = (vectorDerecha * nuevaVelLateral) + (vectorGravedad * velCaidaActual);
        }
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (estaEnSuelo || escalando))
        {
            escalando = false;
            float velLateralActual = Vector2.Dot(rb.linearVelocity, vectorDerecha);
            rb.linearVelocity = vectorDerecha * velLateralActual;
            rb.AddForce(vectorSalto * fuerzaSalto, ForceMode2D.Impulse);
            estaEnSuelo = false;
        }
    }

    private void ManejarAnimacionesVisuales()
    {
        float velLateral = Vector2.Dot(rb.linearVelocity, vectorDerecha);
        animator.SetFloat("Movement", Mathf.Abs(velLateral));
        animator.SetBool("ensuelo", estaEnSuelo);

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (inputHorizontal > 0.01f) spriteRenderer.flipX = false;
        else if (inputHorizontal < -0.01f) spriteRenderer.flipX = true;

        if (spriteRenderer != null)
        {
            if (vectorGravedad == Vector2.up) spriteRenderer.transform.localRotation = rotacionSpriteInicial * Quaternion.Euler(0f, 0f, 180f);
            else spriteRenderer.transform.localRotation = rotacionSpriteInicial;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contacto in collision.contacts)
        {
            if (Vector2.Dot(contacto.normal, vectorSalto) > 0.5f)
            {
                estaEnSuelo = true;
                return;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Escalera")) enEscalera = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Escalera")) { enEscalera = false; escalando = false; } }
}