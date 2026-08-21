using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InvertedGravityController : MonoBehaviour
{
    [Header("Conexión con el Vidrio (Mesa)")]
    [Tooltip("La cámara que graba a este personaje (La que tiene el Render Texture)")]
    public Camera camaraRender;
    [Tooltip("El Quad base/invisible de la mesa que usaste como referencia para los UVs")]
    public Transform pantallaReferencia;

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

    [Header("Estado de Gravedad (Solo Lectura)")]
    public Vector3 vectorGravedad = Vector3.back;
    public Vector3 vectorSalto = Vector3.forward;

    private Rigidbody rb;
    private bool estaEnSuelo;
    private bool enEscalera;
    private bool escalando;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // --- BLOQUEO DE SEGURIDAD ---
        // Congelamos la rotación y también la posición en Y para que jamás flote o se caiga en profundidad 3D
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.sleepThreshold = 0f;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        ManejarSalto();
        ManejarAnimacionesVisuales();
    }

    void FixedUpdate()
    {
        // 1. Leemos el vidrio antes de movernos
        DetectarGravedadDelVidrio();

        if (!escalando)
        {
            AplicarGravedadArtificial();
        }

        ManejarMovimiento();
        estaEnSuelo = false;
    }

    private void DetectarGravedadDelVidrio()
    {
        if (camaraRender == null || pantallaReferencia == null) return;

        Vector3 viewportPos = camaraRender.WorldToViewportPoint(transform.position);
        Vector3 posicionRelativa = new Vector3(viewportPos.x - 0.5f, viewportPos.y - 0.5f, 0f);
        Vector3 posicionEnMesa = pantallaReferencia.TransformPoint(posicionRelativa);

        Vector3 origenRayo = posicionEnMesa + Vector3.up * 5f;
        if (Physics.Raycast(origenRayo, Vector3.down, out RaycastHit hit, 10f))
        {
            if (hit.collider.CompareTag("Fragmento"))
            {
                // Obtenemos la gravedad completa de la pieza
                Vector3 gravedadCruda = hit.transform.rotation * Vector3.back;

                // --- RESTRICCIÓN AL EJE Z ---
                // Anulamos X e Y. Solo nos quedamos con la dirección en Z.
                Vector3 nuevaGravedadZ = new Vector3(0f, 0f, gravedadCruda.z).normalized;

                // Si la nueva gravedad no es cero, la aplicamos
                if (nuevaGravedadZ.sqrMagnitude > 0.01f)
                {
                    CambiarDireccionGravedad(nuevaGravedadZ);
                }
            }
        }
    }

    public void CambiarDireccionGravedad(Vector3 nuevaGravedad)
    {
        vectorGravedad = nuevaGravedad;
        vectorSalto = -vectorGravedad;
    }

    private void ManejarAnimacionesVisuales()
    {
        animator.SetFloat("Movement", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("ensuelo", estaEnSuelo);

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        if (inputHorizontal > 0.01f) spriteRenderer.flipX = false;
        else if (inputHorizontal < -0.01f) spriteRenderer.flipX = true;
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (estaEnSuelo || escalando))
        {
            escalando = false;
            rb.linearVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, vectorSalto);
            rb.AddForce(vectorSalto * fuerzaSalto, ForceMode.Impulse);
            estaEnSuelo = false;
        }
    }

    private void ManejarMovimiento()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        if (enEscalera && Mathf.Abs(inputVertical) > 0.1f) escalando = true;

        if (escalando)
        {
            float nuevaVelocidadZ = inputVertical * velocidadEscalada;
            float velocidadObjetivoX = inputHorizontal * velocidadMax;
            float nuevaVelocidadX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivoX, aceleracionAire * Time.fixedDeltaTime);

            // Mantenemos Y en 0 de forma estricta
            rb.linearVelocity = new Vector3(nuevaVelocidadX, 0f, nuevaVelocidadZ);
        }
        else
        {
            float velocidadObjetivoX = inputHorizontal * velocidadMax;
            float aceleracionActual = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
            float desaceleracionActual = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;
            float tasaDeCambio = (Mathf.Abs(inputHorizontal) > 0.01f) ? aceleracionActual : desaceleracionActual;

            float nuevaVelocidadX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivoX, tasaDeCambio * Time.fixedDeltaTime);

            // Mantenemos Y en 0 de forma estricta, respetando el movimiento Z natural de la gravedad
            rb.linearVelocity = new Vector3(nuevaVelocidadX, 0f, rb.linearVelocity.z);
        }
    }

    private void AplicarGravedadArtificial()
    {
        rb.AddForce(vectorGravedad * fuerzaGravedad, ForceMode.Acceleration);
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