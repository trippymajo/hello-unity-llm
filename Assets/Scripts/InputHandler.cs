using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> Класс обработки инпутов (смотри новую систему инпутов в юнити, удобно п**дец</summary>
public class InputHandler : MonoBehaviour
{
    /// <summary> Обработчик клавиш движения (смотри Move в файле InputSystemActions)</summary>
    /// <param name="context">Контекст обработки клавиш</param>
    public void OnMove(InputValue context)
    {
        PlayerEventManager.OnMove?.Invoke(context.Get<Vector2>());
    }
    /// <summary> Обработчик клавиши взаимодействия (смотри Interact в файле InputSystemActions)</summary>
    /// <param name="context">Контекст обработки клавиш</param>
    public void OnInteract(InputValue context)
    {
        PlayerEventManager.OnInteract?.Invoke(true);
    }
}
