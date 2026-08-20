using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerController2D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _jumpForce = 12f;

    private Rigidbody2D _rb;
    private float _horizontalInput;
    private bool _jumpRequested;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // Solo permite registrar el salto si está tocando el suelo
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _jumpRequested = true;
            _isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        // Movimiento horizontal
        _rb.linearVelocity = new Vector2(_horizontalInput * _moveSpeed, _rb.linearVelocity.y);

        // Salto
        if (_jumpRequested)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _jumpRequested = false;
        }
    }

    // Detecta cuando entra en contacto con cualquier superficie debajo de él
    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Verifica que el contacto sea desde abajo (suelo) y no contra una pared o techo
            if (contact.normal.y > 0.5f)
            {
                _isGrounded = true;
                return;
            }
        }
    }

    // Al salir del contacto con el objeto, se desactiva el estado de suelo
    private void OnCollisionExit2D(Collision2D collision)
    {
        _isGrounded = false;
    }
}