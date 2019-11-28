using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using Material = UnityEngine.Material;

public class ECSManager : MonoBehaviour
{
    public static ECSManager instance;

    public float playerMoveSpeed = 12, playerJumpForce = 4, playerMaxSpeed = 15;

    public float groundCheckDistance = 0.1f;

    public Mesh playerMesh;
    public float playerCapsuleRadius = 0.5f, playerCapsuleHeight = 2f;
    public Material playerMaterial;
    public float3 playerSpawnPos = new float3(1f, 0.5f, -1.8f);
    public float playerSize = 0.5f;
    
    public Mesh floorMesh;
    public Material floorMaterial;
    public float3 floorSpawnPos = new float3(0f, 0f, 0f);
    public float3 floorSize = new float3(10f, 2f, 10f);
    
    public unsafe Entity CreateBody(RenderMesh displayMesh, float3 position, quaternion orientation, 
        BlobAssetReference<Collider> collider,
        float3 linearVelocity, float3 angularVelocity, float mass, bool isDynamic)
    {
        EntityManager entityManager = World.Active.EntityManager;
        ComponentType[] componentTypes = new ComponentType[isDynamic ? 8 : 5];

        componentTypes[0] = typeof(RenderMesh);
        componentTypes[1] = typeof(TranslationProxy);
        componentTypes[2] = typeof(RotationProxy);
        componentTypes[3] = typeof(PhysicsCollider);
        componentTypes[4] = typeof(LocalToWorld);
        if (isDynamic)
        {
            componentTypes[5] = typeof(PhysicsVelocity);
            componentTypes[6] = typeof(PhysicsMass);
            componentTypes[7] = typeof(PhysicsDamping);
        }
        Entity entity = entityManager.CreateEntity(componentTypes);

        entityManager.SetSharedComponentData(entity, displayMesh);

        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.AddComponentData(entity, new Rotation { Value = orientation });
        entityManager.AddComponentData(entity, new Scale() { Value = playerSize});
        entityManager.SetComponentData(entity, new LocalToWorld() { Value = float4x4.TRS(position, orientation, playerSize) });
        entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });
        if (isDynamic)
        {
            Collider* colliderPtr = (Collider*)collider.GetUnsafePtr();
            entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));

            float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), angularVelocity);
            entityManager.SetComponentData(entity, new PhysicsVelocity()
            {
                Linear = linearVelocity,
                Angular = angularVelocityLocal
            });
            entityManager.SetComponentData(entity, new PhysicsDamping()
            {
                Linear = 0.01f,
                Angular = 0.05f
            });
        }

        return entity;
    }
    
    public Entity CreateDynamicCapsule(RenderMesh displayMesh, float radius, float3 position, quaternion orientation)
    {
        var filter = new CollisionFilter()
        {
            BelongsTo = 1u,
            CollidesWith = (uint)(1 << 1),
            GroupIndex = 1
        };
        var capsuleCollider = CapsuleCollider.Create(new CapsuleGeometry()
        {
            Radius = radius, Vertex0 = new float3(position.x, position.y + playerCapsuleHeight/2f, position.z),
            Vertex1 = new float3(position.x, position.y - playerCapsuleHeight / 2f, position.z)
        }, filter);
        return CreateBody(displayMesh, position, orientation, capsuleCollider, float3.zero, float3.zero, 1.0f, true);
    }

    
    
    public Entity CreateStaticBody(RenderMesh displayMesh, float3 position, quaternion orientation, 
        BlobAssetReference<Collider> collider,
        float3 linearVelocity, float3 angularVelocity, float mass)
    {
        EntityManager entityManager = World.Active.EntityManager;
        ComponentType[] componentTypes = new ComponentType[5];

        componentTypes[0] = typeof(RenderMesh);
        componentTypes[1] = typeof(TranslationProxy);
        componentTypes[2] = typeof(RotationProxy);
        componentTypes[3] = typeof(PhysicsCollider);
        componentTypes[4] = typeof(LocalToWorld);
        
        Entity entity = entityManager.CreateEntity(componentTypes);

        entityManager.SetSharedComponentData(entity, displayMesh);

        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.AddComponentData(entity, new Rotation { Value = orientation });
        entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });

        return entity;
    }
    
    public Entity CreateStaticBox(RenderMesh displayMesh, float3 position, quaternion orientation)
    {
        var filter = new CollisionFilter()
        {
            BelongsTo = (uint)(1 << 1),
            CollidesWith = 1u | (uint)(1<<2),
            GroupIndex = 1
        };
        var boxCollider = BoxCollider.Create(new BoxGeometry(){BevelRadius = 0f, Center = position, Orientation = quaternion.identity, Size = floorSize}, filter);
        return CreateStaticBody(displayMesh, position, orientation, boxCollider, float3.zero, float3.zero, 1.0f);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        
        var pMesh = new RenderMesh()
        {
            castShadows = ShadowCastingMode.On,
            layer = 1,
            material = playerMaterial,
            mesh = playerMesh,
            subMesh = 0
        };

        var playerEntity = CreateDynamicCapsule(pMesh, playerCapsuleRadius, playerSpawnPos, quaternion.identity);
        var entMan = World.Active.EntityManager;
        entMan.AddComponent(playerEntity, typeof(PurePlayerMovement));
        entMan.AddComponent(playerEntity, typeof(PurePlayerInput));
        
        var fMesh = new RenderMesh()
        {
            castShadows = ShadowCastingMode.On,
            layer = 1,
            material = floorMaterial,
            mesh = floorMesh,
            subMesh = 0
        };

        var floorEntity = CreateStaticBox(fMesh, floorSpawnPos, quaternion.identity);
    }
    
}
