using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class PurePlayerInputSystem : JobComponentSystem
{
    [BurstCompile]
    public struct PlayerInputJob : IJobForEach<PurePlayerInput>
    {
        public float InputX, InputY;
        public void Execute(ref PurePlayerInput input)
        {
            input.inputX = InputX;
            input.inputY = InputY;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var inputJob = new PlayerInputJob()
        {
            InputX = Input.GetAxis("Horizontal"),
            InputY = Input.GetAxis("Vertical")
        };
        return inputJob.Schedule(this, inputDeps);
    }
}
