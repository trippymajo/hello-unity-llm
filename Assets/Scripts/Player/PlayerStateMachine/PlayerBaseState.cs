using UnityEngine;

/// <summary>
/// Базовый абстрактный класс состояний который определяет, что у каждого состояния есть вход обновление и выход
/// </summary>
public abstract class PlayerBaseState
{
    /// <summary>Действия при начале состояния </summary>
    /// <param name="player">Менеджер состояний</param>
    public abstract void Enter(PlayerStateManager player);
    
    /// <summary> Действия при обновлении состояния </summary>
    /// <param name="player">Менеджер состояний</param>
    public abstract void Update(PlayerStateManager player);
    
    /// <summary>
    /// Действия при обновлении состояния связанного с физикой (виртуальный потому что дополнительный)
    /// остальные обязательны так как паттерн D:
    /// </summary>
    /// <param name="player">Менеджер состояний</param>
    public virtual void FixedUpdate(PlayerStateManager player) {}

    /// <summary> Действия при выходе из состояния </summary>
    /// <param name="player">Менеджер состояний</param>
    public abstract void Exit(PlayerStateManager player);

}
