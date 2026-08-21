using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ArrastrarPiezaXZ : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float elevacionAlAgarrar = 0.5f;
    public float velocidadElevacion = 15f;
    public float velocidadDesplazamiento = 10f; // Controla qué tan rápido patinan por el tablero

    private Camera camaraPrincipal;
    private float alturaFijaY;
    private float alturaObjetivoY;
    private Vector3 offset;
    private Plane planoDeArrastre;

    private BoxCollider miCollider;
    private Vector3 posicionGridOriginal;

    // --- NUEVO ESTADO ---
    private bool siendoArrastrado = false;

    void Start()
    {
        camaraPrincipal = Camera.main;
        miCollider = GetComponent<BoxCollider>();

        alturaFijaY = transform.position.y;
        alturaObjetivoY = alturaFijaY;

        posicionGridOriginal = new Vector3(transform.position.x, alturaFijaY, transform.position.z);
        planoDeArrastre = new Plane(Vector3.up, new Vector3(0, alturaFijaY, 0));
    }

    void Update()
    {
        // La interpolación en Y siempre se ejecuta para subir o bajar fluidamente
        float nuevaY = Mathf.Lerp(transform.position.y, alturaObjetivoY, Time.deltaTime * velocidadElevacion);

        // Si nadie está tocando la pieza, interpolamos X y Z hacia la casilla que le corresponde
        if (!siendoArrastrado)
        {
            float nuevaX = Mathf.Lerp(transform.position.x, posicionGridOriginal.x, Time.deltaTime * velocidadDesplazamiento);
            float nuevaZ = Mathf.Lerp(transform.position.z, posicionGridOriginal.z, Time.deltaTime * velocidadDesplazamiento);

            transform.position = new Vector3(nuevaX, nuevaY, nuevaZ);
        }
        else
        {
            // Si el mouse la tiene agarrada, la posición X y Z se actualiza en OnMouseDrag. 
            // Acá solo aplicamos la Y para que siga subiendo.
            transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
        }
    }

    void OnMouseDown()
    {
        // 1. Verificamos si estamos colisionando con el jugador ANTES de permitir el agarre
        if (EstaTocandoAlJugador())
        {
            return; // Cortamos la ejecución acá, no se puede agarrar
        }

        siendoArrastrado = true; // Tomamos el control manual

        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
        {
            Vector3 puntoDeClic = rayo.GetPoint(distanciaImpacto);
            offset = new Vector3(transform.position.x, alturaFijaY, transform.position.z) - puntoDeClic;
            alturaObjetivoY = alturaFijaY + elevacionAlAgarrar;
        }
    }

    void OnMouseDrag()
    {
        // 2. Si no la pudimos agarrar en OnMouseDown, ignoramos el arrastre
        if (!siendoArrastrado) return;

        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        if (planoDeArrastre.Raycast(rayo, out float distanciaImpacto))
        {
            Vector3 puntoActual = rayo.GetPoint(distanciaImpacto);
            transform.position = new Vector3(puntoActual.x + offset.x, transform.position.y, puntoActual.z + offset.z);
        }
    }

    void OnMouseUp()
    {
        // 3. Evitamos evaluar un intercambio si soltamos el clic sobre una pieza bloqueada
        if (!siendoArrastrado) return;

        siendoArrastrado = false; // Soltamos el control manual, el Update() entra en acción
        alturaObjetivoY = alturaFijaY;
        EvaluarIntercambio();
    }

    // --- NUEVA FUNCIÓN ---
    private bool EstaTocandoAlJugador()
    {
        Vector3 centro = miCollider.bounds.center;
        Vector3 tamanoMedio = miCollider.bounds.size / 2f;

        // Revisamos qué hay exactamente en el espacio que ocupa esta pieza
        Collider[] colisiones = Physics.OverlapBox(centro, tamanoMedio, transform.rotation);

        foreach (Collider col in colisiones)
        {
            // OJO con las mayúsculas: "player" no es lo mismo que "Player" en los tags de Unity
            if (col.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void EvaluarIntercambio()
    {
        Vector3 tamanoFisico = miCollider.bounds.size;
        float areaTotal = tamanoFisico.x * tamanoFisico.z;

        Vector3 centroEnMesa = new Vector3(transform.position.x, alturaFijaY, transform.position.z);
        Collider[] colisionesCercanas = Physics.OverlapBox(centroEnMesa, tamanoFisico / 2f, Quaternion.identity);

        ArrastrarPiezaXZ scriptPiezaDestino = null;
        float mayorPorcentaje = 0f;

        foreach (Collider col in colisionesCercanas)
        {
            if (col.gameObject == this.gameObject) continue;

            ArrastrarPiezaXZ otraPieza = col.GetComponent<ArrastrarPiezaXZ>();
            if (otraPieza != null)
            {
                float superposicionX = Mathf.Max(0, tamanoFisico.x - Mathf.Abs(transform.position.x - col.transform.position.x));
                float superposicionZ = Mathf.Max(0, tamanoFisico.z - Mathf.Abs(transform.position.z - col.transform.position.z));
                float areaInterseccion = superposicionX * superposicionZ;

                float porcentajeCoincidencia = areaInterseccion / areaTotal;

                if (porcentajeCoincidencia >= 0.6f && porcentajeCoincidencia > mayorPorcentaje)
                {
                    mayorPorcentaje = porcentajeCoincidencia;
                    scriptPiezaDestino = otraPieza;
                }
            }
        }

        if (scriptPiezaDestino != null)
        {
            Vector3 casillaObjetivo = scriptPiezaDestino.ObtenerCasillaOriginal();
            scriptPiezaDestino.ForzarNuevaCasilla(posicionGridOriginal);
            ForzarNuevaCasilla(casillaObjetivo);
        }
        else
        {
            ForzarNuevaCasilla(posicionGridOriginal);
        }
    }

    public Vector3 ObtenerCasillaOriginal()
    {
        return posicionGridOriginal;
    }

    public void ForzarNuevaCasilla(Vector3 nuevaPosicion)
    {
        posicionGridOriginal = nuevaPosicion;
    }
}