using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class ArrastrarPiezaXZ : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float elevacionAlAgarrar = 0.5f;
    public float velocidadElevacion = 15f;
    public float velocidadRotacion = 5f;

    [Header("Efecto Visual (Shader)")]
    [ColorUsage(true, true)]
    public Color colorSeleccionVerde = new Color(0.2f, 1f, 0.2f, 50f);

    [ColorUsage(true, true)]
    public Color colorSeleccionRosa = new Color(1f, 0.2f, 0.7f, 50f);

    [Tooltip("El nombre exacto de la referencia en tu Shader (según tu imagen es ColorEmision)")]
    public string propiedadColorShader = "ColorEmision";

    [Header("Efectos de Sonido")]
    public AudioClip[] sonidosGrab;
    public AudioClip[] sonidosDrop;
    [Range(0f, 1f)] public float volumenSonidos = 0.8f;

    [Header("Restricciones de Rotación")]
    public bool rotarEnX = false;
    public bool rotarEnY = true;
    public bool rotarEnZ = true;
    public bool EstaSiendoManipulada => siendoArrastrado || rotando;

    private Camera camaraPrincipal;
    private Vector3 offset;
    private Plane planoDeArrastre;
    private MeshCollider miCollider;
    private Rigidbody rb;
    private AudioSource audioSource;

    private Material materialEdgeGlass;
    private Color colorOriginal;

    private bool siendoArrastrado = false;
    private bool rotando = false;
    private Vector3 posicionObjetivo;
    private Quaternion rotacionObjetivo;

    private int framesEsperaRecalculo = 0;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)] // Mantenemos el fix del cursor por si copiaste un script anterior
    public struct POINT { public int X; public int Y; }
    private POINT posicionFisicaDelMouse;
#endif

    private void GuardarPosicionMouse()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        GetCursorPos(out posicionFisicaDelMouse);
#endif
    }

    private void RestaurarPosicionMouse()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        SetCursorPos(posicionFisicaDelMouse.X, posicionFisicaDelMouse.Y);
#endif
    }

    void Start()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal == null) Debug.LogError("¡Falta el tag 'MainCamera' en la cámara!");

        miCollider = GetComponent<MeshCollider>();
        miCollider.convex = true;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.materials.Length > 0)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.name.Contains("EdgeGlass"))
                {
                    materialEdgeGlass = mat;
                    break;
                }
            }

            if (materialEdgeGlass == null)
            {
                materialEdgeGlass = rend.materials[0];
                Debug.LogWarning("No se encontró un material con el nombre 'EdgeGlass', usando el Element 0.");
            }

            if (!materialEdgeGlass.HasProperty(propiedadColorShader) && materialEdgeGlass.HasProperty("_" + propiedadColorShader))
            {
                propiedadColorShader = "_" + propiedadColorShader;
            }

            if (materialEdgeGlass.HasProperty(propiedadColorShader))
            {
                colorOriginal = materialEdgeGlass.GetColor(propiedadColorShader);
            }
        }
    }

    private void ActualizarEstadoVisual()
    {
        if (materialEdgeGlass == null || !materialEdgeGlass.HasProperty(propiedadColorShader)) return;

        materialEdgeGlass.EnableKeyword("_EMISSION");

        if (rotando)
        {
            materialEdgeGlass.SetColor(propiedadColorShader, colorSeleccionRosa);
        }
        else if (siendoArrastrado)
        {
            materialEdgeGlass.SetColor(propiedadColorShader, colorSeleccionVerde);
        }
        else
        {
            materialEdgeGlass.SetColor(propiedadColorShader, colorOriginal);
        }
    }

    private void ReproducirSonidoAleatorio(AudioClip[] listaDeSonidos)
    {
        if (listaDeSonidos == null || listaDeSonidos.Length == 0) return;
        int indiceAleatorio = Random.Range(0, listaDeSonidos.Length);
        audioSource.PlayOneShot(listaDeSonidos[indiceAleatorio], volumenSonidos);
    }

    void Update()
    {
        if (framesEsperaRecalculo > 0)
        {
            framesEsperaRecalculo--;
            if (framesEsperaRecalculo == 0 && siendoArrastrado)
            {
                Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
                if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
                {
                    Vector3 puntoDeClic = rayo.GetPoint(distanciaImpacto);
                    offset = posicionObjetivo - puntoDeClic;
                    offset.y = 0;
                }
            }
        }

        if (rotando && Input.GetMouseButtonUp(1))
        {
            rotando = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RestaurarPosicionMouse();

            if (siendoArrastrado)
            {
                framesEsperaRecalculo = 2;
            }
            else
            {
                rb.isKinematic = false;
                miCollider.isTrigger = false; // NUEVO: Volvemos a activar las colisiones
                ReproducirSonidoAleatorio(sonidosDrop);
            }

            ActualizarEstadoVisual();
        }

        if (siendoArrastrado && Input.GetMouseButtonDown(1))
        {
            GuardarPosicionMouse();
            rotando = true;
            rotacionObjetivo = rb.rotation;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ActualizarEstadoVisual();
        }

        if (rotando && camaraPrincipal != null)
        {
            float movX = Input.GetAxis("Mouse X") * velocidadRotacion;
            float movY = Input.GetAxis("Mouse Y") * velocidadRotacion;

            Quaternion giro = Quaternion.identity;

            float absX = Mathf.Abs(movX);
            float absY = Mathf.Abs(movY);

            if (absX > 0.01f || absY > 0.01f)
            {
                if (absX > absY)
                {
                    if (rotarEnY) giro *= Quaternion.AngleAxis(-movX, Vector3.up);
                }
                else
                {
                    if (rotarEnZ) giro *= Quaternion.AngleAxis(-movY, Vector3.forward);
                }
            }

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

            if (!EstaSiendoManipulada)
            {
                ReproducirSonidoAleatorio(sonidosGrab);
            }

            GuardarPosicionMouse();
            rotando = true;

            rb.isKinematic = true;
            miCollider.isTrigger = true; // NUEVO: Desactivamos las físicas al rotar

            rotacionObjetivo = rb.rotation;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!siendoArrastrado)
            {
                posicionObjetivo = new Vector3(transform.position.x, transform.position.y + elevacionAlAgarrar, transform.position.z);
            }

            ActualizarEstadoVisual();
        }
    }

    void OnMouseDown()
    {
        if (camaraPrincipal == null) return;

        if (!EstaSiendoManipulada)
        {
            ReproducirSonidoAleatorio(sonidosGrab);
        }

        siendoArrastrado = true;

        rb.isKinematic = true;
        miCollider.isTrigger = true; // NUEVO: Desactivamos colisiones físicas al agarrar

        rotacionObjetivo = rb.rotation;

        framesEsperaRecalculo = 0;

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

        ActualizarEstadoVisual();
    }

    void OnMouseDrag()
    {
        if (!siendoArrastrado || rotando || camaraPrincipal == null) return;

        if (framesEsperaRecalculo > 0) return;

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
            miCollider.isTrigger = false; // NUEVO: Volvemos a activar las colisiones al soltar
            ReproducirSonidoAleatorio(sonidosDrop);
        }

        ActualizarEstadoVisual();
    }

    void OnDisable()
    {
        rotando = false;
        siendoArrastrado = false;
        framesEsperaRecalculo = 0;
        ActualizarEstadoVisual();

        if (miCollider != null) miCollider.isTrigger = false; // NUEVO: Evita que quede como trigger si se apaga el objeto de golpe

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        RestaurarPosicionMouse();
#endif
    }
}