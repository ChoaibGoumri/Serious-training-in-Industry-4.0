using Fusion;
using UnityEngine;
using TMPro;  

public class ConveyorUI : NetworkBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private TextMeshProUGUI pauseStatusText;
    [SerializeField] private TextMeshProUGUI pauseTimeText;
    [SerializeField] private TextMeshProUGUI missingItemsText;
    
    // 1. NUOVO RIFERIMENTO
    [SerializeField] private TextMeshProUGUI validItemsText; 

    public override void Render()
    {
        if (ConveyorBeltSystemManager.Instance == null)
        {
            ToggleUIElements(false, false, false, false);
            return;
        }

        bool isPaused = ConveyorBeltSystemManager.Instance.IsPaused;
        float lastDuration = ConveyorBeltSystemManager.Instance.LastPauseDuration;
        int missingCount = ConveyorBeltSystemManager.Instance.MissingItemCount;
        
        // 2. RECUPERO IL CONTEGGIO VALIDI
        int validCount = ConveyorBeltSystemManager.Instance.ValidItemCount; 

        // --- Gestione Status Pausa ---
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

        // --- Gestione Tempo ---
        if (pauseTimeText)
        {
            pauseTimeText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
                pauseTimeText.text = $"Time taken: {lastDuration:F2}s"; 
            }
        }

        // --- Gestione Oggetti Mancanti (Difettosi) ---
        if (missingItemsText)
        {
            missingItemsText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
                missingItemsText.text = $"Defective items: {missingCount}";
            }
        }

        // 3. NUOVA LOGICA PER OGGETTI VALIDI
        if (validItemsText)
        {
            // Lo mostriamo solo se c'è un report attivo (come gli altri testi)
            validItemsText.gameObject.SetActive(lastDuration > 0);
            if (lastDuration > 0)
            {
                validItemsText.text = $"Valid items: {validCount}";
            }
        }
    }

    // Aggiornato il metodo helper per includere il nuovo testo
    private void ToggleUIElements(bool status, bool time, bool missing, bool valid)
    {
        if (pauseStatusText) pauseStatusText.gameObject.SetActive(status);
        if (pauseTimeText) pauseTimeText.gameObject.SetActive(time);
        if (missingItemsText) missingItemsText.gameObject.SetActive(missing);
        if (validItemsText) validItemsText.gameObject.SetActive(valid);
    }
}