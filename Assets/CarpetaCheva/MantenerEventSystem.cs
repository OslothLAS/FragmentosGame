using UnityEngine;

public class MantenerEventSystem : MonoBehaviour
{
    private static MantenerEventSystem instancia;

    void Awake()
    {
        // Si no hay ningún EventSystem guardado, guardamos este y lo hacemos inmortal
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Esto evita que muera al hacer LoadScene
        }
        else
        {
            // Si ya hay un EventSystem vivo de la escena anterior, destruimos el nuevo (para no tener clones)
            Destroy(gameObject);
        }
    }
}