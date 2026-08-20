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
        // El script de arrastre ya no hace nada más aquí. 
        // Toda la lógica de grilla, encaje y swap la maneja el CuadradoParentable por su cuenta.
    }

    private void FixedUpdate()
    {
        if (_isDragging)
        {
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