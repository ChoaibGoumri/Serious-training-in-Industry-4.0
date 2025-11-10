using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PrefabSpawner : NetworkBehaviour
{
    public NetworkPrefabRef[] prefabsToSpawn;
    public Transform spawnPoint;

    [SerializeField]
    private int numberOfPrefabsToSpawn = 2;

    [SerializeField]
    private float spawnInterval = 0f;

    private bool hasSpawnedInitialPrefabs = false;
    private List<NetworkObject> spawnedObjects = new List<NetworkObject>();
    private Coroutine spawnCoroutine;

    // 🔽 AGGIUNGI QUESTA RIGA 🔽
    // Contatore totale degli oggetti spawnati.
    private int totalSpawnedCount = 0;


    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer && !hasSpawnedInitialPrefabs)
        {
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnSequenceCoroutine());
            }
        }
    }

    private IEnumerator SpawnSequenceCoroutine()
    {
        // Spostato qui: solo se la coroutine parte davvero
        hasSpawnedInitialPrefabs = true;

        if (Runner == null || !Runner.IsServer || prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogError("❌ Spawn aborted: Runner null, not server, or no prefabs assigned.");
            yield break;
        }

        for (int i = 0; i < numberOfPrefabsToSpawn; i++)
        {
            // UNICA MODIFICA: selezione random invece di sequenziale
            int prefabIndex = Random.Range(0, prefabsToSpawn.Length);
            NetworkPrefabRef prefabToSpawn = prefabsToSpawn[prefabIndex];

            Debug.Log($"🔄 Attempting to spawn prefab index {prefabIndex}: {prefabToSpawn}");

            if (!prefabToSpawn.IsValid)
            {
                Debug.LogError($"❌ PrefabRef at index {prefabIndex} is not valid.");
                continue;
            }

            Vector3 spawnPosition = spawnPoint.position;
            RaycastHit hit;
            if (Physics.Raycast(spawnPoint.position + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
            {
                spawnPosition = hit.point + Vector3.up * 0.1f;
            }

            NetworkObject spawnedObject = Runner.Spawn(
                prefabToSpawn,
                spawnPosition,
                Quaternion.identity,
                inputAuthority: PlayerRef.None
            );

            if (spawnedObject != null)
            {
                spawnedObjects.Add(spawnedObject);
                Debug.Log($"✅ Spawned prefab index {prefabIndex} successfully.");
            }
            else
            {
                Debug.LogError($"❌ Failed to spawn prefab at index {prefabIndex}");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RespawnObjectsServerOnly()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Reset del flag per permettere nuovi spawn
        hasSpawnedInitialPrefabs = false;
        
        spawnCoroutine = StartCoroutine(SpawnSequenceCoroutine());
    }


    
    public void TestDirectSpawn()
    {
        if (Runner == null || Object == null || prefabsToSpawn == null || prefabsToSpawn.Length == 0 || spawnPoint == null)
        {
            Debug.LogError("❌ TestDirectSpawn: Missing required references.");
            return;
        }
        
        // Avvia coroutine per spawn con intervallo
        StartCoroutine(TestSpawnWithIntervalCoroutine());
    }

    private IEnumerator TestSpawnWithIntervalCoroutine()
    {
        for (int i = 0; i < numberOfPrefabsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, prefabsToSpawn.Length);
            RPC_TestSpawnSingle(randomIndex);
            
            // Aspetta l'intervallo prima del prossimo spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TestSpawnSingle(int prefabIndex)
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0 || prefabIndex >= prefabsToSpawn.Length || spawnPoint == null)
        {
            Debug.LogError("❌ RPC_TestSpawnSingle: Invalid index or missing data.");
            return;
        }

        NetworkPrefabRef prefabToSpawn = prefabsToSpawn[prefabIndex];
        if (!prefabToSpawn.IsValid)
        {
            Debug.LogError($"❌ Prefab at index {prefabIndex} is not valid.");
            return;
        }

        Vector3 testPosition = spawnPoint.position;
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
        {
            testPosition = hit.point + Vector3.up * 0.1f;
        }

        NetworkObject testObj = Runner.Spawn(
            prefabToSpawn,
            testPosition,
            Quaternion.identity,
            inputAuthority: PlayerRef.None
        );

        if (testObj != null)
        {
            spawnedObjects.Add(testObj);
            totalSpawnedCount++; // ⬅️ AGGIUNGI QUESTA RIGA
            Debug.Log($"✅ Manually spawned prefab index {prefabIndex}");
        }
        else
        {
            Debug.LogError($"❌ Manual spawn failed for prefab index {prefabIndex}");
        }
    }

    public int GetTargetSpawnCount()
    {
        return numberOfPrefabsToSpawn;
    }

    public int GetCurrentActiveCount()
    {
        if (spawnedObjects == null) return 0;

        // Pulisce la lista da oggetti che sono stati distrutti (diventati null)
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null || !spawnedObjects[i].IsValid)
            {
                spawnedObjects.RemoveAt(i);
            }
        }

        // Restituisce il conteggio pulito
        return spawnedObjects.Count;
    }
    public int GetTotalSpawnedCount()
    {
        return totalSpawnedCount;
    }
}