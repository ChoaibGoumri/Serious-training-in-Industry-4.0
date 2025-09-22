using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BoxMoverController : NetworkBehaviour
{
    public NetworkPrefabRef boxPrefab;
    public float moveSpeed = 0.2f;

    [Header("Riferimento al PrefabSpawner")]
    public PrefabSpawner prefabSpawner; 

    [Header("Effetti particellari")]
    public GameObject particleEffectPrefab;      
    public GameObject respawnParticlePrefab;    

    private ARPlaneManager arPlaneManager;
    private SubplaneConfig subplaneConfig;
    private Transform targetSubplane;

    private List<NetworkObject> spawnedBoxes = new List<NetworkObject>();

    public void Awake()
    {
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        if (arPlaneManager == null)
            Debug.LogError("❌ ARPlaneManager non trovato. Assicurati che l'AR sia configurato.");
    }

    public void SpawnAndMoveBox()
    {
        if (subplaneConfig == null)
        {
            subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if (subplaneConfig != null)
                targetSubplane = subplaneConfig.GetSelectedSubplane()?.transform;
        }

        if (subplaneConfig == null || targetSubplane == null)
        {
            Debug.LogError("⛔ Monitor non rilevato. Clicca il bottone 'Configura' prima di spawnare.");
            return;
        }

        if (arPlaneManager.trackables.count == 0)
        {
            Debug.LogWarning("⛔ Nessun piano AR rilevato. Scansiona l'ambiente.");
            return;
        }

        ARPlane firstPlane = null;
        foreach (var plane in arPlaneManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                firstPlane = plane;
                break;
            }
        }

        if (firstPlane != null)
        {
            Vector3 spawnPosition = firstPlane.center;
            NetworkObject newBox = Runner.Spawn(boxPrefab, spawnPosition, Quaternion.identity);

            if (newBox != null)
            {
                spawnedBoxes.Add(newBox);
                Debug.Log("🚀 Box spawnata! La lista contiene ora " + spawnedBoxes.Count + " box.");

                
                StartCoroutine(FadeAndHideBoxWithEffect(newBox, 2f, 1f));
            }
            else
            {
                Debug.LogError("❌ Spawn fallito! Controlla la configurazione del prefab.");
            }
        }
        else
        {
            Debug.LogWarning("⛔ Nessun piano orizzontale rilevato.");
        }
    }

    private IEnumerator FadeAndHideBoxWithEffect(NetworkObject netObj, float waitTime, float fadeDuration)
    {
        yield return new WaitForSeconds(waitTime);

        GameObject box = netObj.gameObject;
        Renderer[] renderers = box.GetComponentsInChildren<Renderer>();
        Material[] materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;

        // Dissolvenza
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            foreach (var mat in materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
            yield return null;
        }

        
        if (particleEffectPrefab != null)
        {
            GameObject particles = Instantiate(particleEffectPrefab, box.transform.position, Quaternion.identity);
            Destroy(particles, 3f);
        }

         
        box.SetActive(false);

         
        if (prefabSpawner != null)
        {
            RPC_RequestDirectSpawn();
        }

        
        yield return new WaitForSeconds(6f);

      
        if (respawnParticlePrefab != null)
        {
            GameObject respawnParticles = Instantiate(respawnParticlePrefab, box.transform.position, Quaternion.identity);
            Destroy(respawnParticles, 3f);
        }
 
        box.SetActive(true);

       
        foreach (var mat in materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = 1f;
                mat.color = c;
            }
        }

        Debug.Log("✅ Box riapparsa nello stesso punto AR!");
    }

   
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDirectSpawn()
    {
        if (prefabSpawner != null)
        {
            prefabSpawner.TestDirectSpawn();
            Debug.Log("✅ TestDirectSpawn eseguito dallo StateAuthority del PrefabSpawner!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (targetSubplane == null) return;

        for (int i = spawnedBoxes.Count - 1; i >= 0; i--)
        {
            NetworkObject box = spawnedBoxes[i];

            if (box == null)
            {
                spawnedBoxes.RemoveAt(i);
                continue;
            }

            Vector3 direction = (targetSubplane.position - box.transform.position).normalized;
            box.transform.position += direction * moveSpeed * Runner.DeltaTime;
        }
    }
}
