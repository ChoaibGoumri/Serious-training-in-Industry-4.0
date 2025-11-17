using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MasterFlowController : NetworkBehaviour
{
  

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

    public override void Spawned()
    {
        if (arRaycastManager == null)
            Debug.LogError("MasterFlowController: ARRaycastManager NON è assegnato!");
        if (vrPrefabSpawner == null)
            Debug.LogError("MasterFlowController: PrefabSpawner NON è assegnato!");
    }

    

    public void Client_StartFlow()
    {
        if (arRaycastManager == null) return;

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
            RPC_StartFlow(spawnPos, 
                          localSubplaneCache.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0f)),
                          localSubplaneCache.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0f)),
                          localSubplaneCache.transform.TransformDirection(Vector3.right));
        }
        else
        {
            Debug.LogWarning("--- CLIENT: Raycast non ha colpito un piano AR. Riprova. ---");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_StartFlow(Vector3 spawnPosition, Vector3 destSinistra, Vector3 spawnDestra, Vector3 moveDirDestra)
    {
        Debug.Log($"RPC_StartFlow ricevuto - SpawnPos: {spawnPosition}");
        
        net_arDestSinistra = destSinistra;
        net_arSpawnDestra = spawnDestra;
        net_arFinalMoveDirection = moveDirDestra.normalized;

        NetworkObject boxNO = Runner.Spawn(arBoxPrefab, spawnPosition, Quaternion.identity);

        if (boxNO != null)
        {
            StartCoroutine(InitializeBoxDelayed(boxNO));
        }
    }

    private System.Collections.IEnumerator InitializeBoxDelayed(NetworkObject boxNO)
    {
        yield return new WaitForSeconds(0.1f);  

        ArBoxMover mover = boxNO.GetComponent<ArBoxMover>();
        if (mover != null)
        {
            mover.Server_Init_ToVR(net_arDestSinistra, arMoveSpeed, this);
            Debug.Log("Box inizializzato con successo");
        }
        else
        {
            Debug.LogError("--- SERVER: Il prefab 'arBoxPrefab' NON HA lo script 'ArBoxMover'! ---");
        }
    }

  
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestVRSpawn()
    {
        // SENZA BLOCCO! Ogni box che arriva spawna un avatar.
        Debug.Log("SERVER: Box arrivato. Spawno Avatar VR...");
        if (vrPrefabSpawner != null)
        {
            vrPrefabSpawner.RPC_TestSpawnSingle(0);
        }
    }

    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_HandleVRExit()
    {
         
        Debug.Log("SERVER: Avatar VR uscito. Spawno Box di ritorno.");

        if (net_arSpawnDestra == Vector3.zero)
        {
            Debug.LogError("SERVER: Impossibile spawnare box di ritorno! Coordinate a zero.");
            return;
        }

       
        NetworkObject boxNO = Runner.Spawn(arBoxPrefab, net_arSpawnDestra, Quaternion.identity);
        RPC_PlayRespawnEffect(net_arSpawnDestra);

       
        if (boxNO != null)
        {
            ArBoxMover mover = boxNO.GetComponent<ArBoxMover>();
            if (mover != null)
            {
                mover.Server_Init_FromVR(net_arFinalMoveDirection, arMoveSpeed, 2.0f, this);
            }
        }
    }


   

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayDespawnEffect(Vector3 position)
    {
        if (despawnEffectPrefab != null)
        {
            GameObject vfx = Instantiate(despawnEffectPrefab, position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayRespawnEffect(Vector3 position)
    {
        if (respawnEffectPrefab != null)
        {
            GameObject vfx = Instantiate(respawnEffectPrefab, position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
    }
}