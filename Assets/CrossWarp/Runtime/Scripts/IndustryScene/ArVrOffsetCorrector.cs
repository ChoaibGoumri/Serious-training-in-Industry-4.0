using Fusion;
using UnityEngine;


[RequireComponent(typeof(MovableObject), typeof(TransitionManager))]
public class ArVrOffsetCorrector : NetworkBehaviour
{
    private MovableObject movableObject;
    private TransitionManager transitionManager;

    
    [Header("Posizione di Arrivo in VR")]
    [Tooltip("Posizione LOCALE rispetto all'anchor VR (NCPCenter)")]
    public Vector3 chosenVrOffset = new Vector3(0, 0, 0.5f); // Esempio: 50cm davanti

    [Tooltip("Rotazione LOCALE rispetto all'anchor VR")]
    public Quaternion chosenVrRotation = Quaternion.identity; // Esempio: rotazione dritta
    // 💡 ------------------------------------- 💡


    [Networked]
    private TransitionState PreviousTransitionState { get; set; }

    public override void Spawned()
    {
        movableObject = GetComponent<MovableObject>();
        transitionManager = GetComponent<TransitionManager>();
        
        if (movableObject == null || transitionManager == null) {
            this.enabled = false;
            return;
        }
        PreviousTransitionState = transitionManager.transitionState;
    }


public override void FixedUpdateNetwork()
{
    if (transitionManager == null || movableObject == null)
        return;

    // Se lo stato della transizione non è cambiato, non fare nulla
    if (transitionManager.transitionState == PreviousTransitionState)
        return;

    // --- CLIENT VR (destinatario) ---
    // Rileva la FINE della transizione AR->VR sul client VR / Desktop
    if (HasStateAuthority &&
        PlatformManager.IsDesktop() &&                              // siamo sul client VR / Desktop
        transitionManager.transitionState == TransitionState.Ended && 
        PreviousTransitionState != TransitionState.Ended)           // appena arrivati in Ended
    {
        Debug.LogWarning($"[ArVrOffsetCorrector] FINE AR->VR. Applico offset PREDEFINITO: {chosenVrOffset}");

        // 1. Imposta la posizione/rotazione di arrivo (valori Networked)
        movableObject.lastOffsetToSubplane = chosenVrOffset;
        movableObject.lastRotationOffsetToSubplane = chosenVrRotation;

        // 2. Il mondo torna ufficialmente in VR
        movableObject.worldState = MovableObjectState.inVR;

        // 3. Sicurezza: l'oggetto NON deve più risultare selezionato
        if (movableObject.selected)
        {
            movableObject.ReleaseSelection();
        }

        // 4. Sicurezza: riaccendi subito mesh + collider lato VR
        movableObject.SetShowing(true);

        // 5. Reset pulito del componente che gestisce il nastro
        var conveyor = GetComponent<ConveyorItemMovement>();
        if (conveyor != null)
        {
            conveyor.ResetAfterReturnToVR();
        }
    }

    // Aggiorna lo stato precedente per il prossimo tick
    PreviousTransitionState = transitionManager.transitionState;
}




}