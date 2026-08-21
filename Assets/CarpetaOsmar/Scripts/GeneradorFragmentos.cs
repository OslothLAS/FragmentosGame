using System.Collections.Generic;
using UnityEngine;

public class GeneradorFragmentos : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto padre que contiene los 40 sprites adentro")]
    public Transform contenedorSprites;

    [Header("Configuración")]
    public int cantidadAleatoriaExtra = 12;

    void Start()
    {
        GenerarCristalesFiltrados();
    }

    private void GenerarCristalesFiltrados()
    {
        // 1. Listas para separar los sprites según su prioridad
        List<Transform> spritesImportantes = new List<Transform>();
        List<Transform> spritesNormales = new List<Transform>();

        // 2. Clasificamos todos los hijos del contenedor
        foreach (Transform sprite in contenedorSprites)
        {
            if (sprite.CompareTag("Importante"))
            {
                spritesImportantes.Add(sprite);
            }
            else
            {
                spritesNormales.Add(sprite);
            }
        }

        // 3. Instanciamos SÍ O SÍ en todos los que tienen el tag "Importante"
        foreach (Transform sprite in spritesImportantes)
        {
            InstanciarFragmentoEn(sprite);
        }

        // 4. Mezclamos la lista de sprites normales de forma aleatoria (Algoritmo Fisher-Yates)
        MezclarLista(spritesNormales);

        // 5. Tomamos solo la cantidad necesaria (protegiéndonos por si hay menos de 6 disponibles)
        int limite = Mathf.Min(cantidadAleatoriaExtra, spritesNormales.Count);
        for (int i = 0; i < limite; i++)
        {
            InstanciarFragmentoEn(spritesNormales[i]);
        }
    }

    // --- MÉTODOS AUXILIARES ---

    private void InstanciarFragmentoEn(Transform spriteTransform)
    {
        string nombreCoordenada = spriteTransform.name;
        GameObject prefab3D = Resources.Load<GameObject>("Fragmentos/" + nombreCoordenada);

        if (prefab3D != null)
        {
            Quaternion rotacionCorregida = Quaternion.Euler(90f, 0f, 0f);

            GameObject fragmentoInstanciado = Instantiate(prefab3D, spriteTransform.position, rotacionCorregida, spriteTransform);
            fragmentoInstanciado.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("Falta el prefab 3D para: " + nombreCoordenada);
        }
    }

    private void MezclarLista(List<Transform> lista)
    {
        // Desordena la lista de manera muy eficiente (O(n))
        for (int i = 0; i < lista.Count; i++)
        {
            Transform temp = lista[i];
            int randomIndex = Random.Range(i, lista.Count);
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }
    }
}