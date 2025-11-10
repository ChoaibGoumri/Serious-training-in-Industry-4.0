using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MasterFlowController : NetworkBehaviour
{
    private enum FlowState { Idle, BoxMovingToVR, VR_Waiting, BoxMovingFromVR }

    [Networked]
    private FlowState CurrentState { get; set; }

    [Header("Riferimenti AR (Assegna)")]
    public NetworkPrefabRef arBoxPrefab;
    public float arMoveSpeed = 1.0f; 
    public ARRaycastManager arRaycastManager;

    [Header("Riferimenti VR (Assegna)")]
    public PrefabSpawner vrPrefabSpawner; 
    
    [Header("Effetti Particellari (Assegna)")]
    public GameObject despawnEffectPrefab;
    public GameObject respawnEffectPrefab;

    [Networked] private Vector3 net_arDestSinistra { get; set; }
    [Networked] private Vector3 net_arSpawnDestra { get; set; }
    [Networked] private Vector3 net_arFinalMoveDirection { get; set; }

    private Subplane localSubplaneCache; 

    [Networked] private NetworkObject activeArBox { get; set; }
    [Networked] private TickTimer finalDespawnTimer { get; set; }

    private Rigidbody activeArBox_RB;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentState = FlowState.Idle;
        }

        if (arRaycastManager == null)
            Debug.LogError("MasterFlowController: ARRaycastManager NON è assegnato!");
        if (vrPrefabSpawner == null)
            Debug.LogError("MasterFlowController: PrefabSpawner NON è assegnato!");
    }

    public void Client_StartFlow()
    {
        if (arRaycastManager == null)
        {
            Debug.LogError("--- CLIENT: FALLITO! arRaycastManager è NULL. ---");
            return;
        }

        if (localSubplaneCache == null)
        {
            localSubplaneCache = FindObjectOfType<Subplane>();
            if (localSubplaneCache == null)
            {
                Debug.LogError("--- CLIENT: FALLITO! Non trovo 'Subplane'. ---");
                return;
            }
        }
        
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Vector3 spawnPos = hits[0].pose.position;
            RPC_StartFlow(spawnPos, localSubplaneCache.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0f)), 
                          localSubplaneCache.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0f)),
                          localSubplaneCache.transform.TransformDirection(Vector3.right));
        }
        else
        {
            Debug.LogWarning("--- CLIENT: FALLITO - Raycast non ha colpito un piano AR. ---");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_StartFlow(Vector3 spawnPosition, Vector3 destSinistra, Vector3 spawnDestra, Vector3 moveDirDestra)
    {
        if (CurrentState != FlowState.Idle)
        {
            Debug.LogWarning("--- SERVER: FALLITO - Stato non Idle. ---");
            return;
        }

        if (vrPrefabSpawner == null)
        {
            Debug.LogError("--- SERVER: FALLITO - PrefabSpawner non assegnato. ---");
            return;
        }

        net_arDestSinistra = destSinistra;
        net_arSpawnDestra = spawnDestra;
        net_arFinalMoveDirection = moveDirDestra;

        activeArBox = Runner.Spawn(arBoxPrefab, spawnPosition, Quaternion.identity);
        
        if (activeArBox != null)
        {
            activeArBox_RB = activeArBox.GetComponent<Rigidbody>();
            if (activeArBox_RB != null)
            {
                activeArBox_RB.useGravity = false;
                activeArBox_RB.isKinematic = true; 
            }
        }
        
        CurrentState = FlowState.BoxMovingToVR;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HandleVRExit()
    {
        if (CurrentState != FlowState.VR_Waiting) return; 
        
        Debug.Log("SERVER: FASE 2 completa. Avatar VR arrivato.");
        
        if (net_arSpawnDestra == Vector3.zero)
        {
            Debug.LogError("SERVER: Impossibile spawnare box di ritorno! Coordinate a zero.");
            return;
        }

        activeArBox = Runner.Spawn(arBoxPrefab, net_arSpawnDestra, Quaternion.identity);
        RPC_PlayRespawnEffect(net_arSpawnDestra);
        
        if (activeArBox != null)
        {
            activeArBox_RB = activeArBox.GetComponent<Rigidbody>();
            if (activeArBox_RB != null)
            {
                activeArBox_RB.useGravity = false;
                activeArBox_RB.isKinematic = true; 
            }
        }
        
        finalDespawnTimer = TickTimer.CreateFromSeconds(Runner, 2.0f); 
        CurrentState = FlowState.BoxMovingFromVR;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        switch (CurrentState)
        {
            case FlowState.BoxMovingToVR:
                HandleBoxMovingToVR();
                break;

            case FlowState.BoxMovingFromVR:
                HandleBoxMovingFromVR();
                break;
        }
    }

    private void HandleBoxMovingToVR()
    {
        if (activeArBox_RB == null) return; 

        if (Vector3.Distance(activeArBox_RB.position, net_arDestSinistra) < 0.1f)
        {
            Debug.Log("SERVER: FASE 1 completa. Box AR arrivato a sinistra.");
            RPC_PlayDespawnEffect(activeArBox_RB.position);
            Runner.Despawn(activeArBox); 
            activeArBox = null;
            activeArBox_RB = null;
            
            Debug.Log("SERVER: FASE 2 - Spawno Avatar VR...");
            vrPrefabSpawner.RPC_TestSpawnSingle(0); 
            
            CurrentState = FlowState.VR_Waiting;
        }
        else
        {
            Vector3 currentPos = activeArBox_RB.position;
            Vector3 direction = (net_arDestSinistra - currentPos).normalized;
            Vector3 newPos = currentPos + (direction * arMoveSpeed * Runner.DeltaTime);
            activeArBox_RB.MovePosition(newPos);
        }
    }
    
    private void HandleBoxMovingFromVR()
    {
        if (activeArBox_RB == null) return;
            
 
        if (finalDespawnTimer.Expired(Runner))
        {
            Debug.Log("SERVER: FASE 3 completa. Timer scaduto, box si ferma.");
            
      
            activeArBox_RB = null;
            activeArBox = null;
            
            CurrentState = FlowState.Idle;
        }
        else
        {
             
            Vector3 currentPos = activeArBox_RB.position;
            Vector3 newPos = currentPos + (net_arFinalMoveDirection * arMoveSpeed * Runner.DeltaTime);
            activeArBox_RB.MovePosition(newPos);
        }
       
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayDespawnEffect(Vector3 position)
    {
        if (despawnEffectPrefab != null)
        {
            GameObject vfx = Instantiate(despawnEffectPrefab, position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayRespawnEffect(Vector3 position)
    {
        if (respawnEffectPrefab != null)
        {
            GameObject vfx = Instantiate(respawnEffectPrefab, position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
    }
}