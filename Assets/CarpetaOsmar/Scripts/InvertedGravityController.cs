using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InvertedGravityController : MonoBehaviour
{
    [Header("Velocidad General")]
    public float velocidadMax = 8f;

    [Header("Suavizado en Suelo")]
    public float aceleracionSuelo = 60f; // Responde muy rápido en el piso
    public float desaceleracionSuelo = 60f;

    [Header("Suavizado en Aire (Menos Control)")]
    public float aceleracionAire = 15f; // Tarda mucho más en acelerar al moverse en el aire
    public float desaceleracionAire = 5f; // Casi no frena si sueltas la tecla, conservando el impulso

    [Header("Salto y Gravedad")]
    public float fuerzaSalto = 10f;
    public float fuerzaGravedad = 15f;

    private Rigidbody rb;
    private bool estaEnSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    void Update()
    {
        ManejarSalto();
    }

    void FixedUpdate()
    {
        AplicarGravedadArtificial();
        ManejarMovimiento();

        estaEnSuelo = false;
    }

    private void ManejarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
            rb.AddForce(Vector3.forward * fuerzaSalto, ForceMode.Impulse);
            estaEnSuelo = false;
        }
    }

    private void ManejarMovimiento()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float velocidadObjetivoX = inputHorizontal * velocidadMax;

        float aceleracionActual = estaEnSuelo ? aceleracionSuelo : aceleracionAire;
        float desaceleracionActual = estaEnSuelo ? desaceleracionSuelo : desaceleracionAire;

        float tasaDeCambio = (Mathf.Abs(inputHorizontal) > 0.01f) ? aceleracionActual : desaceleracionActual;

        float nuevaVelocidadX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivoX, tasaDeCambio * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(nuevaVelocidadX, rb.linearVelocity.y, rb.linearVelocity.z);
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
}