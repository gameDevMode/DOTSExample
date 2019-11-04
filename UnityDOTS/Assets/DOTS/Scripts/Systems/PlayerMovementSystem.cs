using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(PlayerInputSystem))]
public class PlayerMovementSystem : ComponentSystem
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        float deltaTime = Time.deltaTime;
        Entities.ForEach((Rigidbody Rigidbody, PlayerInput PlayerInput, PlayerMovement PlayerMovement) =>
        {
            PlayerInput.InputX = Input.GetAxis("Horizontal");
            PlayerInput.InputY = Input.GetAxis("Vertical");
            if(Input.GetButtonDown("Jump"))
                Rigidbody.AddForce(0f, PlayerMovement.jumpForce, 0f, ForceMode.Impulse);
            float3 targetPos = Rigidbody.position +
                        new Vector3(PlayerInput.InputX * deltaTime * PlayerMovement.moveSpeed, 0f, PlayerInput.InputY * deltaTime * PlayerMovement.moveSpeed);
            Rigidbody.MovePosition(targetPos);
        });
    }
}
