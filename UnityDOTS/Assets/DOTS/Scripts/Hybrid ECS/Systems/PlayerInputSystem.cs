using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public class PlayerInputSystem : ComponentSystem
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        Entities.ForEach((PlayerInput PlayerInput) =>
        {
            PlayerInput.InputX = Input.GetAxis("Horizontal");
            PlayerInput.InputY = Input.GetAxis("Vertical");
        });
    }
}
