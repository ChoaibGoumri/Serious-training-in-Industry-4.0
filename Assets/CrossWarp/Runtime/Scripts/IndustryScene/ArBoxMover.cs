using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]  
public class ArBoxMover : NetworkBehaviour
{
    private enum BoxState { Idle, MovingToVR, MovingFromVR, Done }

    [Networked] private BoxState CurrentState { get; set; }
    [Networked] private Vector3 net_destination { get; set; }
    [Networked] private Vector3 net_moveDirection { get; set; }
    [Networked] private float net_moveSpeed { get; set; }
    [Networked] private TickTimer net_despawnTimer { get; set; }
    [Networked] private NetworkBool net_isInitialized { get; set; }
    [Networked] private MasterFlowController flowController { get; set; }
    
    private Rigidbody rb;
    private NetworkTransform netTransform;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        netTransform = GetComponent<NetworkTransform>();
        
        
        rb.useGravity = false;
        rb.isKinematic = false; 
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;  
        
        CurrentState = BoxState.Idle;  
        
        Debug.Log($"ArBoxMover Spawned - HasStateAuthority: {HasStateAuthority}");
    }

    public void Server_Init_ToVR(Vector3 destination, float speed, MasterFlowController controller)
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning("Server_Init_ToVR chiamato su un client!");
            return;
        }

        net_destination = destination;
        net_moveSpeed = speed;
        flowController = controller;
        net_isInitialized = true;
        CurrentState = BoxState.MovingToVR;
        
        Debug.Log($"Box inizializzato ToVR - Pos: {transform.position} -> Dest: {destination}");
    }

    public void Server_Init_FromVR(Vector3 moveDirection, float speed, float duration, MasterFlowController controller)
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning("Server_Init_FromVR chiamato su un client!");
            return;
        }

        net_moveDirection = moveDirection.normalized;  
        net_moveSpeed = speed;
        flowController = controller;
        net_despawnTimer = TickTimer.CreateFromSeconds(Runner, duration);
        net_isInitialized = true;
        CurrentState = BoxState.MovingFromVR;
        
        Debug.Log($"Box inizializzato FromVR - Dir: {moveDirection}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !net_isInitialized) return;

        switch (CurrentState)
        {
            case BoxState.MovingToVR:
                HandleMovingToVR();
                break;
            case BoxState.MovingFromVR:
                HandleMovingFromVR();
                break;
        }
    }

    private void HandleMovingToVR()
    {
        float distanceToTarget = Vector3.Distance(transform.position, net_destination);
        
        if (distanceToTarget < 0.1f)
        {
            CurrentState = BoxState.Done;
            
            if (flowController != null)
            {
                flowController.RPC_RequestVRSpawn();
                flowController.RPC_PlayDespawnEffect(transform.position);
            }
            
            Debug.Log("Box arrivato a destinazione VR - Despawn");
            Runner.Despawn(Object);
        }
        else
        {
            Vector3 direction = (net_destination - transform.position).normalized;
            Vector3 movement = direction * net_moveSpeed * Runner.DeltaTime;
            
            
            rb.velocity = direction * net_moveSpeed;
            
          
        }
    }

    private void HandleMovingFromVR()
    {
        if (net_despawnTimer.Expired(Runner))
        {
            CurrentState = BoxState.Done;
            Debug.Log("Timer box di ritorno scaduto - Despawn");
            Runner.Despawn(Object);
        }
        else
        {
            
            rb.velocity = net_moveDirection * net_moveSpeed;
            
        }
    }
}