using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InvertedGravityController : MonoBehaviour
{
    [Header("Velocidad General")]
    public float velocidadMax = 8f;

    [Header("Suavizado en Suelo")]
    public float aceleracionSuelo = 60f;
    public float desaceleracionSuelo = 60f;

    [Header("Suavizado en Aire (Menos Control)")]
    public float aceleracionAire = 15f;
    public float desaceleracionAire = 5f;

    [Header("Salto y Gravedad")]
    public float fuerzaSalto = 10f;
    public float fuerzaGravedad = 15f;

    [Header("Escaleras")]
    public float velocidadEscalada = 5f;

    [Header("Animación y Visuales")]
    public Animator animator;
    public SpriteRenderer spriteRenderer; // Necesario para voltear el dibujo

    private Rigidbody rb;
    private bool estaEnSuelo;

    // --- ESTADOS DE ESCALERA ---
    private bool enEscalera;
    private bool escalando;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.sleepThreshold = 0f;

        // Autocompletar la referencia si te olvidas de arrastrarla en el inspector
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        ManejarSalto();
        ManejarAnimacionesVisuales();
    }

    void FixedUpdate()
    {
        if (!escalando)
        {
            AplicarGravedadArtificial();
        }

        ManejarMovimiento();

        estaEnSuelo = false;
    }

    private void ManejarAnimacionesVisuales()
    {
        // 1. CONTROL DE CORRER / QUIETO
        animator.SetFloat("Movement", Mathf.Abs(rb.linearVelocity.x));

        // 2. CONTROL DE SALTO / CAÍDA
        // Le enviamos el estado físico al Animator usando el nombre exacto que pediste
        animator.SetBool("ensuelo", estaEnSuelo);

        // 3. CONTROL DE LA DIRECCIÓN (Flip)
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        if (inputHorizontal > 0.01f)
        {
            spriteRenderer.flipX = false; // Mira a la derecha
        }
        else if (inputHorizontal < -0.01f)
        {
            spriteRenderer.flipX = true;  // Mira a la izquierda
        }
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (estaEnSuelo || escalando))
        {
            escalando = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
            rb.AddForce(Vector3.forward * fuerzaSalto, ForceMode.Impulse);
            estaEnSuelo = false;
        }
    }

    private void ManejarMovimiento()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        if (enEscalera && Mathf.Abs(inputVertical) > 0.1f)
        {
            escalando = true;
        }

        if (escalando)
        {
            float nuevaVelocidadZ = inputVertical * velocidadEscalada;

            float velocidadObjetivoX = inputHorizontal * velocidadMax;
            float nuevaVelocidadX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivoX, aceleracionAire * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(nuevaVelocidadX, rb.linearVelocity.y, nuevaVelocidadZ);
        }
        else
        {
            float velocidadObjetivoX = inputHorizontal * velocidadMax;
            float aceleracionActual = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
            float desaceleracionActual = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;
            float tasaDeCambio = (Mathf.Abs(inputHorizontal) > 0.01f) ? aceleracionActual : desaceleracionActual;

            float nuevaVelocidadX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivoX, tasaDeCambio * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(nuevaVelocidadX, rb.linearVelocity.y, rb.linearVelocity.z);
        }
    }

    private void AplicarGravedadArtificial()
    {
        rb.AddForce(Vector3.back * fuerzaGravedad, ForceMode.Acceleration);
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.z > 0.5f)
            {
                estaEnSuelo = true;
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Escalera"))
        {
            enEscalera = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Escalera"))
        {
            enEscalera = false;
            escalando = false;
        }
    }
}