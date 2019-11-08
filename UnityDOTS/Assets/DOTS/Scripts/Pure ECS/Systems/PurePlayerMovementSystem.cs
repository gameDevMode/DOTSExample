using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.Animations;
using Material = UnityEngine.Material;
using Math = System.Math;

[UpdateAfter(typeof(PurePlayerInputSystem))]
public class PurePlayerMovementSystem : JobComponentSystem
{
    [BurstCompile]
    public struct PlayerMovementJob : IJobForEach<PurePlayerInput, PurePlayerMovement, PhysicsVelocity, PhysicsMass, Translation, Rotation>
    {
        public float deltaTime;
        public bool jumpPressed;
        public float moveSpeed, jumpForce, maxPlayerSpeed;


        private bool isJumping, changeOfDirection;
        private float prevInputX, prevInputY;
        public void Execute(ref PurePlayerInput input, ref PurePlayerMovement movement, ref PhysicsVelocity vel, ref PhysicsMass mass,
            ref Translation translation, ref Rotation rotation)
        {
            movement.jumpForce = jumpForce;
            movement.moveSpeed = moveSpeed;
            rotation.Value = new quaternion(0f, rotation.Value.value.y, 0f, rotation.Value.value.w);
            if (jumpPressed && !isJumping)
            {
                vel.Linear = float3.zero;
                vel.Angular = float3.zero;
                var impulse = new float3(0f, jumpForce, 0f);
                vel.ApplyImpulse(mass, translation, rotation, impulse, translation.Value);
            }

            if (Math.Abs(vel.Linear.y) > 0.1f)
                isJumping = true;
            else
                isJumping = false;

            if (input.InputX != 0 || input.InputY != 0)
            {
                //Player Input Given
                changeOfDirection = false;
                if ((input.InputX > 0 && prevInputX < 0) || (input.InputX < 0 && prevInputX > 0))
                    changeOfDirection = true;
                if ((input.InputY > 0 && prevInputY < 0) || (input.InputY < 0 && prevInputY > 0))
                    changeOfDirection = true;
                if(changeOfDirection)
                    vel.Linear = float3.zero;
                
                vel.Angular = float3.zero;
                if(Vector3.SqrMagnitude(new Vector3(vel.Angular.x + vel.Linear.x, vel.Angular.y + vel.Linear.y, vel.Angular.z + vel.Linear.z )) < maxPlayerSpeed * maxPlayerSpeed)
                    vel.Linear += new float3(input.InputX * deltaTime * moveSpeed, 0f, input.InputY * deltaTime * moveSpeed);

                prevInputX = input.InputX;
                prevInputY = input.InputY;
            }
            else
            {
                if(isJumping) return;
                vel.Linear = math.lerp(vel.Linear, float3.zero, deltaTime * 20f);
                vel.Angular = math.lerp(vel.Angular, float3.zero, deltaTime * 20f);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var playerMovementJob = new PlayerMovementJob
        {
            deltaTime = Time.deltaTime,
            jumpPressed = Input.GetButtonDown("Jump"),
            moveSpeed = PlayerManager.Instance.moveSpeed,
            jumpForce = PlayerManager.Instance.jumpForce,
            maxPlayerSpeed = PlayerManager.Instance.maxPlayerSpeed
        };

        return playerMovementJob.Schedule(this, inputDeps);
    }
}
