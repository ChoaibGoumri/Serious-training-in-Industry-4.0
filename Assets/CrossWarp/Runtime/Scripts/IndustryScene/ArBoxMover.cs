using UnityEngine;
using Fusion;

// RIMOSSO: [RequireComponent(typeof(Rigidbody))]
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
    
    // RIMOSSO: private Rigidbody rb;

    public override void Spawned()
    {
        // RIMOSSO: configurazione Rigidbody
        CurrentState = BoxState.Idle; 
    }

    // --- I TUOI METODI DI INIT (INVARIATI) ---
    public void Server_Init_ToVR(Vector3 destination, float speed, MasterFlowController controller)
    {
        if (!HasStateAuthority) return;

        net_destination = destination;
        net_moveSpeed = speed;
        flowController = controller;
        net_isInitialized = true;
        CurrentState = BoxState.MovingToVR;
    }

    public void Server_Init_FromVR(Vector3 moveDirection, float speed, float duration, MasterFlowController controller)
    {
        if (!HasStateAuthority) return;

        net_moveDirection = moveDirection.normalized;  
        net_moveSpeed = speed;
        flowController = controller;
        net_despawnTimer = TickTimer.CreateFromSeconds(Runner, duration);
        net_isInitialized = true;
        CurrentState = BoxState.MovingFromVR;
    }

    public override void FixedUpdateNetwork()
    {
        // NOTA: Ho tolto "!HasStateAuthority" da qui.
        // Il movimento deve girare su TUTTI per non laggare.
        // Il despawn invece resta protetto (vedi sotto).
        if (!net_isInitialized) return;

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
        // 1. MOVIMENTO (Eseguito da Server E Client per fluidità)
        Vector3 direction = (net_destination - transform.position).normalized;
        // IMPORTANTE: Usa Runner.DeltaTime, non Time.deltaTime
        transform.position += direction * net_moveSpeed * Runner.DeltaTime;

        // Opzionale: guarda la destinazione
        if (direction != Vector3.zero) 
            transform.rotation = Quaternion.LookRotation(direction);


        // 2. LOGICA DI CONTROLLO E DESPAWN (TUA LOGICA ORIGINALE)
        // Eseguita SOLO se sei il Server
        if (HasStateAuthority)
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
        }
    }

    private void HandleMovingFromVR()
    {
        // 1. MOVIMENTO (Eseguito da Server E Client per fluidità)
        transform.position += net_moveDirection * net_moveSpeed * Runner.DeltaTime;
        
        // Opzionale
        if (net_moveDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(net_moveDirection);

        // 2. LOGICA DI DESPAWN (TUA LOGICA ORIGINALE)
        // Eseguita SOLO se sei il Server
        if (HasStateAuthority)
        {
            if (net_despawnTimer.Expired(Runner))
            {
                CurrentState = BoxState.Done;
                Debug.Log("Timer box di ritorno scaduto - Despawn");
                Runner.Despawn(Object);
            }
        }
    }
}