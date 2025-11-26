using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ConveyorBeltSystemManager : NetworkBehaviour {

    [Header("Riferimenti di Scena")]
    [SerializeField] private PrefabSpawner spawner; 

    [Header("Configurazione Prefab (Liste)")]
    [Tooltip("Inserisci qui tutti i vari prefab considerati VALIDI")]
    [SerializeField] private GameObject[] validItemPrefabs;

    [Tooltip("Inserisci qui tutti i vari prefab considerati DIFETTOSI")]
    [SerializeField] private GameObject[] defectiveItemPrefabs;

    public static ConveyorBeltSystemManager Instance { get; private set; }

    // --- Variabili di Stato (Interne) ---
    [Networked] private int MissingItemOffset { get; set; }
    [Networked] private int PauseStartTick { get; set; }

    // --- Variabili Pubbliche per UI e Logica ---
    [Networked] public NetworkBool IsPaused { get; set; }
    [Networked] public float LastPauseDuration { get; set; }

    // Variabili per il punteggio
    [Networked] public int DefectiveCaughtCount { get; set; } // Quelli tolti (Mancanti)
    [Networked] public int DefectiveTotalCount { get; set; }  // Tolti + Quelli sul nastro
    [Networked] public int ValidOnBeltCount { get; set; }     // Validi attualmente sul nastro
    [Networked] public int CurrentScore { get; set; }         // Punteggio Totale

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
            // --- INIZIO PAUSA ---
            PauseStartTick = Runner.Tick;
            Debug.Log($"SISTEMA NASTRI: In Pausa. Tick di inizio: {PauseStartTick}");
            
            // Calcolo offset generico basato sul numero totale di oggetti
            int total = spawner.GetTotalSpawnedCount();
            int current = 0;
            
            // Conta quanti oggetti di QUALSIASI tipo sono sul nastro
            foreach (var item in FindObjectsOfType<ConveyorItemMovement>()) 
                if (item.IsOnConveyor()) current++;
            
            MissingItemOffset = total - current;
            LastPauseDuration = 0f;

        } else {
            // --- FINE PAUSA (RESUME) ---
            if (PauseStartTick > 0) {
                int ticksElapsed = Runner.Tick - PauseStartTick;
                float elapsedTime = ticksElapsed * Runner.DeltaTime;
                Debug.Log($"SISTEMA NASTRI: Riavviato. Tempo di pausa: {elapsedTime:F2} secondi.");
                
                LastPauseDuration = elapsedTime;
                PauseStartTick = 0;

                CalculateScoreAndStats(); 
            }
        }
    }

    private void CalculateScoreAndStats() {
        if (!Object.HasStateAuthority) return; 
        if (spawner == null) return;

        int totalSpawned = spawner.GetTotalSpawnedCount();
        ConveyorItemMovement[] allItems = FindObjectsOfType<ConveyorItemMovement>();

        int currentCountOnBelt = 0;      // Totale oggetti fisici sul nastro
        int currentDefectiveOnBelt = 0;  // Di cui difettosi
        int currentValidOnBelt = 0;      // Di cui validi
        
        // Scansioniamo ogni oggetto in scena
        foreach (ConveyorItemMovement item in allItems)
        {
            if (item.IsOnConveyor())
            {
                currentCountOnBelt++;

                // Controlla se l'oggetto è nella lista dei DIFETTOSI
                if (IsItemInList(item.gameObject, defectiveItemPrefabs))
                {
                    currentDefectiveOnBelt++;
                }
                // Controlla se l'oggetto è nella lista dei VALIDI
                else if (IsItemInList(item.gameObject, validItemPrefabs))
                {
                    currentValidOnBelt++;
                }
                // Se non è in nessuna delle due liste, viene contato nel totale ma ignorato per Valid/Defective specifici
            }
        }

        // Calcolo "Mancanti" (Quelli messi in AR)
        // Formula: Totale Spawnati - Quelli fisici sul nastro - Offset
        int itemsInAR = (totalSpawned - currentCountOnBelt) - MissingItemOffset;
        if (itemsInAR < 0) itemsInAR = 0;

        // Assegnazione valori Networked
        
        // 1. Difettosi Catturati (Assumiamo che tutto ciò che manca sia un difettoso risolto)
        DefectiveCaughtCount = itemsInAR; 

        // 2. Difettosi Totali (Catturati + Quelli che sono sfuggiti e sono ancora sul nastro)
        DefectiveTotalCount = DefectiveCaughtCount + currentDefectiveOnBelt;

        // 3. Validi (Solo quelli sul nastro)
        ValidOnBeltCount = currentValidOnBelt;

        // 4. Punteggio (Validi sul nastro + Difettosi catturati)
        CurrentScore = ValidOnBeltCount + DefectiveCaughtCount;

        Debug.Log($"STATS -> Score: {CurrentScore} | Defective: {DefectiveCaughtCount}/{DefectiveTotalCount} | Valid: {ValidOnBeltCount}");
    }

    // Nuova funzione helper che controlla una LISTA di prefab
    private bool IsItemInList(GameObject itemObj, GameObject[] prefabList)
    {
        if (prefabList == null || itemObj == null) return false;

        // Scorre la lista dei prefab di riferimento
        foreach(var prefabRef in prefabList)
        {
            if (prefabRef == null) continue;

            // Se il nome dell'oggetto in scena contiene il nome del prefab (es. "BadItem" è dentro "BadItem(Clone)")
            if (itemObj.name.Contains(prefabRef.name))
            {
                return true;
            }
        }
        return false;
    }
}