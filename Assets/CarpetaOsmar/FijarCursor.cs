using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class FijarCursor : MonoBehaviour
{
    IEnumerator Start()
    {
        // 1. Dejamos pasar el primer frame crítico donde el Input System lee (0,0)
        yield return new WaitForEndOfFrame();

        // 2. Si detecta un ratón, lo teletransporta forzosamente al centro de la ventana
        if (Mouse.current != null)
        {
            Vector2 centro = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Input System mueve el puntero de Windows
            Mouse.current.WarpCursorPosition(centro);

            // Unity ajusta su estado interno
            InputState.Change(Mouse.current.position, centro);
        }
    }
}