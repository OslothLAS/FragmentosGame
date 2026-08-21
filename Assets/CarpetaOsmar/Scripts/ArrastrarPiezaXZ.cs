using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ArrastrarPiezaXZ : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float elevacionAlAgarrar = 0.5f;
    public float velocidadElevacion = 15f;
    public float velocidadRotacion = 10f;

    private Camera camaraPrincipal;
    private Vector3 offset;
    private Plane planoDeArrastre;
    private MeshCollider miCollider;
    private Rigidbody rb;

    private bool siendoArrastrado = false;
    private bool rotando = false;
    private Vector3 posicionObjetivo;

    // --- NUEVO: Referencia al jugador ---
    private InvertedGravityController jugadorAsociado;

    void Start()
    {
        camaraPrincipal = Camera.main;
        miCollider = GetComponent<MeshCollider>();
        miCollider.convex = true;
        rb = GetComponent<Rigidbody>();

        jugadorAsociado = Object.FindAnyObjectByType<InvertedGravityController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            rotando = false;
            if (!siendoArrastrado) rb.isKinematic = false;
        }

        if (rotando)
        {
            float movX = Input.GetAxis("Mouse X") * velocidadRotacion;
            float movY = Input.GetAxis("Mouse Y") * velocidadRotacion;

            transform.Rotate(Vector3.up, -movX, Space.World);
            transform.Rotate(Vector3.right, movY, Space.World);
        }

        // --- NUEVA MECÁNICA: Actualizar gravedad en tiempo real ---
        if (siendoArrastrado || rotando)
        {
            if (jugadorAsociado != null)
            {
                // Convertimos la dirección "Atrás" a la rotación actual del fragmento
                Vector3 nuevaGravedad = transform.rotation * Vector3.back;
                jugadorAsociado.CambiarDireccionGravedad(nuevaGravedad);
            }
        }
    }

    void FixedUpdate()
    {
        if (siendoArrastrado || rotando)
        {
            float nuevaY = Mathf.Lerp(rb.position.y, posicionObjetivo.y, Time.fixedDeltaTime * velocidadElevacion);
            Vector3 nuevaPosicion = new Vector3(posicionObjetivo.x, nuevaY, posicionObjetivo.z);
            rb.MovePosition(nuevaPosicion);
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (EstaTocandoAlJugador()) return;

            rotando = true;
            rb.isKinematic = true;

            if (!siendoArrastrado)
            {
                posicionObjetivo = new Vector3(transform.position.x, transform.position.y + elevacionAlAgarrar, transform.position.z);
            }
        }
    }

    void OnMouseDown()
    {
        if (EstaTocandoAlJugador()) return;

        siendoArrastrado = true;
        rb.isKinematic = true;

        float alturaObjetivoY = transform.position.y;
        if (!rotando) alturaObjetivoY += elevacionAlAgarrar;

        planoDeArrastre = new Plane(Vector3.up, new Vector3(0, alturaObjetivoY, 0));
        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
        {
            Vector3 puntoDeClic = rayo.GetPoint(distanciaImpacto);
            offset = new Vector3(transform.position.x - puntoDeClic.x, 0, transform.position.z - puntoDeClic.z);
        }

        if (!rotando)
        {
            posicionObjetivo = new Vector3(transform.position.x, alturaObjetivoY, transform.position.z);
        }
    }

    void OnMouseDrag()
    {
        if (!siendoArrastrado || rotando) return;

        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
        {
            Vector3 puntoActual = rayo.GetPoint(distanciaImpacto);
            posicionObjetivo.x = puntoActual.x + offset.x;
            posicionObjetivo.z = puntoActual.z + offset.z;
        }
    }

    void OnMouseUp()
    {
        if (!siendoArrastrado) return;
        siendoArrastrado = false;
        if (!rotando) rb.isKinematic = false;
    }

    private bool EstaTocandoAlJugador()
    {
        Vector3 centro = miCollider.bounds.center;
        Vector3 tamanoMedio = miCollider.bounds.size / 2f;
        Collider[] colisiones = Physics.OverlapBox(centro, tamanoMedio, transform.rotation);

        foreach (Collider col in colisiones)
        {
            if (col.CompareTag("Player")) return true;
        }
        return false;
    }
}