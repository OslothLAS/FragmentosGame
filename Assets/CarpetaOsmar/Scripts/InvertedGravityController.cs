using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class InvertedGravityController : MonoBehaviour
{
    [Header("Detección por UV")]
    public Camera camaraRender;
    private DetectorUV[] todosLosFragmentos;

    private Dictionary<DetectorUV, Quaternion> memoriaRotaciones = new Dictionary<DetectorUV, Quaternion>();

    [Header("Mecánica de Inclinación")]
    public float margenGrados = 20f;

    [Tooltip("Invierte la gravedad cuando cae hacia los costados (Izquierda pasa a ser Derecha y viceversa)")]
    public bool invertirGravedadX = false;

    [Tooltip("Invierte la gravedad cuando cae hacia adelante/atrás (El Piso pasa a ser Techo y viceversa)")]
    public bool invertirGravedadZ = false;

    [Header("Velocidad General")]
    public float velocidadMax = 8f;

    [Header("Suavizado en Suelo")]
    public float aceleracionSuelo = 60f;
    public float desaceleracionSuelo = 60f;

    [Header("Suavizado en Aire")]
    public float aceleracionAire = 15f;
    public float desaceleracionAire = 5f;

    [Header("Salto y Gravedad")]
    public float fuerzaSalto = 10f;
    public float fuerzaGravedad = 15f;
    public float velocidadEscalada = 5f;

    [Header("Animación y Visuales")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Vectores Dinámicos (Solo Lectura)")]
    public Vector3 vectorGravedad = Vector3.back;
    public Vector3 vectorSalto = Vector3.forward;
    public Vector3 vectorDerecha = Vector3.right;

    private Rigidbody rb;
    private bool estaEnSuelo;
    private bool enEscalera;
    private bool escalando;

    private Quaternion rotacionSpriteInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.sleepThreshold = 0f;

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            rotacionSpriteInicial = spriteRenderer.transform.localRotation;
        }

        todosLosFragmentos = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude);

        foreach (DetectorUV frag in todosLosFragmentos)
        {
            memoriaRotaciones.Add(frag, frag.transform.rotation);
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
            rb.AddForce(vectorGravedad * fuerzaGravedad, ForceMode.Acceleration);
        }

        ManejarMovimiento();
        estaEnSuelo = false;
    }

    private void DetectarGravedadPorUV()
    {
        if (camaraRender == null || todosLosFragmentos.Length == 0) return;

        Vector3 viewportPos = camaraRender.WorldToViewportPoint(transform.position);
        Vector2 uvDelPersonaje = new Vector2(viewportPos.x, viewportPos.y);

        foreach (DetectorUV fragmento in todosLosFragmentos)
        {
            if (fragmento.ContieneAlPersonaje(uvDelPersonaje))
            {
                ArrastrarPiezaXZ scriptArrastre = fragmento.GetComponent<ArrastrarPiezaXZ>();
                if (scriptArrastre != null && scriptArrastre.EstaSiendoManipulada)
                {
                    break;
                }

                CalcularNuevaGravedad(fragmento);
                break;
            }
        }
    }

    private void CalcularNuevaGravedad(DetectorUV fragmento)
    {
        Quaternion rotacionOriginal = memoriaRotaciones[fragmento];
        Quaternion rotacionActual = fragmento.transform.rotation;
        Quaternion diferenciaRotacion = rotacionActual * Quaternion.Inverse(rotacionOriginal);

        Vector3 direccionRelativa = diferenciaRotacion * Vector3.back;
        direccionRelativa.y = 0;
        direccionRelativa.Normalize();

        float anguloY = Vector3.SignedAngle(Vector3.back, direccionRelativa, Vector3.up);

        Vector3 nuevaGravedad = Vector3.back;

        if (Mathf.Abs(anguloY) > margenGrados)
        {
            if (anguloY > margenGrados && anguloY <= 135f)
            {
                nuevaGravedad = Vector3.left;
            }
            else if (anguloY < -margenGrados && anguloY >= -135f)
            {
                nuevaGravedad = Vector3.right;
            }
            else if (Mathf.Abs(anguloY) > 135f)
            {
                nuevaGravedad = Vector3.forward;
            }
        }

        if (invertirGravedadX)
        {
            if (nuevaGravedad == Vector3.left) nuevaGravedad = Vector3.right;
            else if (nuevaGravedad == Vector3.right) nuevaGravedad = Vector3.left;
        }

        if (invertirGravedadZ)
        {
            if (nuevaGravedad == Vector3.back) nuevaGravedad = Vector3.forward;
            else if (nuevaGravedad == Vector3.forward) nuevaGravedad = Vector3.back;
        }

        vectorGravedad = nuevaGravedad;
        vectorSalto = -nuevaGravedad;
        vectorDerecha = Vector3.Cross(vectorGravedad, Vector3.up).normalized;
    }

    private void ManejarMovimiento()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        if (enEscalera && Mathf.Abs(inputVertical) > 0.1f) escalando = true;

        float velLateralActual = Vector3.Dot(rb.linearVelocity, vectorDerecha);
        float velCaidaActual = Vector3.Dot(rb.linearVelocity, vectorGravedad);

        if (escalando)
        {
            float nuevaVelSalto = inputVertical * velocidadEscalada;
            float velObjetivoX = inputHorizontal * velocidadMax;
            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, velObjetivoX, aceleracionAire * Time.fixedDeltaTime);

            Vector3 nuevaVelocidad = (vectorDerecha * nuevaVelLateral) + (vectorSalto * nuevaVelSalto);
            nuevaVelocidad.y = 0f;
            rb.linearVelocity = nuevaVelocidad;
        }
        else
        {
            float targetSpeed = inputHorizontal * velocidadMax;
            float tasaDeCambio = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
            if (Mathf.Abs(inputHorizontal) < 0.01f) tasaDeCambio = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;

            float nuevaVelLateral = Mathf.MoveTowards(velLateralActual, targetSpeed, tasaDeCambio * Time.fixedDeltaTime);

            Vector3 nuevaVelocidad = (vectorDerecha * nuevaVelLateral) + (vectorGravedad * velCaidaActual);
            nuevaVelocidad.y = 0f;
            rb.linearVelocity = nuevaVelocidad;
        }
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (estaEnSuelo || escalando))
        {
            escalando = false;

            float velLateralActual = Vector3.Dot(rb.linearVelocity, vectorDerecha);
            Vector3 velocidadCorrigida = vectorDerecha * velLateralActual;
            velocidadCorrigida.y = 0f;
            rb.linearVelocity = velocidadCorrigida;

            rb.AddForce(vectorSalto * fuerzaSalto, ForceMode.Impulse);
            estaEnSuelo = false;
        }
    }

    private void ManejarAnimacionesVisuales()
    {
        float velLateral = Vector3.Dot(rb.linearVelocity, vectorDerecha);
        animator.SetFloat("Movement", Mathf.Abs(velLateral));
        animator.SetBool("ensuelo", estaEnSuelo);

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (inputHorizontal > 0.01f) spriteRenderer.flipX = false;
        else if (inputHorizontal < -0.01f) spriteRenderer.flipX = true;

        if (spriteRenderer != null)
        {
            // Solo rotamos 180° en Z si la gravedad es exactamente hacia arriba (Techo)
            if (vectorGravedad == Vector3.forward)
            {
                spriteRenderer.transform.localRotation = rotacionSpriteInicial * Quaternion.Euler(0f, 0f, 180f);
            }
            else
            {
                // En cualquier otro caso (piso normal o paredes), el sprite se mantiene derecho
                spriteRenderer.transform.localRotation = rotacionSpriteInicial;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            if (Vector3.Dot(contacto.normal, vectorSalto) > 0.5f)
            {
                estaEnSuelo = true;
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Escalera")) enEscalera = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Escalera")) { enEscalera = false; escalando = false; } }
}