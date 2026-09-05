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
    [Tooltip("Si la inclinación es menor a este ángulo, la gravedad apuntará directo abajo. Puedes bajarlo a 0.")]
    public float margenGrados = 2f;
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
    [Tooltip("Qué tan rápido se acomoda el personaje al ángulo del suelo (Rotación progresiva)")]
    public float velocidadRotacionSprite = 10f;

    [Header("Audio")] // <--- NUEVA SECCIÓN DE AUDIO
    public AudioSource sourceCorrer;
    public AudioSource sourceSalto;

    [Header("Vectores Dinámicos")]
    public Vector2 vectorGravedad = Vector2.down;
    public Vector2 vectorSalto = Vector2.up;
    public Vector2 vectorDerecha = Vector2.right;

    private Rigidbody2D rb;
    private bool estaEnSuelo;
    private bool enEscalera;
    private bool escalando;
    private Quaternion rotacionSpriteInicial;

    // Variables de herencia de velocidad para plataformas móviles
    private Vector2 velocidadPlataforma = Vector2.zero;

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
        foreach (DetectorUV detector in todosLosDetectores)
        {
            memoriaRotaciones.Add(detector.transform, detector.transform.rotation);
        }
    }

    void Update()
    {
        ManejarSalto();
        ManejarAnimacionesVisuales();
        ManejarAudio(); // <--- LLAMAMOS A LA LÓGICA DE AUDIO
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

        AplicarGravedad(nuevaGravedad, fragmento.name);
    }

    private void AplicarGravedad(Vector2 nuevaGravedad, string nombrePieza)
    {
        if (invertirGravedadX) nuevaGravedad.x = -nuevaGravedad.x;
        if (invertirGravedadY) nuevaGravedad.y = -nuevaGravedad.y;

        if (Vector2.Distance(vectorGravedad, nuevaGravedad) > 0.01f)
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

        Vector2 velocidadRelativa = rb.linearVelocity - velocidadPlataforma;

        float velLateralActual = Vector2.Dot(velocidadRelativa, vectorDerecha);
        float velCaidaActual = Vector2.Dot(velocidadRelativa, vectorGravedad);

        if (escalando)
        {
            float nuevaVelSalto = inputVertical * velocidadEscalada;
            float velObjetivoX = inputHorizontal * velocidadMax;
            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, velObjetivoX, aceleracionAire * Time.fixedDeltaTime);

            rb.linearVelocity = (vectorDerecha * nuevaVelLateral) + (vectorSalto * nuevaVelSalto) + velocidadPlataforma;
        }
        else
        {
            float targetSpeed = inputHorizontal * velocidadMax;
            float tasaDeCambio = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
            if (Mathf.Abs(inputHorizontal) < 0.01f) tasaDeCambio = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;

            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, targetSpeed, tasaDeCambio * Time.fixedDeltaTime);

            rb.linearVelocity = (vectorDerecha * nuevaVelLateral) + (vectorGravedad * velCaidaActual) + velocidadPlataforma;
        }
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (estaEnSuelo || escalando))
        {
            escalando = false;

            Vector2 velocidadRelativa = rb.linearVelocity - velocidadPlataforma;
            float velLateralActual = Vector2.Dot(velocidadRelativa, vectorDerecha);

            rb.linearVelocity = (vectorDerecha * velLateralActual) + velocidadPlataforma;
            rb.AddForce(vectorSalto * fuerzaSalto, ForceMode2D.Impulse);

            estaEnSuelo = false;
            velocidadPlataforma = Vector2.zero;

            // <--- REPRODUCIR SONIDO DE SALTO
            if (sourceSalto != null)
            {
                sourceSalto.Play();
            }
        }
    }

    private void ManejarAnimacionesVisuales()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Movement", Mathf.Abs(inputHorizontal));
        animator.SetBool("ensuelo", estaEnSuelo);

        if (inputHorizontal > 0.01f) spriteRenderer.flipX = false;
        else if (inputHorizontal < -0.01f) spriteRenderer.flipX = true;

        if (spriteRenderer != null)
        {
            float anguloZ = Mathf.Atan2(vectorSalto.y, vectorSalto.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotacionObjetivo = rotacionSpriteInicial * Quaternion.Euler(0f, 0f, anguloZ);

            spriteRenderer.transform.localRotation = Quaternion.Slerp(
                spriteRenderer.transform.localRotation,
                rotacionObjetivo,
                velocidadRotacionSprite * Time.deltaTime
            );
        }
    }

    // <--- NUEVO MÉTODO PARA GESTIONAR EL AUDIO AL CORRER
    private void ManejarAudio()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        if (sourceCorrer != null)
        {
            // Solo debe sonar si está tocando el suelo, no está escalando y está apretando A o D
            bool intentandoCorrer = estaEnSuelo && !escalando && Mathf.Abs(inputHorizontal) > 0.1f;

            if (intentandoCorrer)
            {
                // Si cumple las condiciones y NO estaba sonando, lo reproducimos
                if (!sourceCorrer.isPlaying)
                {
                    sourceCorrer.Play();
                }
            }
            else
            {
                // Si soltó la tecla o saltó y estaba sonando, lo frenamos
                if (sourceCorrer.isPlaying)
                {
                    sourceCorrer.Stop();
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contacto in collision.contacts)
        {
            if (Vector2.Dot(contacto.normal, vectorSalto) > 0.5f)
            {
                estaEnSuelo = true;

                MovimientoVaiven plataforma = collision.gameObject.GetComponent<MovimientoVaiven>();
                if (plataforma != null)
                {
                    velocidadPlataforma = plataforma.VelocidadActual;
                }
                else
                {
                    velocidadPlataforma = Vector2.zero;
                }
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<MovimientoVaiven>() != null)
        {
            velocidadPlataforma = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Escalera")) enEscalera = true; }
    private void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Escalera")) { enEscalera = false; escalando = false; } }
}