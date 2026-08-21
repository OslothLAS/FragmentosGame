using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DetectorCelda : MonoBehaviour
{
    private GameObject _hijoLeft;
    private GameObject _hijoRight;
    private BoxCollider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;

        Transform tLeft = transform.Find("Left");
        Transform tRight = transform.Find("Right");

        if (tLeft != null) _hijoLeft = tLeft.gameObject;
        if (tRight != null) _hijoRight = tRight.gameObject;
    }

    private void Start()
    {
        // 1. Por defecto arrancamos asumiendo que no hay nada
        gameObject.tag = "Vacio";

        // 2. Escaneamos si ya hay un objeto físico posicionado encima en el Editor
        Bounds misBounds = _collider.bounds;
        float areaPropia = misBounds.size.x * misBounds.size.y;

        Collider2D[] contactados = Physics2D.OverlapBoxAll(misBounds.center, misBounds.size, 0f);

        foreach (Collider2D col in contactados)
        {
            if (col.gameObject == gameObject) continue;

            // Verificamos si lo que está encima es un fragmento
            if (col.GetComponent<DraggableObject2D>() != null)
            {
                Bounds otroBounds = col.bounds;

                // Calculamos el área matemática de superposición exacta
                float xMin = Mathf.Max(misBounds.min.x, otroBounds.min.x);
                float xMax = Mathf.Min(misBounds.max.x, otroBounds.max.x);
                float yMin = Mathf.Max(misBounds.min.y, otroBounds.min.y);
                float yMax = Mathf.Min(misBounds.max.y, otroBounds.max.y);

                if (xMax > xMin && yMax > yMin)
                {
                    float areaInterseccion = (xMax - xMin) * (yMax - yMin);
                    float porcentaje = areaInterseccion / areaPropia;

                    // SOLO hereda el tag si el fragmento la cubre en al menos un 70%
                    if (porcentaje >= 0.7f)
                    {
                        gameObject.tag = col.tag;
                        Debug.Log($"[Inicio] La celda '{gameObject.name}' arrancó con un objeto encima ({porcentaje * 100}%). Tomó el tag: {gameObject.tag}");
                        break; // Ya encontramos nuestro objeto válido, no seguimos buscando
                    }
                }
            }
        }

        Invoke(nameof(ActualizarConexiones), 0.05f);
    }

    public void ActualizarConexiones()
    {
        if (_hijoLeft == null || _hijoRight == null) return;

        // REGLA: "Vacío tiene siempre Right y Left ACTIVADOS"
        if (gameObject.CompareTag("Vacio"))
        {
            _hijoLeft.SetActive(true);
            _hijoRight.SetActive(true);
            return;
        }

        // REGLA: "Cielo tiene siempre Right y Left DESACTIVADOS"
        if (gameObject.CompareTag("Cielo"))
        {
            _hijoLeft.SetActive(false);
            _hijoRight.SetActive(false);
            return;
        }

        // REGLAS PARA CUANDO ES "SUELO"
        if (gameObject.CompareTag("Suelo"))
        {
            if (int.TryParse(gameObject.name, out int miNumero))
            {
                GameObject celdaIzq = GameObject.Find((miNumero - 1).ToString("00"));
                GameObject celdaDer = GameObject.Find((miNumero + 1).ToString("00"));

                // --- Lógica del Lado DERECHO (Right) ---
                if (celdaDer != null && (celdaDer.CompareTag("Suelo") || celdaDer.CompareTag("Cielo")))
                {
                    _hijoRight.SetActive(false);
                }
                else
                {
                    _hijoRight.SetActive(true);
                }

                // --- Lógica del Lado IZQUIERDO (Left) ---
                if (celdaIzq != null && (celdaIzq.CompareTag("Suelo") || celdaIzq.CompareTag("Cielo")))
                {
                    _hijoLeft.SetActive(false);
                }
                else
                {
                    _hijoLeft.SetActive(true);
                }
            }
        }
    }

    public void AvisarVecinos()
    {
        if (int.TryParse(gameObject.name, out int miNumero))
        {
            GameObject celdaIzq = GameObject.Find((miNumero - 1).ToString("00"));
            GameObject celdaDer = GameObject.Find((miNumero + 1).ToString("00"));

            if (celdaIzq != null && celdaIzq.TryGetComponent<DetectorCelda>(out var scriptIzq))
                scriptIzq.ActualizarConexiones();

            if (celdaDer != null && celdaDer.TryGetComponent<DetectorCelda>(out var scriptDer))
                scriptDer.ActualizarConexiones();
        }
    }
}