using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SombraSeguidora2D : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("El SpriteRenderer del personaje principal que vamos a copiar")]
    public SpriteRenderer spritePrincipal;

    [Header("Posicionamiento")]
    [Tooltip("Distancia relativa al personaje principal (ej: X=0.2, Y=-0.2)")]
    public Vector3 offset = new Vector3(0.5f, -0.5f, 0f);

    private SpriteRenderer miSpriteRenderer;

    void Start()
    {
        miSpriteRenderer = GetComponent<SpriteRenderer>();

        // Configuramos la sombra visualmente (negro semitransparente)
        // Puedes borrar esta línea si prefieres configurar el color desde el Inspector
        miSpriteRenderer.color = new Color(0f, 0f, 0f, 0.4f);
    }

    void LateUpdate()
    {
        if (spritePrincipal == null) return;

        // 1. Perseguir al personaje manteniendo la distancia del offset
        transform.position = spritePrincipal.transform.position + offset;

        // 2. Copiar la rotación exacta (útil si tu personaje invierte la gravedad)
        transform.rotation = spritePrincipal.transform.rotation;

        // 3. Clonar el frame de animación exacto y la dirección en la que mira
        miSpriteRenderer.sprite = spritePrincipal.sprite;
        miSpriteRenderer.flipX = spritePrincipal.flipX;
        miSpriteRenderer.flipY = spritePrincipal.flipY;

        // Opcional: Asegurar que la sombra se dibuje detrás del personaje
        miSpriteRenderer.sortingOrder = spritePrincipal.sortingOrder - 1;
    }
}