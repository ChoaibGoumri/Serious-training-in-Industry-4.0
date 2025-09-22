using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

public class ARDiagnosticLogger : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private bool continuousLogging = false;
    [SerializeField] private float loggingInterval = 2f;
    
    private ARSession arSession;
    private XROrigin xrOrigin;
    private ARCameraManager arCameraManager;
    private ARPlaneManager arPlaneManager;
    private ARPointCloudManager arPointCloudManager;
    private ARRaycastManager arRaycastManager;
    
    void Start()
    {
        // Trova tutte le componenti AR nella scena
        FindARComponents();
        Debug.Log("🔍 AR Diagnostic Script AVVIATO!");
        Debug.LogWarning("⚠️ Test Warning");
        Debug.LogError("❌ Test Error");   
        if (logOnStart)
        {
            LogARStatus();
        }
        
        if (continuousLogging)
        {
            InvokeRepeating(nameof(LogARStatus), loggingInterval, loggingInterval);
        }
    }
    
    void FindARComponents()
    {
        arSession = FindObjectOfType<ARSession>();
        xrOrigin = FindObjectOfType<XROrigin>();
        arCameraManager = FindObjectOfType<ARCameraManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        arPointCloudManager = FindObjectOfType<ARPointCloudManager>();
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }
    
    [ContextMenu("Log AR Status")]
    public void LogARStatus()
    {
        Debug.Log("=== AR DIAGNOSTIC LOG ===");
        Debug.Log($"Time: {System.DateTime.Now:HH:mm:ss}");
        Debug.Log("========================");
        
        // XR Management Status
        LogXRManagementStatus();
        
        // AR Session Status
        LogARSessionStatus();
        
        // AR Components Status
        LogARComponentsStatus();
        
        // Device Support
        LogDeviceSupport();
        
        // Tracking Status
        LogTrackingStatus();
        
        Debug.Log("=== END DIAGNOSTIC LOG ===\n");
    }
    
    void LogXRManagementStatus()
    {
        Debug.Log("--- XR MANAGEMENT STATUS ---");
        
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager != null)
        {
            Debug.Log($"XR Manager Active: {xrManager.activeLoader != null}");
            if (xrManager.activeLoader != null)
            {
                Debug.Log($"Active Loader: {xrManager.activeLoader.name}");
            }
            else
            {
                Debug.LogWarning("❌ XR Loader non attivo!");
            }
        }
        else
        {
            Debug.LogError("❌ XR Manager non trovato!");
        }
    }
    
    void LogARSessionStatus()
    {
        Debug.Log("--- AR SESSION STATUS ---");
        
        if (arSession != null)
        {
            Debug.Log($"✅ AR Session trovata: {arSession.name}");
            Debug.Log($"Session Enabled: {arSession.enabled}");
            Debug.Log($"Session State: {ARSession.state}");
            
            switch (ARSession.state)
            {
                case ARSessionState.None:
                    Debug.LogWarning("⚠️ AR Session: None");
                    break;
                case ARSessionState.Unsupported:
                    Debug.LogError("❌ AR Session: Unsupported");
                    break;
                case ARSessionState.CheckingAvailability:
                    Debug.Log("🔍 AR Session: Checking Availability");
                    break;
                case ARSessionState.NeedsInstall:
                    Debug.LogWarning("⚠️ AR Session: Needs Install");
                    break;
                case ARSessionState.Installing:
                    Debug.Log("📦 AR Session: Installing");
                    break;
                case ARSessionState.Ready:
                    Debug.Log("✅ AR Session: Ready");
                    break;
                case ARSessionState.SessionInitializing:
                    Debug.Log("🚀 AR Session: Initializing");
                    break;
                case ARSessionState.SessionTracking:
                    Debug.Log("✅ AR Session: Tracking");
                    break;
            }
        }
        else
        {
            Debug.LogError("❌ AR Session non trovata nella scena!");
        }
    }
    
    void LogARComponentsStatus()
    {
        Debug.Log("--- AR COMPONENTS STATUS ---");
        
        // XR Origin
        if (xrOrigin != null)
        {
            Debug.Log($"✅ XR Origin: {xrOrigin.name}");
            Debug.Log($"XR Origin Enabled: {xrOrigin.enabled}");
            Debug.Log($"Origin Height: {xrOrigin.CameraFloorOffsetObject?.transform.localPosition.y}");
            
            // Controlla la camera nell'XROrigin
            var xrCamera = xrOrigin.Camera;
            if (xrCamera != null)
            {
                Debug.Log($"XR Camera Found: {xrCamera.name}");
                Debug.Log($"XR Camera Enabled: {xrCamera.enabled}");
            }
        }
        else
        {
            Debug.LogError("❌ XR Origin non trovata!");
        }
        
        // AR Camera Manager
        if (arCameraManager != null)
        {
            Debug.Log($"✅ AR Camera Manager: {arCameraManager.name}");
            Debug.Log($"Camera Enabled: {arCameraManager.enabled}");
            Debug.Log($"Camera Auto Focus: {arCameraManager.autoFocusRequested}");
        }
        else
        {
            Debug.LogWarning("⚠️ AR Camera Manager non trovata");
        }
        
        // AR Plane Manager
        if (arPlaneManager != null)
        {
            Debug.Log($"✅ AR Plane Manager: {arPlaneManager.name}");
            Debug.Log($"Plane Detection Enabled: {arPlaneManager.enabled}");
            Debug.Log($"Plane Count: {arPlaneManager.trackables.count}");
        }
        else
        {
            Debug.Log("ℹ️ AR Plane Manager non presente");
        }
        
        // AR Point Cloud Manager
        if (arPointCloudManager != null)
        {
            Debug.Log($"✅ AR Point Cloud Manager: {arPointCloudManager.name}");
            Debug.Log($"Point Cloud Enabled: {arPointCloudManager.enabled}");
        }
        else
        {
            Debug.Log("ℹ️ AR Point Cloud Manager non presente");
        }
        
        // AR Raycast Manager
        if (arRaycastManager != null)
        {
            Debug.Log($"✅ AR Raycast Manager: {arRaycastManager.name}");
            Debug.Log($"Raycast Enabled: {arRaycastManager.enabled}");
        }
        else
        {
            Debug.Log("ℹ️ AR Raycast Manager non presente");
        }
    }
    
    void LogDeviceSupport()
    {
        Debug.Log("--- DEVICE SUPPORT ---");
        
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Device Model: {SystemInfo.deviceModel}");
        Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
        
        // Controllo supporto AR
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                Debug.Log("📱 Piattaforma Android - Verifica supporto ARCore");
                break;
            case RuntimePlatform.IPhonePlayer:
                Debug.Log("📱 Piattaforma iOS - Verifica supporto ARKit");
                break;
            default:
                Debug.LogWarning("⚠️ Piattaforma non supportata per AR");
                break;
        }
    }
    
    void LogTrackingStatus()
    {
        Debug.Log("--- TRACKING STATUS ---");
        
        if (arCameraManager != null && arCameraManager.enabled)
        {
            var camera = arCameraManager.GetComponent<Camera>();
            if (camera != null)
            {
                Debug.Log($"Camera Active: {camera.enabled}");
                Debug.Log($"Camera Position: {camera.transform.position}");
                Debug.Log($"Camera Rotation: {camera.transform.rotation.eulerAngles}");
            }
            
            // Controlla anche la XR Origin camera se disponibile
            if (xrOrigin?.Camera != null)
            {
                Debug.Log($"XR Origin Camera Position: {xrOrigin.Camera.transform.position}");
                Debug.Log($"XR Origin Camera Rotation: {xrOrigin.Camera.transform.rotation.eulerAngles}");
            }
            
            // Frame info se disponibile
            if (arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                Debug.Log($"✅ Camera frame disponibile: {image.width}x{image.height}");
                image.Dispose();
            }
            else
            {
                Debug.LogWarning("⚠️ Nessun frame camera disponibile");
            }
        }
    }
    
    // Metodo pubblico per chiamare il log da altri script
    public void LogFromOtherScript()
    {
        Debug.Log("--- LOG CHIAMATO DA SCRIPT ESTERNO ---");
        LogARStatus();
    }
}