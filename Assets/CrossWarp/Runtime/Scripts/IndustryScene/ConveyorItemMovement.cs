using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
public class ConveyorItemMovement : NetworkBehaviour {
    private Rigidbody _rigidbody;
    private ConveyorBeltController currentBelt;
    private MovableObject movableObject;
    private Vector3 targetPosition;
    private bool wasKinematicBefore;

   
    [Networked]
    private NetworkBool IsKinematicOnBelt { get; set; }

     
    private bool _previousKinematicState;

    public override void Spawned() {
        _rigidbody = GetComponent<Rigidbody>();
        movableObject = GetComponent<MovableObject>();
        
        
        if (_rigidbody == null) {
            Debug.LogError($"❌ [Spawned] {gameObject.name} non ha un Rigidbody!");
            return;
        }

        targetPosition = transform.position;
        wasKinematicBefore = _rigidbody.isKinematic;
        _previousKinematicState = IsKinematicOnBelt;

        Debug.Log($"🚀 [Spawned] {gameObject.name} con Rigidbody attivato.");

        
        ApplyKinematicState(IsKinematicOnBelt);
    }

    
    private void ApplyKinematicState(bool shouldBeKinematicOnBelt) {
        if (_rigidbody == null) {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null) return;
        }

        if (shouldBeKinematicOnBelt) {
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        } else {
            _rigidbody.isKinematic = wasKinematicBefore;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null) {
         
            TrySetBelt(belt);
        }
    }

    private void OnCollisionStay(Collision collision) {
    
        if (currentBelt != null || Object == null || !Object.IsValid || !Object.HasStateAuthority) {
            return;
        }

        if (movableObject != null && movableObject.selected) {
            return;
        }

        
        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null) {
            Debug.Log($"[OnCollisionStay] {gameObject.name} ha ri-acquisito il nastro.");
            
            TrySetBelt(belt);
        }
    }

     
    private void TrySetBelt(ConveyorBeltController belt) {
        
        if (currentBelt == belt) return; 

        
        if (movableObject != null && movableObject.selected) return;

        currentBelt = belt;
        
     
        targetPosition = transform.position; 
        
        IsKinematicOnBelt = true;
    }

    private void OnCollisionExit(Collision collision) {
            
            if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

            ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
            if (belt != null && belt == currentBelt) {
                currentBelt = null;
                
                
              
                IsKinematicOnBelt = false;
              
                ApplyKinematicState(IsKinematicOnBelt); 
                
               
                if (movableObject != null && _rigidbody != null) {
                    _rigidbody.velocity = Vector3.zero; // <-- Questa era la linea 84
                    movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(transform.position);
                    movableObject.lastRotationOffsetToSubplane = movableObject.CalculateLastRotationOffsetToSubplane(transform.rotation);
                }
            }
        }
    public override void FixedUpdateNetwork() {
         
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        
        if (movableObject != null && movableObject.selected) {
            if (currentBelt != null) {
                Debug.Log($"⚠️ [FixedUpdateNetwork] {gameObject.name} è stato selezionato, esco dal nastro");
                currentBelt = null;
                IsKinematicOnBelt = false; 
                
                if (_rigidbody != null) {
                    _rigidbody.velocity = Vector3.zero;
                }
            }
            if (IsKinematicOnBelt) IsKinematicOnBelt = false;
            return;
        }

        
        if (ConveyorBeltSystemManager.Instance != null && ConveyorBeltSystemManager.Instance.IsPaused) {
            return;  
        }
        
        
        if (currentBelt == null) {
            if (IsKinematicOnBelt) IsKinematicOnBelt = false;
            return;
        }

        if (!IsKinematicOnBelt) IsKinematicOnBelt = true;

        if (Runner == null) return;

        Vector3 velocity = currentBelt.GetConveyorVelocity();
        targetPosition += velocity * Runner.DeltaTime;
        
        if (_rigidbody != null) {
            _rigidbody.MovePosition(targetPosition);
        }
        
        if (movableObject != null) {
            movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(targetPosition);
        }
        
        if (Time.frameCount % 60 == 0) {
            Debug.Log($"➡️ [FixedUpdateNetwork] {gameObject.name} target pos: {targetPosition}");
        }
    }

   
    public override void Render() {
            
           
            if (movableObject != null && movableObject.selected) {
                
                _previousKinematicState = IsKinematicOnBelt; 
                return;  
            }

            
            if (_previousKinematicState != IsKinematicOnBelt) {
                _previousKinematicState = IsKinematicOnBelt;
                ApplyKinematicState(IsKinematicOnBelt);
            }
    }

    public bool IsOnConveyor() {
        return currentBelt != null;
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        if (currentBelt != null) {
            currentBelt = null;
        }
        IsKinematicOnBelt = false;
    }

    [ContextMenu("Debug State")]
    private void DebugState() {
        Debug.Log($"--- {gameObject.name} State ---");
        Debug.Log($"Current Belt: {(currentBelt != null ? currentBelt.name : "None")}");
        Debug.Log($"IsKinematicOnBelt: {IsKinematicOnBelt}");
        Debug.Log($"Has Authority: {(Object != null ? Object.HasStateAuthority : false)}");
        Debug.Log($"Target Position: {targetPosition}");
    }
}