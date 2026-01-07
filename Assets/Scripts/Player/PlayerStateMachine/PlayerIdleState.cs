using UnityEngine;

/// <summary> Класс айдл состояния персонажа </summary>
public class PlayerIdleState : PlayerBaseState
{
    public override void Enter(PlayerStateManager player)
    {
        //допустим сброс счётчика до начала анимации ожидания
        Debug.Log("Entered PlayerIdleState");
    }
    public override void Update(PlayerStateManager player)
    {
        //допустим тут мы включаем анимацию после того как прошло н секунд

        if (player.movementVector.magnitude > 0.1f)
        {
            player.ChangeState(player.movingState);
        }
    }
    public override void Exit(PlayerStateManager player)
    {
        
    }
}
