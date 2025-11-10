using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ConveyorBeltSystemManager : NetworkBehaviour {

    [Header("Riferimenti di Scena")]
    [SerializeField]
    private PrefabSpawner spawner; // ❗️ Trascina qui il tuo PrefabSpawner

    // 1. SINGLETON
    public static ConveyorBeltSystemManager Instance { get; private set; }

    // 2. STATO SINCRONIZZATO
    [Networked]
    public NetworkBool IsPaused { get; set; }
    
    // 3. VARIABILI PER LA UI
    [Networked]
    public float LastPauseDuration { get; set; }
    [Networked]
    public int MissingItemCount { get; set; }

    // 4. TIMER
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

    // 5. FUNZIONE PER BOTTONE UI
    public void OnPauseButtonPressed() {
        if (Object == null || !Object.IsValid) {
            Debug.LogWarning("Impossibile attivare la pausa: NetworkObject non valido");
            return;
        }
        Rpc_TogglePause();
    }

    // 6. RPC PER GESTIRE LA PAUSA
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_TogglePause() {
        if (!Object.HasStateAuthority) return;
        
        IsPaused = !IsPaused;

        if (IsPaused) {
            // PAUSA
            PauseStartTick = Runner.Tick;
            Debug.Log($"SISTEMA NASTRI: In Pausa. Tick di inizio: {PauseStartTick}");
            
            // Azzera la UI
            LastPauseDuration = 0f;
            MissingItemCount = 0;

        } else {
            // RIPRESA
            if (PauseStartTick > 0) {
                // Calcola il tempo
                int ticksElapsed = Runner.Tick - PauseStartTick;
                float elapsedTime = ticksElapsed * Runner.DeltaTime;
                Debug.Log($"SISTEMA NASTRI: Riavviato. Tempo di pausa: {elapsedTime:F2} secondi.");
                
                LastPauseDuration = elapsedTime;
                PauseStartTick = 0;

                // Calcola gli oggetti mancanti
                CalculateMissingItems(); 
            }
        }
    }

    // 7. CALCOLO OGGETTI MANCANTI (Logica Dinamica)
    private void CalculateMissingItems() {
        if (!Object.HasStateAuthority) return; 

        // 1. Verifica che lo spawner sia collegato
        if (spawner == null)
        {
            Debug.LogError("❌ Calcolo Fallito: Il 'PrefabSpawner' non è collegato al ConveyorBeltSystemManager nell'Inspector!");
            MissingItemCount = -1; // Mostra -1 per errore
            return;
        }

        // 2. Chiedi allo spawner il totale (Conteggio A)
        int totalSpawned = spawner.GetTotalSpawnedCount();

        // 3. Trova tutti gli oggetti trasportabili nella scena
        ConveyorItemMovement[] allItems = FindObjectsOfType<ConveyorItemMovement>();

        // 4. Conta quanti sono *attualmente sul nastro* (Conteggio B)
        int currentCountOnBelt = 0;
        foreach (ConveyorItemMovement item in allItems)
        {
            if (item.IsOnConveyor())
            {
                currentCountOnBelt++;
            }
        }

        // 5. Il calcolo è la differenza
        int conteggioMancanti = totalSpawned - currentCountOnBelt;

        Debug.Log($"📊 Calcolo Oggetti: Totale Spawanti={totalSpawned}, Attuali su Nastro={currentCountOnBelt} => Mancanti={conteggioMancanti}");

        MissingItemCount = conteggioMancanti;
    }
}