using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshRenderer))] 
public class ConveyorBeltController : MonoBehaviour {
    
    [Header("Conveyor Settings")]
    public Vector3 direction = Vector3.right; 
    public float maxSpeed = 5.0f;

    [Header("Physics Simulation")]
    [Tooltip("Velocità di partenza (Basso = Lento/Realistico)")]
    public float startAcceleration = 2.0f;  

    [Tooltip("Velocità di frenata (Alto = Brusco/Immediato)")]
    public float stopDeceleration = 100.0f;  

    [Header("Visual Settings")]
    public float textureScrollSpeed = 0.5f; 
    public string texturePropertyName = "_BaseMap"; 

    [Header("Debug Info")]
    public bool showDebugInfo = true;
    [SerializeField] private float _currentSpeed = 0f; 

    private BoxCollider _collider;
    private MeshRenderer _meshRenderer;
    private Material _conveyorMaterial;
    private Vector2 _currentTextureOffset;

    private void Reset() {
        _collider = GetComponent<BoxCollider>();
        _collider.size = new Vector3(3f, 0.1f, 3f);
        _collider.center = Vector3.zero;
        _collider.isTrigger = false; 
    }

    private void Awake() {
        _collider = GetComponent<BoxCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshRenderer != null) {
            _conveyorMaterial = _meshRenderer.material;
        }

        if (_collider.isTrigger) _collider.isTrigger = false;
        if (direction == Vector3.zero) direction = Vector3.right;
    }

    private void Update()
    {
        CalculateSpeed();
        UpdateVisuals();
    }

    private void CalculateSpeed()
    {
        float targetSpeed = maxSpeed;
        
         
        if (ConveyorBeltSystemManager.Instance != null && ConveyorBeltSystemManager.Instance.IsPaused)
        {
            targetSpeed = 0f;
        }

         
        float speedChangeRate;

        
        if (_currentSpeed < targetSpeed)
        {
            speedChangeRate = startAcceleration;  
        }
        
        else
        {
            speedChangeRate = stopDeceleration;  
        }

         
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
    }

    private void UpdateVisuals()
    {
        if (_conveyorMaterial == null) return;

        if (_currentSpeed > 0f)
        {
            float offsetStep = _currentSpeed * textureScrollSpeed * Time.deltaTime;
            
             
            _currentTextureOffset.x -= offsetStep; 
            
            if(_currentTextureOffset.x < -1f) _currentTextureOffset.x += 1f;

            _conveyorMaterial.SetTextureOffset(texturePropertyName, _currentTextureOffset);
        }
    }

    public Vector3 GetConveyorVelocity() {
        return direction.normalized * _currentSpeed;
    }

    private void OnDrawGizmos() {
        if (!showDebugInfo) return;

        Vector3 velocity = direction.normalized;
        Gizmos.color = _currentSpeed < 0.1f ? Color.red : Color.green;

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