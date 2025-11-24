using Fusion;
using UnityEngine;


[RequireComponent(typeof(MovableObject), typeof(TransitionManager))]
public class ArVrOffsetCorrector : NetworkBehaviour
{
    private MovableObject movableObject;
    private TransitionManager transitionManager;

    
    [Header("Posizione di Arrivo in VR")]
    [Tooltip("Posizione LOCALE rispetto all'anchor VR (NCPCenter)")]
    public Vector3 chosenVrOffset = new Vector3(0, 0, 0.5f);  

    [Tooltip("Rotazione LOCALE rispetto all'anchor VR")]
    public Quaternion chosenVrRotation = Quaternion.identity;  
    


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

     
    if (transitionManager.transitionState == PreviousTransitionState)
        return;

 
    if (HasStateAuthority &&
        PlatformManager.IsDesktop() &&                               
        transitionManager.transitionState == TransitionState.Ended && 
        PreviousTransitionState != TransitionState.Ended)           
    {
        Debug.LogWarning($"[ArVrOffsetCorrector] FINE AR->VR. Applico offset PREDEFINITO: {chosenVrOffset}");

         
        movableObject.lastOffsetToSubplane = chosenVrOffset;
        movableObject.lastRotationOffsetToSubplane = chosenVrRotation;

         
        movableObject.worldState = MovableObjectState.inVR;

        
        if (movableObject.selected)
        {
            movableObject.ReleaseSelection();
        }

       
        movableObject.SetShowing(true);

        
        var conveyor = GetComponent<ConveyorItemMovement>();
        if (conveyor != null)
        {
            conveyor.ResetAfterReturnToVR();
        }
    }

     
    PreviousTransitionState = transitionManager.transitionState;
}




}