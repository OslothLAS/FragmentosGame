using UnityEngine;

public class GravedadFragmento : MonoBehaviour
{
    // Este script asume que el estado ideal de "rompecabezas armado" es la rotación 0,0,0 (Quaternion.identity).
    // Es totalmente inmune a si las piezas se generan aleatoriamente, se caen o se tiran por el aire al inicio.

    public Vector2 CalcularGravedad(float margenGrados)
    {
        // Calculamos la rotación actual comparada contra el Norte absoluto del universo 3D
        Vector3 ejeLocalDerecha = Quaternion.Inverse(Quaternion.identity) * Vector3.right;
        Quaternion diferenciaRotacion = transform.rotation * Quaternion.Inverse(Quaternion.identity);

        Vector3 derechaActual = transform.rotation * ejeLocalDerecha;
        derechaActual.y = 0;

        if (derechaActual.sqrMagnitude > 0.001f)
        {
            derechaActual.Normalize();
        }

        float anguloY = Vector3.SignedAngle(Vector3.right, derechaActual, Vector3.up);
        Vector3 normalRelativa = diferenciaRotacion * Vector3.up;

        // Efecto Panqueque: Si la pieza está dada vuelta sobre la mesa, invertimos el ángulo
        if (normalRelativa.y < 0)
        {
            anguloY = -anguloY;
        }

        Vector2 gravedadSalida = Vector2.down; // Gravedad por defecto hacia abajo

        // Evaluamos en qué cuadrante cayó el ángulo de la pieza
        if (Mathf.Abs(anguloY) > margenGrados)
        {
            if (anguloY > margenGrados && anguloY <= 135f) gravedadSalida = Vector2.left;
            else if (anguloY < -margenGrados && anguloY >= -135f) gravedadSalida = Vector2.right;
            else if (Mathf.Abs(anguloY) > 135f) gravedadSalida = Vector2.up;
        }

        return gravedadSalida;
    }
}