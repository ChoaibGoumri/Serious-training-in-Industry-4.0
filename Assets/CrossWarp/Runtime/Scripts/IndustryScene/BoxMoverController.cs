using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BoxMoverController : NetworkBehaviour
{
    public NetworkPrefabRef boxPrefab;
    public float moveSpeed = 0.2f;

    // Riferimenti ai manager. ARPlaneManager viene trovato subito,
    // gli altri solo al momento dello spawn.
    private ARPlaneManager arPlaneManager;
    private SubplaneConfig subplaneConfig;
    private Transform targetSubplane;

    // Usiamo una lista per gestire più box.
    private List<NetworkObject> spawnedBoxes = new List<NetworkObject>();

    public void Awake()
    {
        // È sicuro cercare ARPlaneManager qui, perché è un componente di base della scena AR.
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        
        if (arPlaneManager == null)
        {
            Debug.LogError("❌ ARPlaneManager non trovato. Assicurati che l'AR sia configurato.");
        }
    }

    public override void Spawned()
    {
        // Questo metodo viene chiamato da Fusion. È un buon posto per inizializzazioni legate alla rete.
    }

    // 👉 Funzione da collegare al bottone
    public void SpawnAndMoveBox()
    {
        // 1. Cerca il subplane solo quando il bottone viene premuto
        if (subplaneConfig == null) {
            subplaneConfig = FindObjectOfType<SubplaneConfig>();
            if (subplaneConfig != null) {
                targetSubplane = subplaneConfig.GetSelectedSubplane()?.transform;
            }
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

        // 2. Trova il primo piano orizzontale rilevato
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
            
            Debug.Log("✅ Piano rilevato. Avvio la procedura di spawn.");
            
            // 3. Spawna la box e muovila
            NetworkObject newBox = Runner.Spawn(boxPrefab, spawnPosition, Quaternion.identity);
            
            if (newBox != null)
            {
                spawnedBoxes.Add(newBox);
                Debug.Log("🚀 Box spawnata! La lista contiene ora " + spawnedBoxes.Count + " box.");
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