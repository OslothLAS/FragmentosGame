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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.sleepThreshold = 0f;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        todosLosFragmentos = Object.FindObjectsByType<DetectorUV>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

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
                // NUEVA LÓGICA DE BLOQUEO:
                ArrastrarPiezaXZ scriptArrastre = fragmento.GetComponent<ArrastrarPiezaXZ>();
                if (scriptArrastre != null && scriptArrastre.EstaSiendoManipulada)
                {
                    // Si el jugador está moviendo esta pieza, no calculamos nueva gravedad.
                    // El personaje se mantiene pegado al piso con la dirección actual.
                    break;
                }

                // Solo recalcula cuando la pieza es soltada
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