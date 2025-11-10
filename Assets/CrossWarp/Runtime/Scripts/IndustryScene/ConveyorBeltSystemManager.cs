using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ConveyorBeltSystemManager : NetworkBehaviour {

    [Header("Riferimenti di Scena")]
    [SerializeField]
    private PrefabSpawner spawner; 

   
    public static ConveyorBeltSystemManager Instance { get; private set; }

  
    [Networked]
    public NetworkBool IsPaused { get; set; }
   
    [Networked]
    public float LastPauseDuration { get; set; }
    [Networked]
    public int MissingItemCount { get; set; }

 
    [Networked]
    private int PauseStartTick { get; set; }

    public override void Spawned() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

 
    public void OnPauseButtonPressed() {
        if (Object == null || !Object.IsValid) {
            Debug.LogWarning("Impossibile attivare la pausa: NetworkObject non valido");
            return;
        }
        Rpc_TogglePause();
    }

 
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_TogglePause() {
        if (!Object.HasStateAuthority) return;
        
        IsPaused = !IsPaused;

        if (IsPaused) {
           
            PauseStartTick = Runner.Tick;
            Debug.Log($"SISTEMA NASTRI: In Pausa. Tick di inizio: {PauseStartTick}");
            
           
            LastPauseDuration = 0f;
            MissingItemCount = 0;

        } else {
             
            if (PauseStartTick > 0) {
               
                int ticksElapsed = Runner.Tick - PauseStartTick;
                float elapsedTime = ticksElapsed * Runner.DeltaTime;
                Debug.Log($"SISTEMA NASTRI: Riavviato. Tempo di pausa: {elapsedTime:F2} secondi.");
                
                LastPauseDuration = elapsedTime;
                PauseStartTick = 0;

                
                CalculateMissingItems(); 
            }
        }
    }

    
    private void CalculateMissingItems() {
        if (!Object.HasStateAuthority) return; 

        
        if (spawner == null)
        {
            Debug.LogError("❌ Calcolo Fallito: Il 'PrefabSpawner' non è collegato al ConveyorBeltSystemManager nell'Inspector!");
            MissingItemCount = -1;  
            return;
        }

        
        int totalSpawned = spawner.GetTotalSpawnedCount();

        
        ConveyorItemMovement[] allItems = FindObjectsOfType<ConveyorItemMovement>();

        
        int currentCountOnBelt = 0;
        foreach (ConveyorItemMovement item in allItems)
        {
            if (item.IsOnConveyor())
            {
                currentCountOnBelt++;
            }
        }

        int conteggioMancanti = totalSpawned - currentCountOnBelt;

        Debug.Log($"📊 Calcolo Oggetti: Totale Spawanti={totalSpawned}, Attuali su Nastro={currentCountOnBelt} => Mancanti={conteggioMancanti}");

        MissingItemCount = conteggioMancanti;
    }
}