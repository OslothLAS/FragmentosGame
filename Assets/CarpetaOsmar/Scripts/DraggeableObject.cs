using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject2D : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [Tooltip("Velocidad máxima al arrastrar con el mouse")]
    [SerializeField] private float _maxSpeed = 40f;
    [Tooltip("Velocidad con la que el objeto vuelve a su lugar si falla la validación")]
    [SerializeField] private float _returnSpeed = 15f;

    [Header("Ajustes de Validación")]
    [Tooltip("Porcentaje requerido para validar (0.7 = 70%)")]
    [Range(0f, 1f)]
    [SerializeField] private float _porcentajeRequerido = 0.7f;

    [Header("Efectos Visuales al Agarrar")]
    [Tooltip("Cuánto se acerca a la cámara. En Unity 2D usar un número NEGATIVO (ej: -1).")]
    [SerializeField] private float _elevacionZ = -1f;

    // --- NUEVO: Control total de rotación en los 3 ejes ---
    [Tooltip("Ángulos de giro (X, Y, Z). Modificá X o Y para que se incline lateralmente dándole un efecto 3D.")]
    [SerializeField] private Vector3 _angulosGiro = new Vector3(15f, 0f, 10f);
    // ------------------------------------------------------

    [Tooltip("Velocidad con la que se eleva y rota (animación suave).")]
    [SerializeField] private float _velocidadEfectos = 15f;

    private Camera _mainCamera;
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private Vector3 _offset;
    private float _zCoord;
    private bool _isDragging = false;
    private bool _isReturning = false;
    private Vector2 _targetPosition;
    private Vector2 _posicionInicial;

    private GameObject _celdaOcupada;

    private float _zBase;
    private Quaternion _rotacionBase;

    private float _targetZ;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        _celdaOcupada = DetectarCeldaValida();

        // Memorizamos su estado "de reposo" una única vez al inicio
        _zBase = transform.position.z;
        _rotacionBase = transform.rotation;

        // Al arrancar, los objetivos son iguales a su estado de reposo
        _targetZ = _zBase;
        _targetRotation = _rotacionBase;
    }

    private void Update()
    {
        // Transición suave hacia los objetivos en Z y Rotación
        if (Mathf.Abs(transform.position.z - _targetZ) > 0.001f)
        {
            float smoothZ = Mathf.Lerp(transform.position.z, _targetZ, _velocidadEfectos * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, smoothZ);
        }

        if (Quaternion.Angle(transform.rotation, _targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, _velocidadEfectos * Time.deltaTime);
        }
    }

    private void OnMouseDown()
    {
        _posicionInicial = transform.position;
        _isReturning = false;

        GameObject celdaActual = DetectarCeldaValida();
        if (celdaActual != null)
        {
            _celdaOcupada = celdaActual;
        }

        // --- NUEVO: Aplicamos los ángulos X, Y y Z combinados ---
        _targetZ = _zBase + _elevacionZ;
        _targetRotation = _rotacionBase * Quaternion.Euler(_angulosGiro.x, _angulosGiro.y, _angulosGiro.z);
        // --------------------------------------------------------

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

        // Al soltar, los objetivos vuelven a ser los del estado de reposo
        _targetZ = _zBase;
        _targetRotation = _rotacionBase;

        GameObject nuevaCelda = DetectarCeldaValida();

        if (nuevaCelda != null)
        {
            Debug.Log($"¡ÉXITO! Pieza soltada en la celda: {nuevaCelda.name}");

            // 1. LIMPIEZA
            if (_celdaOcupada != null && _celdaOcupada != nuevaCelda)
            {
                _celdaOcupada.tag = "Vacio";

                if (_celdaOcupada.TryGetComponent<DetectorCelda>(out var scriptVieja))
                {
                    scriptVieja.ActualizarConexiones();
                    scriptVieja.AvisarVecinos();
                }
            }

            // 2. ACTUALIZACIÓN
            nuevaCelda.tag = gameObject.tag;

            if (nuevaCelda.TryGetComponent<DetectorCelda>(out var scriptNueva))
            {
                scriptNueva.ActualizarConexiones();
                scriptNueva.AvisarVecinos();
            }

            // 3. MEMORIZAR
            _celdaOcupada = nuevaCelda;
        }
        else
        {
            Debug.Log("No alcanzó el 70%. Volviendo lentamente a la posición inicial.");
            _isReturning = true;
        }
    }

    private void FixedUpdate()
    {
        if (_isDragging)
        {
            Vector2 newPos = Vector2.MoveTowards(_rb.position, _targetPosition, _maxSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);
        }
        else if (_isReturning)
        {
            Vector2 newPos = Vector2.MoveTowards(_rb.position, _posicionInicial, _returnSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            if (Vector2.Distance(_rb.position, _posicionInicial) < 0.001f)
            {
                _rb.position = _posicionInicial;
                _isReturning = false;
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = _zCoord;
        return _mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private GameObject DetectarCeldaValida()
    {
        if (_collider == null) return null;

        Bounds misBounds = _collider.bounds;
        float areaPropia = misBounds.size.x * misBounds.size.y;

        Collider2D[] contactados = Physics2D.OverlapBoxAll(misBounds.center, misBounds.size, 0f);

        foreach (Collider2D col in contactados)
        {
            if (col.gameObject == gameObject) continue;

            if (col.GetComponent<DetectorCelda>() != null)
            {
                Bounds otroBounds = col.bounds;

                float xMin = Mathf.Max(misBounds.min.x, otroBounds.min.x);
                float xMax = Mathf.Min(misBounds.max.x, otroBounds.max.x);
                float yMin = Mathf.Max(misBounds.min.y, otroBounds.min.y);
                float yMax = Mathf.Min(misBounds.max.y, otroBounds.max.y);

                if (xMax > xMin && yMax > yMin)
                {
                    float areaInterseccion = (xMax - xMin) * (yMax - yMin);
                    float porcentaje = areaInterseccion / areaPropia;

                    if (porcentaje >= _porcentajeRequerido)
                    {
                        return col.gameObject;
                    }
                }
            }
        }
        return null;
    }
}