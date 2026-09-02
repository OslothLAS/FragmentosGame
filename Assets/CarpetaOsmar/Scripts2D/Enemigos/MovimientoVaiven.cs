using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoVaiven : MonoBehaviour
{
    public enum ModoActivacion { SiempreActivo, RequiereBoton }

    [Header("Condición de Movimiento")]
    [Tooltip("SiempreActivo ignora los botones. RequiereBoton lee el canal seleccionado.")]
    public ModoActivacion modo = ModoActivacion.SiempreActivo;

    [Tooltip("Si elegiste RequiereBoton, ¿qué canal debe estar activo?")]
    public BotonDePiso.CanalBoton canalRequerido = BotonDePiso.CanalBoton.Canal1;

    [Header("Dirección del Movimiento")]
    public bool moverHorizontal = true;
    public bool moverVertical = false;

    [Header("Ajustes de Velocidad y Distancia")]
    public float velocidad = 3f;
    public float distancia = 120f;

    [Header("Depuración (Solo Lectura)")]
    public bool estaActivo = false;

    public Vector2 VelocidadActual { get; private set; }

    private Vector2 posicionInicial;
    private Vector2 vectorDireccion;
    private Rigidbody2D rb;

    // Usamos un tiempo interno para evitar teletransportes si la plataforma se detiene y arranca
    private float tiempoInterno = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        posicionInicial = transform.position;
        vectorDireccion = Vector2.zero;

        if (moverHorizontal) vectorDireccion.x = 1f;
        if (moverVertical) vectorDireccion.y = 1f;
        vectorDireccion.Normalize();
    }

    void FixedUpdate()
    {
        // Actualiza la variable pública para que la veas en el Inspector
        estaActivo = (modo == ModoActivacion.SiempreActivo) || BotonDePiso.EstadoCanales[canalRequerido];

        if (!estaActivo)
        {
            VelocidadActual = Vector2.zero;
            return;
        }

        tiempoInterno += Time.fixedDeltaTime;

        float oscilacion = Mathf.Sin(tiempoInterno * velocidad) * distancia;
        Vector2 nuevaPos = posicionInicial + (vectorDireccion * oscilacion);

        VelocidadActual = (nuevaPos - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(nuevaPos);
    }
}