using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CuadradoParentable : MonoBehaviour
{
    private BoxCollider2D _miCollider;
    private Vector3 _ultimaPosicion;
    private bool _fuiArrastrado;
    private Vector2 _posicionInicialCuadrado;

    private void Awake()
    {
        _miCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        AlinearInicioGrilla();
        _ultimaPosicion = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (transform.position != _ultimaPosicion)
            {
                if (!_fuiArrastrado)
                {
                    // Guardamos la posición exacta del cuadrado al empezar a arrastrar
                    _posicionInicialCuadrado = transform.position;
                    _fuiArrastrado = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_fuiArrastrado)
            {
                ProcesarAcomodo();
                _fuiArrastrado = false;
            }
        }

        _ultimaPosicion = transform.position;
    }

    private void AlinearInicioGrilla()
    {
        float ancho = _miCollider.bounds.size.x;
        float alto = _miCollider.bounds.size.y;

        Vector2 posActual = transform.position;
        float xAlineado = Mathf.Round(posActual.x / ancho) * ancho;
        float yAlineado = Mathf.Round(posActual.y / alto) * alto;

        MoverObjeto(transform, new Vector2(xAlineado, yAlineado));
    }

    private void ProcesarAcomodo()
    {
        float ancho = _miCollider.bounds.size.x;
        float alto = _miCollider.bounds.size.y;

        Vector2 tamañoBusqueda = new Vector2(ancho * 2.5f, alto * 2.5f);
        Collider2D[] contactados = Physics2D.OverlapBoxAll(_miCollider.bounds.center, tamañoBusqueda, 0f);

        BoxCollider2D otroCuadrado = null;
        float menorDistancia = float.MaxValue;

        foreach (Collider2D col in contactados)
        {
            if (col.gameObject != gameObject && col.name == "CuadradoParentable")
            {
                if (col is BoxCollider2D otroBox)
                {
                    float dist = Vector2.Distance(_miCollider.bounds.center, otroBox.bounds.center);
                    if (dist < menorDistancia)
                    {
                        menorDistancia = dist;
                        otroCuadrado = otroBox;
                    }
                }
            }
        }

        if (otroCuadrado != null)
        {
            EvaluarAccion(otroCuadrado, ancho, alto);
        }
        else
        {
            float xAlineado = Mathf.Round(transform.position.x / ancho) * ancho;
            float yAlineado = Mathf.Round(transform.position.y / alto) * alto;
            MoverObjeto(transform, new Vector2(xAlineado, yAlineado));
        }
    }

    private void EvaluarAccion(BoxCollider2D otroBox, float ancho, float alto)
    {
        Bounds misBounds = _miCollider.bounds;
        Bounds otrosBounds = otroBox.bounds;

        Transform otroTransform = otroBox.transform;
        float areaPropia = ancho * alto;

        // --- 1. EVALUAR SUPERPOSICIÓN DEL 60% PARA INTERCAMBIO ---
        float xMin = Mathf.Max(misBounds.min.x, otrosBounds.min.x);
        float xMax = Mathf.Min(misBounds.max.x, otrosBounds.max.x);
        float yMin = Mathf.Max(misBounds.min.y, otrosBounds.min.y);
        float yMax = Mathf.Min(misBounds.max.y, otrosBounds.max.y);

        if (xMax > xMin && yMax > yMin)
        {
            float areaInterseccion = (xMax - xMin) * (yMax - yMin);
            float porcentaje = areaInterseccion / areaPropia;

            if (porcentaje >= 0.60f)
            {
                Debug.Log("Superposición >= 60%: ¡INTERCAMBIO Y ALINEACIÓN A GRILLA DE LOS CUADRADOS!");

                // Alineamos estrictamente a la grilla usando la posición del cuadrado contrincante y la nuestra inicial
                Vector2 destinoOtro = new Vector2(
                    Mathf.Round(_posicionInicialCuadrado.x / ancho) * ancho,
                    Mathf.Round(_posicionInicialCuadrado.y / alto) * alto
                );

                Vector2 destinoMio = new Vector2(
                    Mathf.Round(otroTransform.position.x / ancho) * ancho,
                    Mathf.Round(otroTransform.position.y / alto) * alto
                );

                MoverObjeto(otroTransform, destinoOtro);
                MoverObjeto(transform, destinoMio);
                return;
            }
        }

        // --- 2. ACOPLE EN GRILLA (SI NO HUBO INTERCAMBIO) ---
        Vector2 direccion = misBounds.center - otrosBounds.center;
        int celdasX = Mathf.RoundToInt(direccion.x / ancho);
        int celdasY = Mathf.RoundToInt(direccion.y / alto);

        if (celdasX == 0 && celdasY == 0)
        {
            if (Mathf.Abs(direccion.x) > Mathf.Abs(direccion.y))
                celdasX = (direccion.x > 0) ? 1 : -1;
            else
                celdasY = (direccion.y > 0) ? 1 : -1;
        }

        Vector2 centroIdeal = (Vector2)otrosBounds.center + new Vector2(celdasX * ancho, celdasY * alto);

        Collider2D[] obstaculos = Physics2D.OverlapBoxAll(centroIdeal, new Vector2(ancho * 0.8f, alto * 0.8f), 0f);
        foreach (Collider2D obs in obstaculos)
        {
            if (obs.gameObject != gameObject && obs.gameObject != otroBox.gameObject && obs.name == "CuadradoParentable")
            {
                MoverObjeto(transform, new Vector2(
                    Mathf.Round(_posicionInicialCuadrado.x / ancho) * ancho,
                    Mathf.Round(_posicionInicialCuadrado.y / alto) * alto
                ));
                return;
            }
        }

        Vector2 desplazamiento = centroIdeal - (Vector2)misBounds.center;
        MoverObjeto(transform, (Vector2)transform.position + desplazamiento);
    }

    private void MoverObjeto(Transform t, Vector2 destino)
    {
        if (t.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.position = destino;
        }
        t.position = new Vector3(destino.x, destino.y, t.position.z);
    }
}