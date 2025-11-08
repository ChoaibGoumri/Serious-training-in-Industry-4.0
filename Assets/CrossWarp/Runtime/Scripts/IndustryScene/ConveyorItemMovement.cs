using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
public class ConveyorItemMovement : NetworkBehaviour {
    private Rigidbody _rigidbody;
    private ConveyorBeltController currentBelt;
    private MovableObject movableObject;
    private Vector3 targetPosition; // 🔹 Posizione target per interpolazione
    private bool wasKinematicBefore;

    public override void Spawned() {
        _rigidbody = GetComponent<Rigidbody>();
        movableObject = GetComponent<MovableObject>();
        targetPosition = transform.position;
        wasKinematicBefore = _rigidbody.isKinematic;

        Debug.Log($"🚀 [Spawned] {gameObject.name} con Rigidbody attivato.");
    }

    private void OnCollisionEnter(Collision collision) {
        if (Object == null || !Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null) {
            //Debug.Log($"📥 [OnCollisionEnter] {gameObject.name} entrato in conveyor '{belt.gameObject.name}'");
            currentBelt = belt;
            targetPosition = transform.position;
            
            // 🔹 Rendi kinematic ma mantieni l'interpolazione
            if (movableObject != null && !movableObject.selected) {
                wasKinematicBefore = _rigidbody.isKinematic;
                _rigidbody.isKinematic = true;
                // 🔹 IMPORTANTE: Mantieni l'interpolazione attiva!
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                //Debug.Log($"🔒 Rigidbody reso kinematic con interpolazione");
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (!Object.HasStateAuthority) return;

        ConveyorBeltController belt = collision.gameObject.GetComponent<ConveyorBeltController>();
        if (belt != null && belt == currentBelt) {
            //Debug.Log($"📤 [OnCollisionExit] {gameObject.name} uscito da conveyor '{belt.gameObject.name}'");
            currentBelt = null;
            
            // 🔹 Ripristina il Rigidbody
            if (movableObject != null) {
                _rigidbody.isKinematic = wasKinematicBefore;
                _rigidbody.velocity = Vector3.zero;
                
                // Aggiorna gli offset
                movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(transform.position);
                movableObject.lastRotationOffsetToSubplane = movableObject.CalculateLastRotationOffsetToSubplane(transform.rotation);

               //Debug.Log($"🔓 Rigidbody ripristinato");
            }
        }
    }

    public override void FixedUpdateNetwork() {
        if (!Object.HasStateAuthority) return;

        // 🔹 Se l'oggetto è selezionato, esci dal nastro
        if (movableObject != null && movableObject.selected) {
            if (currentBelt != null) {
                Debug.Log($"⚠️ [FixedUpdateNetwork] {gameObject.name} è stato selezionato, esco dal nastro");
                currentBelt = null;
                _rigidbody.isKinematic = wasKinematicBefore;
                _rigidbody.velocity = Vector3.zero;
            }
            return;
        }

        if (currentBelt == null) {
            return;
        }

        // 🔹 Calcola la nuova posizione target
        Vector3 velocity = currentBelt.GetConveyorVelocity();
        targetPosition += velocity * Runner.DeltaTime;
        
        // 🔹 USA MovePosition per movimento fluido con interpolazione
        _rigidbody.MovePosition(targetPosition);
        
        // 🔹 Aggiorna gli offset di MovableObject
        if (movableObject != null) {
            movableObject.lastOffsetToSubplane = movableObject.CalculateLastOffsetToSubplane(targetPosition);
        }
        
        if (Time.frameCount % 30 == 0) {
            Debug.Log($"➡️ [FixedUpdateNetwork] {gameObject.name} target pos: {targetPosition}");
        }
    }

    public bool IsOnConveyor() {
        return currentBelt != null;
    }
}