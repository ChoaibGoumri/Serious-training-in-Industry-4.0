using Fusion;
using UnityEngine;
using TMPro; // Ricorda di importare TextMeshPro

public class ConveyorUI : NetworkBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private TextMeshProUGUI pauseStatusText;
    [SerializeField] private TextMeshProUGUI pauseTimeText;
    [SerializeField] private TextMeshProUGUI missingItemsText;

    
    public override void Render()
    {
        if (ConveyorBeltSystemManager.Instance == null)
        {
            ToggleUIElements(false, false, false);
            return;
        }

        // Leggi i dati di rete dal Manager
        bool isPaused = ConveyorBeltSystemManager.Instance.IsPaused;
        float lastDuration = ConveyorBeltSystemManager.Instance.LastPauseDuration;
        int missingCount = ConveyorBeltSystemManager.Instance.MissingItemCount;



        // Aggiorna il testo "IN PAUSA"
        // Aggiorna il testo "IN PAUSA"
        if (pauseStatusText)
        {
            pauseStatusText.gameObject.SetActive(true); // Sempre visibile

            if (isPaused)
            {
                pauseStatusText.text = "RIPRENDI";
                pauseStatusText.color = Color.green;
            }
            else
            {
                pauseStatusText.text = "PAUSA";
                pauseStatusText.color = Color.red;
            }
        }


        // Mostra il tempo (solo se > 0)
        if (pauseTimeText)
        {
            pauseTimeText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
                pauseTimeText.text = $"Tempo impiegato: {lastDuration:F2}s"; 
            }
        }

        // 🔽 FIX: Mostra il conteggio (anche '0') dopo la pausa 🔽
        // Il conteggio si attiva quando 'lastDuration' è > 0
        if (missingItemsText)
        {
            missingItemsText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
                // Mostra il conteggio (es. "Oggetti Mancanti: 0")
                missingItemsText.text = $"Oggetti difettati: {missingCount}";
            }
        }
    }

    private void ToggleUIElements(bool status, bool time, bool missing)
    {
        if (pauseStatusText) pauseStatusText.gameObject.SetActive(status);
        if (pauseTimeText) pauseTimeText.gameObject.SetActive(time);
        if (missingItemsText) missingItemsText.gameObject.SetActive(missing);
    }
}