using Fusion;
using UnityEngine;
using TMPro;  

public class ConveyorUI : NetworkBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private TextMeshProUGUI pauseStatusText;
    [SerializeField] private TextMeshProUGUI pauseTimeText;
    
    [Tooltip("Mostra: Difettosi presi / Difettosi Totali")]
    [SerializeField] private TextMeshProUGUI defectiveStatsText; // Rinomina nell'inspector se necessario
    
    [Tooltip("Mostra: Validi sul nastro")]
    [SerializeField] private TextMeshProUGUI validItemsText; 
    
    [Tooltip("Mostra: Punteggio Totale")]
    [SerializeField] private TextMeshProUGUI scoreText; 

    public override void Render()
    {
        if (ConveyorBeltSystemManager.Instance == null)
        {
            ToggleUIElements(false);
            return;
        }

        var manager = ConveyorBeltSystemManager.Instance;

        bool isPaused = manager.IsPaused;
        float lastDuration = manager.LastPauseDuration;
        
        // Recuperiamo i nuovi dati calcolati
        int defCaught = manager.DefectiveCaughtCount;
        int defTotal = manager.DefectiveTotalCount;
        int validCount = manager.ValidOnBeltCount;
        int score = manager.CurrentScore;

        // 1. Stato Pausa
        if (pauseStatusText)
        {
            pauseStatusText.gameObject.SetActive(true); 
            if (isPaused)
            {
                pauseStatusText.text = " || RESUME";
                pauseStatusText.color = Color.green;
            }
            else
            {
                pauseStatusText.text = " >> PAUSE";
                pauseStatusText.color = Color.red;
            }
        }

        // Mostriamo le statistiche solo se c'è stata una pausa (come da tua logica originale)
        bool showStats = lastDuration > 0;

        // 2. Tempo
        if (pauseTimeText)
        {
            pauseTimeText.gameObject.SetActive(showStats);
            if(showStats) pauseTimeText.text = $"• Time taken: {lastDuration:F2}s"; 
        }

        // 3. Difettosi (X / Y)
        if (defectiveStatsText)
        {
            defectiveStatsText.gameObject.SetActive(showStats);
            if(showStats)
            {
                // Es: "Defective: 2 / 5"
                defectiveStatsText.text = $"• Defective: {defCaught} / {defTotal}";
            }
        }

        // 4. Validi (Sul nastro)
        if (validItemsText)
        {
            validItemsText.gameObject.SetActive(showStats);
            if (showStats)
            {
                validItemsText.text = $"• Non Defective items: {validCount}";
            }
        }

        // 5. Punteggio Massimo
        if (scoreText)
        {
            scoreText.gameObject.SetActive(showStats);
            if (showStats)
            {
                scoreText.text = $"• Score: {score}";
            }
        }
    }
    
    private void ToggleUIElements(bool active)
    {
        if (pauseStatusText) pauseStatusText.gameObject.SetActive(active);
        if (pauseTimeText) pauseTimeText.gameObject.SetActive(active);
        if (defectiveStatsText) defectiveStatsText.gameObject.SetActive(active);
        if (validItemsText) validItemsText.gameObject.SetActive(active);
        if (scoreText) scoreText.gameObject.SetActive(active);
    }
}