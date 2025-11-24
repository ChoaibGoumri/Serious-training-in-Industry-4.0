using UnityEngine;
using System.Collections.Generic;

public class DeformPrefabSurface : MonoBehaviour
{
    [Header("Impostazioni Graffio")]
    public int numberOfScratches = 5;  
    [Range(0.01f, 0.5f)]
    public float scratchWidth = 0.1f; 
    [Range(0.01f, 0.2f)]
    public float scratchDepth = 0.05f;  
    
    [Header("Configurazione")]
    public bool applyOnStart = true;
    public Color scratchColor = new Color(0.3f, 0.3f, 0.3f);  
    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Renderer objRenderer;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        objRenderer = GetComponent<Renderer>();

        if (meshFilter == null)
        {
            Debug.LogError("Serve un MeshFilter per graffiare l'oggetto!");
            return;
        }

        // Salva l'originale
        originalMesh = meshFilter.sharedMesh;

        if (applyOnStart)
        {
            ApplyScratches();
        }
    }

    [ContextMenu("Genera Graffi")]
    public void ApplyScratches()
    {
        // 1. Clona la mesh
        Mesh clonedMesh = Instantiate(originalMesh);
        Vector3[] vertices = clonedMesh.vertices;
        Vector3[] normals = clonedMesh.normals;
        Color[] colors = new Color[vertices.Length]; 

        
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;

        
        for (int n = 0; n < numberOfScratches; n++)
        {
            
            Vector3 startPoint = GetRandomPointOnMesh(clonedMesh);
            Vector3 endPoint = startPoint + (Random.insideUnitSphere * 0.5f);  

            
            Vector3 scratchDir = (endPoint - startPoint).normalized;
            float scratchLength = Vector3.Distance(startPoint, endPoint);

             
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertexPos = vertices[i];
                
                 
                Vector3 project = Vector3.Project(vertexPos - startPoint, scratchDir);
                float distOnLine = Vector3.Dot(vertexPos - startPoint, scratchDir);

                
                if (distOnLine > 0 && distOnLine < scratchLength)
                {
                    
                    float distFromLine = Vector3.Distance(vertexPos, startPoint + project);

                     
                    if (distFromLine < scratchWidth)
                    {
                        
                        float depthFactor = 1f - (distFromLine / scratchWidth);
                        
                       
                        vertices[i] -= normals[i] * (scratchDepth * depthFactor);

                         
                        colors[i] = scratchColor;
                    }
                }
            }
        }

        // 3. Applica tutto
        clonedMesh.vertices = vertices;
        clonedMesh.colors = colors;  
        
        clonedMesh.RecalculateNormals(); 
        clonedMesh.RecalculateBounds();
        
        meshFilter.mesh = clonedMesh;
        
        
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null) mc.sharedMesh = clonedMesh;

         
        Debug.Log("Graffi applicati! Assicurati che il tuo materiale/shader supporti i 'Vertex Colors' se vuoi vedere il colore scuro nel solco.");
    }
 
    private Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        Bounds bounds = mesh.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    [ContextMenu("Ripristina")]
    public void Restore()
    {
        if (meshFilter != null) meshFilter.mesh = originalMesh;
    }
}