using UnityEngine;
using Fusion;

[RequireComponent(typeof(Collider))]
public class ExitPointTrigger : MonoBehaviour
{
    private MasterFlowController masterController;
    private bool hasTriggered = false; 

    void Start()
    {
        var col = GetComponent<Collider>();
        // Assicurati che sia un SENSORE
        if (!col.isTrigger)
        {
            Debug.LogWarning($"--- EXITPOINT: Il collider su {gameObject.name} NON era un trigger! Lo imposto ora.", gameObject);
            col.isTrigger = true; 
        }

        masterController = FindObjectOfType<MasterFlowController>();
        if (masterController == null)
        {
            Debug.LogError($"--- EXITPOINT: FALLITO! Non trovo 'MasterFlowController' nella scena!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"--- EXITPOINT: Qualcosa ha toccato il trigger: {other.name}", other.gameObject);

        if (hasTriggered || masterController == null) return;

        // 👇 CONTROLLO REIMPOSTATO: Cerca lo script che sappiamo essere lì
        if (other.GetComponent<ConveyorItemMovement>() != null)
        {
            Debug.Log($"✅ ExitPoint: Avatar VR rilevato (Script: ConveyorItemMovement)! Invio RPC...", other.gameObject);
            
            hasTriggered = true; 
            masterController.RPC_HandleVRExit(); // Chiama il regista

            if (other.GetComponent<NetworkObject>() != null)
            {
                FindObjectOfType<NetworkRunner>().Despawn(other.GetComponent<NetworkObject>());
            }
        }
        else
        {
            Debug.LogWarning($"--- EXITPOINT: Oggetto {other.name} ha toccato il trigger, ma non ha lo script 'ConveyorItemMovement'!", other.gameObject);
        }
    }
}