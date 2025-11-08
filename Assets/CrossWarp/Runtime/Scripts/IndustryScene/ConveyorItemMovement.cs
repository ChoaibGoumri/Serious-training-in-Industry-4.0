using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody))]
public class ConveyorItemMovement : NetworkBehaviour {
    private Rigidbody _rigidbody;
    private ConveyorBeltController currentBelt;

    public override void Spawned() {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        Debug.Log($"🚀 [Spawned] {gameObject.name} con Rigidbody attivato.");
    }

    // Nello script ConveyorItemMovement.cs

// Nello script ConveyorItemMovement.cs

    private void OnTriggerEnter(Collider other) 
    {
        // 👇 --- AGGIUNGI QUESTO CONTROLLO --- 👇
        // Se l'oggetto è già stato distrutto (p.es. dall'ExitPoint), non fare nulla.
        if (Object == null || !Object.HasStateAuthority) return;
        // 👆 --- FINE DEL FIX --- 👆

        ConveyorBeltController belt = other.GetComponent<ConveyorBeltController>();
        if (belt != null) {
            Debug.Log($"📥 [OnTriggerEnter] {gameObject.name} entrato in conveyor '{belt.gameObject.name}'");
            currentBelt = belt;
        } else {
            Debug.Log($"📥 [OnTriggerEnter] {gameObject.name} collide con '{other.gameObject.name}' ma non è conveyor.");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!Object.HasStateAuthority) return;

        ConveyorBeltController belt = other.GetComponent<ConveyorBeltController>();
        if (belt != null && belt == currentBelt) {
            Debug.Log($"📤 [OnTriggerExit] {gameObject.name} uscito da conveyor '{belt.gameObject.name}'");
            currentBelt = null;
            _rigidbody.velocity = Vector3.zero;
        }
    }

    public override void FixedUpdateNetwork() {
        if (!Object.HasStateAuthority) return;
        if (_rigidbody == null) return;

        if (currentBelt == null) {
            // Per debug
            if (Time.frameCount % 60 == 0) {
                //Debug.Log($"⚠️ [FixedUpdateNetwork] {gameObject.name} NON è su nessun conveyor.");
            }
            return;
        }

        Vector3 velocity = currentBelt.GetConveyorVelocity();

        if (_rigidbody.velocity != velocity) {
            Debug.Log($"➡️ [FixedUpdateNetwork] {gameObject.name} velocità impostata a {velocity}");
        }

        _rigidbody.velocity = velocity;
    }
}
