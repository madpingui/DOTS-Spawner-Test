using Unity.Entities;
using Unity.Mathematics;

public struct MovementComponent : IComponentData
{
    public float3 Direction;
    public float Speed;
    public double NextDirectionChangeTime; // Time when the direction should change
}
