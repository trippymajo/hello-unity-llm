using UnityEngine;
using UnityEngine.Events;

public static class PlayerEventManager
{
    // 
    public static UnityEvent<Vector2> OnMove = new UnityEvent<Vector2>();
    public static UnityEvent<bool> OnInteract = new UnityEvent<bool>();
    public static UnityEvent<bool> OnChat = new UnityEvent<bool>();
}
