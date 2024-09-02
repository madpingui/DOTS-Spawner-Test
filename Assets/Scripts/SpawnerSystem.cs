using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public partial struct OptimizedSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

        // Initialize the Random struct with a seed based on the elapsed time or any other unique value.
        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000);
        var random = new Random(seed);

        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb,
            Random = random

        }.ScheduleParallel();

        // Move the entities based on their movement component.
        new MoveEntitiesJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Random = random
        }.ScheduleParallel();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
public partial struct ProcessSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;
    public Random Random;

    // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    // This example queries for all Spawner components and uses `ref` to specify that the operation
    // requires read and write access. Unity processes `Execute` for each entity that matches the
    // component data query.
    private void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    {
        // If the next spawn time has passed.
        if (spawner.NextSpawnTime < ElapsedTime)
        {
            // Spawns a new entity and positions it at the spawner.
            Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
            Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));

            // Assign a random movement direction and speed using Unity.Mathematics.Random.
            float3 randomDirection = math.normalize(Random.NextFloat3(-1f, 1f));
            float randomSpeed = Random.NextFloat(1f, 5f);

            Ecb.AddComponent(chunkIndex, newEntity, new MovementComponent
            {
                Direction = randomDirection,
                Speed = randomSpeed,
                NextDirectionChangeTime = ElapsedTime
            });

            // Resets the next spawn time.
            spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
        }
    }
}

[BurstCompile]
public partial struct MoveEntitiesJob : IJobEntity
{
    public double ElapsedTime;
    public float DeltaTime;
    public Random Random;

    private void Execute(ref MovementComponent movement, ref LocalTransform transform)
    {
        // Check if it's time to change the direction
        if (ElapsedTime >= movement.NextDirectionChangeTime)
        {
            // Generate a new random direction
            movement.Direction = math.normalize(Random.NextFloat3(-1f, 1f));

            // Update the time for the next direction change
            movement.NextDirectionChangeTime = ElapsedTime + 1.0;
        }

        // Move the entity in the current direction
        transform.Position += movement.Direction * movement.Speed * DeltaTime;
    }
}