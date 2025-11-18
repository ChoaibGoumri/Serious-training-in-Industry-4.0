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
        // Se lo stato non è cambiato, non fare nulla
        if (transitionManager.transitionState == PreviousTransitionState) {
            return;
        }

        // --- PARTE 2: IL CLIENT VR (Destinatario) ---
        // Rileva la FINE della transizione AR->VR sul client VR
        // (che ha appena ottenuto l'autorità)
        if (HasStateAuthority &&
            PlatformManager.IsDesktop() && // Assicura che siamo su VR
            transitionManager.transitionState == TransitionState.Ended &&
            PreviousTransitionState != TransitionState.Ended &&
            movableObject.worldState == MovableObjectState.TransitioningToVR) 
        {
            Debug.LogWarning($"[ArVrOffsetCorrector] FINE AR->VR. Applico offset PREDEFINITO: {chosenVrOffset}");

            // 1. SOVRASCRIVI i dati [Networked] con la tua posizione scelta
            movableObject.lastOffsetToSubplane = chosenVrOffset;
            movableObject.lastRotationOffsetToSubplane = chosenVrRotation;
            
            // 2. Finalizza lo stato
            movableObject.worldState = MovableObjectState.inVR;
        }

        // Aggiorna lo stato precedente per il prossimo tick
        PreviousTransitionState = transitionManager.transitionState;
    }
}