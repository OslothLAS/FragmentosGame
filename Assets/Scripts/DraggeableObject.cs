using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject2D : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [Tooltip("Velocidad máxima para evitar saltos bruscos que rompan la física")]
    [SerializeField] private float _maxSpeed = 40f;

    private Camera _mainCamera;
    private Rigidbody2D _rb;
    private Vector3 _offset;
    private float _zCoord;
    private bool _isDragging = false;
    private Vector2 _targetPosition;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnMouseDown()
    {
        _zCoord = _mainCamera.WorldToScreenPoint(transform.position).z;
        _offset = transform.position - GetMouseWorldPos();
        _isDragging = true;
        _targetPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        Vector3 mouseWorld = GetMouseWorldPos() + _offset;
        _targetPosition = new Vector2(mouseWorld.x, mouseWorld.y);
    }

    private void OnMouseUp()
    {
        _isDragging = false;

        // 1. Buscamos al objeto hijo que lleva el cuadrado
        Transform miCuadrado = transform.Find("CuadradoParentable");

        if (miCuadrado != null)
        {
            Collider2D miCollider = miCuadrado.GetComponent<Collider2D>();

            if (miCollider != null)
            {
                Bounds misBounds = miCollider.bounds;
                float areaPropia = misBounds.size.x * misBounds.size.y;

                // 2. Preparamos una lista y un filtro para ver qué está tocando
                List<Collider2D> contactados = new List<Collider2D>();
                ContactFilter2D filtro = new ContactFilter2D().NoFilter();

                int cantidad = miCollider.Overlap(filtro, contactados);

                // 3. Revisamos los objetos contactados y calculamos su superposición
                for (int i = 0; i < cantidad; i++)
                {
                    GameObject otroObjeto = contactados[i].gameObject;

                    // Evitamos evaluarnos a nosotros mismos si el collider nos detecta
                    if (otroObjeto == miCuadrado.gameObject) continue;

                    Bounds otroBounds = contactados[i].bounds;

                    float xMin = Mathf.Max(misBounds.min.x, otroBounds.min.x);
                    float xMax = Mathf.Min(misBounds.max.x, otroBounds.max.x);
                    float yMin = Mathf.Max(misBounds.min.y, otroBounds.min.y);
                    float yMax = Mathf.Min(misBounds.max.y, otroBounds.max.y);

                    if (xMax > xMin && yMax > yMin)
                    {
                        float areaInterseccion = (xMax - xMin) * (yMax - yMin);
                        float porcentaje = areaInterseccion / areaPropia;

                        // Si la superposición es del 70% (0.7f) o mayor
                        if (porcentaje >= 0.7f)
                        {
                            Debug.Log($"¡Superposición del {porcentaje * 100}% con el objeto: {otroObjeto.name}!");

                            // Acá podés disparar la lógica que necesites con el objeto detectado

                            break; // Salimos del loop para registrar el primero que cumpla
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isDragging)
        {
            // Movemos hacia el objetivo a una velocidad controlada sin saltos infinitos
            Vector2 newPos = Vector2.MoveTowards(_rb.position, _targetPosition, _maxSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = _zCoord;
        return _mainCamera.ScreenToWorldPoint(mousePoint);
    }
}