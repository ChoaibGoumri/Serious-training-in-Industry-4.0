using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
public class ConveyorItemMovement : NetworkBehaviour {
    private Rigidbody _rigidbody;
    private ConveyorBeltController currentBelt;
    private MovableObject movableObject;
    private Vector3 targetPosition;
    private bool wasKinematicBefore;

    // ✅ FIX: Rimosso OnChanged - Usiamo una proprietà con getter/setter custom
    [Networked]
    private NetworkBool IsKinematicOnBelt { get; set; }

    // ✅ FIX: Variabile locale per tracciare il valore precedente
    private bool _previousKinematicState;

    public override void Spawned() {
        _rigidbody = GetComponent<Rigidbody>();
        movableObject = GetComponent<MovableObject>();
        
        // ✅ FIX: Verifica che il Rigidbody esista
        if (_rigidbody == null) {
            Debug.LogError($"❌ [Spawned] {gameObject.name} non ha un Rigidbody!");
            return;
        }

        targetPosition = transform.position;
        wasKinematicBefore = _rigidbody.isKinematic;
        _previousKinematicState = IsKinematicOnBelt;

        Debug.Log($"🚀 [Spawned] {gameObject.name} con Rigidbody attivato.");

        // Applica lo stato corretto all'avvio
        ApplyKinematicState(IsKinematicOnBelt);
    }

    // ✅ FIX PER IL LAG: Applica lo stato su tutti i client
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
        // ✅ FIX: Verifica null safety completa
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null) {
            currentBelt = belt;
            targetPosition = transform.position;
            
            // Imposta lo stato [Networked] (solo Authority)
            if (movableObject != null && !movableObject.selected) {
                IsKinematicOnBelt = true; 
                Debug.Log($"✅ [OnCollisionEnter] {gameObject.name} è entrato sul nastro");
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        // ✅ FIX: Verifica null safety
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null && belt == currentBelt) {
            currentBelt = null;
            
            // Imposta lo stato [Networked] (solo Authority)
            IsKinematicOnBelt = false;
            
            if (movableObject != null && _rigidbody != null) {
                _rigidbody.velocity = Vector3.zero;
                movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(transform.position);
                movableObject.lastRotationOffsetToSubplane = movableObject.CalculateLastRotationOffsetToSubplane(transform.rotation);
            }

            Debug.Log($"✅ [OnCollisionExit] {gameObject.name} è uscito dal nastro");
        }
    }

    public override void FixedUpdateNetwork() {
        // ✅ FIX: Verifica completa dell'oggetto
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        // La selezione ha priorità
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

        // ⭐️ NUOVA LOGICA DI PAUSA ⭐️
        if (ConveyorBeltSystemManager.Instance != null && ConveyorBeltSystemManager.Instance.IsPaused) {
            return; // Non muovere l'oggetto se il sistema è in pausa
        }
        
        // Il resto della logica di movimento
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

    // ✅ FIX: Controlla i cambiamenti nello stato in Render
    public override void Render() {
        // Questo viene chiamato su tutti i client per sincronizzare visualmente
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