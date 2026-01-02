using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary> Класс менеджера состояний персонажа (смотрите паттер конечных состояний (конченных) </summary>
public class PlayerStateManager : MonoBehaviour
{
    [Header("States")]
    public PlayerBaseState currentState; // текущее состояние
    public PlayerIdleState idleState = new PlayerIdleState(); // объект айдл состояния
    public PlayerMovingState movingState = new PlayerMovingState(); // объект состояния движения

    [Header("Movement parameters")]
    public Rigidbody2D rb2D; // риджидбади персонажа
    public Vector2 movementVector; // вектор движения персонажа
    public float speed = 1f; // скорость персонажа
    
    /// todo: полностью заменить на инишиалайз чтобы потом иметь полный контроль над инициализацией компонентов в сцене
    private void Start()
    {
        currentState = idleState;
        Initialize();
    }

    private void Update()
    {
        currentState.Update(this);
    }

    private void FixedUpdate()
    {
        currentState.FixedUpdate(this);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary> Метод инициализации параметров и зависимостей </summary>
    public void Initialize()
    {
        rb2D = GetComponent<Rigidbody2D>();
        SubscribeToPlayerEvents();
    }

    /// <summary> Метод подписки на события ивент менеджера </summary>
    private void SubscribeToPlayerEvents()
    {
        PlayerEventManager.OnInteract.AddListener(Interact);
        PlayerEventManager.OnMove.AddListener(Move);
    }
    
    /// <summary> Метод отмены подписки на события ивент менеджера </summary>
    private void UnsubscribeFromEvents()
    {
        PlayerEventManager.OnInteract?.RemoveListener(Interact);
        PlayerEventManager.OnMove?.RemoveListener(Move);
    }

    /// <summary> Метод смены состояния </summary>
    /// <param name="newState">Новое состояние</param>
    public void ChangeState(PlayerBaseState newState)
    {
        currentState.Exit(this);
        currentState = newState;
        newState.Enter(this);
    }
    
    /// <summary> Метод получения вектора движения, который слушает ивент OnMove (возможно лучше переименовать)</summary>
    /// <param name="input">Вектор движения</param>
    private void Move(Vector2 input)
    {
        movementVector = input.normalized;
    }

    /// <summary> Метод который срабатывает при ивенте OnInteract
    /// (Возможно он будет вызываться из контроллера персонажа если его будем делать) </summary>
    /// <param name="value">фактически флаг для галочки, при нажатии на кнопку всегда true</param>
    private void Interact(bool value)
    {
        
    }
}
