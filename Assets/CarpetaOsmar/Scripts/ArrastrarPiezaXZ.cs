using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))] // Agrega automáticamente el parlante
public class ArrastrarPiezaXZ : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float elevacionAlAgarrar = 0.5f;
    public float velocidadElevacion = 15f;
    public float velocidadRotacion = 5f;

    [Header("Efectos de Sonido")]
    [Tooltip("Arrastrá acá tus 7 sonidos de agarre")]
    public AudioClip[] sonidosGrab;
    [Tooltip("Arrastrá acá tus 5 sonidos de soltar")]
    public AudioClip[] sonidosDrop;
    [Range(0f, 1f)] public float volumenSonidos = 0.8f;

    [Header("Restricciones de Rotación")]
    public bool rotarEnX = true;
    public bool rotarEnY = true;
    public bool rotarEnZ = false;
    public bool EstaSiendoManipulada => siendoArrastrado || rotando;

    private Camera camaraPrincipal;
    private Vector3 offset;
    private Plane planoDeArrastre;
    private MeshCollider miCollider;
    private Rigidbody rb;
    private AudioSource audioSource; // Referencia al parlante

    private bool siendoArrastrado = false;
    private bool rotando = false;
    private Vector3 posicionObjetivo;
    private Quaternion rotacionObjetivo;

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal == null)
        {
            Debug.LogError("¡Falta el tag 'MainCamera' en la cámara de esta escena!");
        }

        miCollider = GetComponent<MeshCollider>();
        miCollider.convex = true;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Configuramos el AudioSource por código
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 1 = Sonido 3D (suena desde donde está la pieza)
    }

    // --- SISTEMA DE AUDIO ALEATORIO ---
    private void ReproducirSonidoAleatorio(AudioClip[] listaDeSonidos)
    {
        if (listaDeSonidos == null || listaDeSonidos.Length == 0) return;

        // Elegimos un número al azar entre 0 y la cantidad de sonidos en tu lista
        int indiceAleatorio = Random.Range(0, listaDeSonidos.Length);

        // PlayOneShot reproduce el sonido entero sin interrumpir otros sonidos
        audioSource.PlayOneShot(listaDeSonidos[indiceAleatorio], volumenSonidos);
    }

    void Update()
    {
        // Corrección aplicada: Solo si ESTA pieza estaba rotando
        if (rotando && Input.GetMouseButtonUp(1))
        {
            rotando = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (siendoArrastrado)
            {
                Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
                if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
                {
                    Vector3 puntoDeClic = rayo.GetPoint(distanciaImpacto);
                    offset = posicionObjetivo - puntoDeClic;
                    offset.y = 0;
                }
            }
            else
            {
                rb.isKinematic = false;
                ReproducirSonidoAleatorio(sonidosDrop); // <- DROP ACÁ
            }
        }

        if (siendoArrastrado && Input.GetMouseButtonDown(1))
        {
            rotando = true;
            rotacionObjetivo = rb.rotation;

            // Corrección aplicada: Locked en vez de Confined
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (rotando && camaraPrincipal != null)
        {
            float movX = Input.GetAxis("Mouse X") * velocidadRotacion;
            float movY = Input.GetAxis("Mouse Y") * velocidadRotacion;

            Quaternion giro = Quaternion.identity;

            if (rotarEnY) giro *= Quaternion.AngleAxis(-movX, Vector3.up);
            else if (rotarEnZ) giro *= Quaternion.AngleAxis(-movX, Vector3.forward);
            if (rotarEnX) giro *= Quaternion.AngleAxis(movY, Vector3.right);

            if (giro != Quaternion.identity)
            {
                Vector3 centroLocalEscalado = Vector3.Scale(miCollider.sharedMesh.bounds.center, transform.lossyScale);
                Vector3 centroMundoObjetivo = posicionObjetivo + (rotacionObjetivo * centroLocalEscalado);
                rotacionObjetivo = giro * rotacionObjetivo;
                posicionObjetivo = centroMundoObjetivo - (rotacionObjetivo * centroLocalEscalado);
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
            rb.MoveRotation(rotacionObjetivo);
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Input.GetMouseButton(0)) return;

            // Solo hacemos sonido si no la estábamos tocando ya
            if (!EstaSiendoManipulada)
            {
                ReproducirSonidoAleatorio(sonidosGrab); // <- GRAB ACÁ
            }

            rotando = true;
            rb.isKinematic = true;
            rotacionObjetivo = rb.rotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!siendoArrastrado)
            {
                posicionObjetivo = new Vector3(transform.position.x, transform.position.y + elevacionAlAgarrar, transform.position.z);
            }
        }
    }

    void OnMouseDown()
    {
        if (camaraPrincipal == null) return;

        // Solo hacemos sonido si no la estábamos tocando ya
        if (!EstaSiendoManipulada)
        {
            ReproducirSonidoAleatorio(sonidosGrab); // <- GRAB ACÁ
        }

        siendoArrastrado = true;
        rb.isKinematic = true;
        rotacionObjetivo = rb.rotation;

        float alturaObjetivoY = transform.position.y;
        if (!rotando) alturaObjetivoY += elevacionAlAgarrar;

        planoDeArrastre = new Plane(Vector3.up, new Vector3(0, alturaObjetivoY, 0));
        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
        {
            Vector3 puntoDeClic = rayo.GetPoint(distanciaImpacto);
            offset = transform.position - puntoDeClic;
            offset.y = 0;
        }

        if (!rotando)
        {
            posicionObjetivo = new Vector3(transform.position.x, alturaObjetivoY, transform.position.z);
        }
    }

    void OnMouseDrag()
    {
        if (!siendoArrastrado || rotando || camaraPrincipal == null) return;

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

        if (!rotando)
        {
            rb.isKinematic = false;
            ReproducirSonidoAleatorio(sonidosDrop); // <- DROP ACÁ
        }
    }

    void OnDisable()
    {
        if (rotando)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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