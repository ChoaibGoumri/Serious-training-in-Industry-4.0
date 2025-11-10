using Fusion;
using UnityEngine;
using TMPro;  

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

    
        bool isPaused = ConveyorBeltSystemManager.Instance.IsPaused;
        float lastDuration = ConveyorBeltSystemManager.Instance.LastPauseDuration;
        int missingCount = ConveyorBeltSystemManager.Instance.MissingItemCount;


 
        if (pauseStatusText)
        {
            pauseStatusText.gameObject.SetActive(true); 

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


        
        if (pauseTimeText)
        {
            pauseTimeText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
                pauseTimeText.text = $"Tempo impiegato: {lastDuration:F2}s"; 
            }
        }

       
        if (missingItemsText)
        {
            missingItemsText.gameObject.SetActive(lastDuration > 0);
            if(lastDuration > 0)
            {
               
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