using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
public class ConveyorItemMovement : NetworkBehaviour {
    private Rigidbody _rigidbody;
    private ConveyorBeltController currentBelt;
    private MovableObject movableObject;
    
    // targetPosition diventa il cursore virtuale che guida il movimento
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

        targetPosition = _rigidbody.position; 
        wasKinematicBefore = _rigidbody.isKinematic;
        _previousKinematicState = IsKinematicOnBelt;

        ApplyKinematicState(IsKinematicOnBelt);
    }

    // 💡 1. SENSORE DI AUTORITÀ (Gira su tutti i client)
    // Se siamo in VR, vediamo un nastro sotto, ma non comandiamo noi... richiediamo il comando!
    private void FixedUpdate()
    {
        if (Object != null && Object.IsValid && !Object.HasStateAuthority && PlatformManager.IsDesktop())
        {
            // Raycast leggermente spostato (+0.1) per non colpire noi stessi
            if (_rigidbody != null && Physics.Raycast(_rigidbody.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 2.0f))
            {
                if (hit.collider.GetComponentInParent<ConveyorBeltController>() != null)
                {
                    Object.RequestStateAuthority();
                }
            }
        }
    }

    private void ApplyKinematicState(bool shouldBeKinematicOnBelt) {
        if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null) return;

        if (shouldBeKinematicOnBelt) {
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        } else {
            _rigidbody.isKinematic = wasKinematicBefore;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    // Gestione collisioni fisiche (es. caduta in AR)
    private void OnCollisionEnter(Collision collision) => HandleCollision(collision);
    private void OnCollisionStay(Collision collision) => HandleCollision(collision);

    private void HandleCollision(Collision collision) {
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;
        if (currentBelt != null) return; // Priorità al target già agganciato
        if (movableObject != null && movableObject.selected) return;

        ConveyorBeltController belt = collision.gameObject.GetComponentInParent<ConveyorBeltController>();
        if (belt != null) {
            TrySetBelt(belt);
        }
    }

    private void TrySetBelt(ConveyorBeltController belt) {
        if (currentBelt == belt) return; 
        if (movableObject != null && movableObject.selected) return;

        currentBelt = belt;
        
        // Quando agganciamo, resettiamo il target sulla posizione attuale
        targetPosition = _rigidbody.position; 
        
        IsKinematicOnBelt = true;
    }

    private void OnCollisionExit(Collision collision) {
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponentInParent<ConveyorBeltController>();
        if (belt != null && belt == currentBelt) {
            currentBelt = null;
            IsKinematicOnBelt = false;
            ApplyKinematicState(IsKinematicOnBelt); 
            
            if (movableObject != null && _rigidbody != null) {
                _rigidbody.velocity = Vector3.zero;
                movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(transform.position);
                movableObject.lastRotationOffsetToSubplane = movableObject.CalculateLastRotationOffsetToSubplane(transform.rotation);
            }
        }
    }
     
    public override void FixedUpdateNetwork() {
          
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;
        
        // --- GESTIONE PRESA MANUALE ---
        if (movableObject != null && movableObject.selected) {
            if (currentBelt != null) {
                currentBelt = null;
                IsKinematicOnBelt = false; 
                if (_rigidbody != null) _rigidbody.velocity = Vector3.zero;
            }
            if (IsKinematicOnBelt) IsKinematicOnBelt = false;
            return;
        }

        if (ConveyorBeltSystemManager.Instance != null && ConveyorBeltSystemManager.Instance.IsPaused) return;  
        
        // --- 2. RICERCA DEL NASTRO (MAGNATE + SNAP) ---
        if (currentBelt == null) {
            
            // VR: Cinematico (niente gravità = niente sfarfallio)
            // AR: Fisico (gravità attiva)
            if (PlatformManager.IsDesktop()) {
                 if (!IsKinematicOnBelt) IsKinematicOnBelt = true;
            } else {
                 if (IsKinematicOnBelt) IsKinematicOnBelt = false;
            }

            // Sync posizione continua per AR (evita sfarfallio in caduta)
            if (movableObject != null && _rigidbody != null && !PlatformManager.IsDesktop()) {
                 movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(_rigidbody.position);
                 movableObject.lastRotationOffsetToSubplane = movableObject.CalculateLastRotationOffsetToSubplane(_rigidbody.rotation);
            }

            // 💡 3. RAYCAST MAGNETE 💡
            if (_rigidbody != null) 
            {
                // Raycast verso il basso (lunghezza 3m)
                if (Physics.Raycast(_rigidbody.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 3.0f)) 
                {
                    ConveyorBeltController beltFound = hit.collider.GetComponentInParent<ConveyorBeltController>();
                    if (beltFound != null) {
                        
                        TrySetBelt(beltFound);
                        
                        // SNAP: Teletrasporto sulla superficie del nastro
                        // +0.05f verticale per non compenetrare il pavimento
                        Vector3 snapPosition = hit.point + (Vector3.up * 0.05f);
                        
                        // Aggiorniamo SUBITO il targetPosition allo snap
                        targetPosition = snapPosition;

                        // Aggiorniamo MovableObject e Rigidbody
                        if (movableObject != null) {
                            movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(snapPosition);
                        }
                        _rigidbody.MovePosition(snapPosition);
                        return;
                    }
                }
            }
            return;
        }

        // --- 3. MOVIMENTO SUL NASTRO ---
        
        if (!IsKinematicOnBelt) IsKinematicOnBelt = true;

        if (Runner == null) return;

        Vector3 velocity = currentBelt.GetConveyorVelocity();
        
        // 💡 4. LOGICA ACCUMULATIVA (Sblocca l'oggetto immobile)
        // Non usiamo transform.position (che viene frenato da MovableObject).
        // Accumuliamo la velocità su targetPosition, che avanza inesorabile.
        targetPosition += velocity * Runner.DeltaTime;
        
        // A. Aggiorna Offset di MovableObject (MovableObject è costretto a inseguire targetPosition)
        if (movableObject != null) {
            movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(targetPosition);
        }
        
        // B. Muoviamo fisicamente il Rigidbody
        if (_rigidbody != null) {
            _rigidbody.MovePosition(targetPosition);
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
    public bool IsOnConveyor() => currentBelt != null;
    public override void Despawned(NetworkRunner runner, bool hasState) {
        if (currentBelt != null) currentBelt = null;
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

    /// <summary>
/// Chiamato quando l'oggetto è appena tornato da AR a VR,
/// per resettare lo stato del nastro e riallineare il target.
/// </summary>
public void ResetAfterReturnToVR()
{
    if (_rigidbody == null)
        _rigidbody = GetComponent<Rigidbody>();

    // Nessun nastro agganciato
    currentBelt = null;

    // Fuori dal nastro: niente kinematic forzato
    IsKinematicOnBelt = false;
    ApplyKinematicState(false);

    if (_rigidbody != null)
    {
        // Azzera subito velocità e rotazione per evitare comportamenti strani
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        // Riallinea il cursore virtuale alla posizione attuale
        targetPosition = _rigidbody.position;
    }

    Debug.Log($"[ConveyorItemMovement] ResetAfterReturnToVR su {gameObject.name}");
}

}