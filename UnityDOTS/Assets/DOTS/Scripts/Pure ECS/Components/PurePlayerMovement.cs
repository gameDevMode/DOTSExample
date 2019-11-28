using Unity.Entities;

public struct PurePlayerMovement : IComponentData
{
    public float moveSpeed, jumpForce, maxSpeed;
}
