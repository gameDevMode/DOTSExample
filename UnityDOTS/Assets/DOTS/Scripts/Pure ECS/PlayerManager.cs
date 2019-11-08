using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager Instance;
    public float moveSpeed, jumpForce, maxPlayerSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        var entityMan = World.Active.EntityManager;
        var query = entityMan.CreateEntityQuery(typeof(PhysicsMass), typeof(PhysicsVelocity), typeof(PhysicsDamping));
        var playerEntity = query.GetSingletonEntity();
        entityMan.AddComponent(playerEntity, typeof(PurePlayerInput));
        entityMan.AddComponent(playerEntity, typeof(PurePlayerMovement));
    }

}
