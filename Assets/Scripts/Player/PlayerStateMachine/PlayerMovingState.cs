using UnityEngine;

/// <summary> Класс состояния персонажа при движении </summary>
public class PlayerMovingState : PlayerBaseState
{
    public override void Enter(PlayerStateManager player)
    {
        // запуск анимации ходьбы
        Debug.Log("Entered PlayerMovingState");
    }

    public override void Update(PlayerStateManager player)
    {
        
    }

    public override void FixedUpdate(PlayerStateManager player)
    {
        if (player.movementVector.magnitude < 0.1f)
        {
            player.ChangeState(player.idleState);
        }
        
        player.rb2D.MovePosition(player.rb2D.position + player.movementVector * (player.speed * Time.fixedDeltaTime));
    }

    public override void Exit(PlayerStateManager player)
    {
        
    }
}
