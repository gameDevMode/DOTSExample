using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

public class PurePlayerMovementSystem : JobComponentSystem
{
    
    public struct PlayerMovementJob : IJobForEach<PurePlayerMovement, PurePlayerInput, PhysicsMass, PhysicsVelocity,
        Translation>
    {

        public bool jumpPressed;
        public float moveSpeed, jumpForce, maxSpeed, groundCheckDistance;

        private float3 _target;

        [ReadOnly]public CollisionWorld collisionWorld;
        [BurstCompile]
        private bool IsGrounded(Translation translation)
        {
            var filter = new CollisionFilter()
            {
                BelongsTo = (uint)(1 << 2),
                CollidesWith = (uint)(1 << 1),
                GroupIndex = 1
            };
            var from = translation.Value - new float3(0f, ECSManager.instance.playerCapsuleHeight / 2f, 0f);
            var to = translation.Value - new float3(0f, ECSManager.instance.playerCapsuleHeight / 2f + groundCheckDistance, 0f);
            var input = new RaycastInput()
            {
                End = to,
                Filter = filter,
                Start = from
            };

            var hit = collisionWorld.CastRay(input);
            Debug.Log(hit ? "<size=22>IsGrounded: <color=green>True</color></size>" : "<size=22>IsGrounded: <color=red>False</color></size>");
            return hit;
        }
        [BurstCompile]
        public void Execute(ref PurePlayerMovement movement, ref PurePlayerInput input, ref PhysicsMass mass, ref PhysicsVelocity vel,
            ref Translation translation)
        {
            movement.jumpForce = jumpForce;
            movement.moveSpeed = moveSpeed;
            mass.InverseInertia[0] = 0f;
            mass.InverseInertia[2] = 0f;

            _target.x = translation.Value.x + input.inputX;
            _target.y = 0f;
            _target.z = translation.Value.z + input.inputY;

            var v = math.normalizesafe(_target - translation.Value).xz * moveSpeed;

            var l = vel.Linear;
            l.x = v.x;
            l.z = v.y;

            if (math.SQRT2 * vel.Linear.x * vel.Linear.z < maxSpeed)
                vel.Linear = l;

            if (jumpPressed && IsGrounded(translation))
            {
                var impulse = new float3(0f, jumpForce, 0f);
                vel.ApplyLinearImpulse(mass, impulse);
            }

        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if(ECSManager.instance == null)
            throw new System.NotImplementedException();
        
        var playerMovementJob = new PlayerMovementJob()
        {
            jumpForce = ECSManager.instance.playerJumpForce,
            jumpPressed = Input.GetButtonDown("Jump"),
            moveSpeed = ECSManager.instance.playerMoveSpeed,
            maxSpeed = ECSManager.instance.playerMaxSpeed,
            groundCheckDistance = ECSManager.instance.groundCheckDistance,
            collisionWorld = World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld
        };

        var newDeps = JobHandle.CombineDependencies(
            World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().FinalJobHandle, inputDeps);
        return playerMovementJob.Schedule(this, newDeps);
    }
}
