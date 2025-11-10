using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ConveyorBeltController : MonoBehaviour {
    [Header("Conveyor Settings")]
    public Vector3 direction = Vector3.right; 
    public float speed = 5.0f; 

    [Header("Debug Info")]
    public bool showDebugInfo = true;

    private BoxCollider _collider;

    private void Reset() {
        _collider = GetComponent<BoxCollider>();
        _collider.size = new Vector3(3f, 0.1f, 3f);
        _collider.center = Vector3.zero;
        _collider.isTrigger = false; 
        Debug.Log($"🔧 [RESET] Conveyor configurato: Size: {_collider.size}, IsTrigger: {_collider.isTrigger}");
    }

    private void Awake() {
        _collider = GetComponent<BoxCollider>();

        if (_collider.isTrigger) {
            Debug.LogWarning($"⚠️ [{gameObject.name}] BoxCollider era trigger, lo disattivo");
            _collider.isTrigger = false;
        }

        if (direction == Vector3.zero) {
            direction = Vector3.right;
            Debug.LogWarning("⚠️ Direction era zero, impostata a Vector3.right");
        }

        Debug.Log($"🎯 Conveyor inizializzato - Direzione: {direction}, Speed: {speed}");
    }

    public Vector3 GetConveyorVelocity() {
        return direction.normalized * speed;
    }

    private void OnDrawGizmos() {
        Vector3 velocity = direction.normalized;
        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Gizmos.DrawLine(center, center + velocity * 2f);
        Gizmos.DrawSphere(center + velocity * 2f, 0.1f);

        if (_collider != null) {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_collider.center, _collider.size);
        }
    }
}
