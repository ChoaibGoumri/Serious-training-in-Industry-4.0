using UnityEngine;
using System.Collections.Generic;

public class DeformPrefabSurface : MonoBehaviour
{
    [Header("Impostazioni Graffio")]
    public int numberOfScratches = 5; // Quanti graffi fare
    [Range(0.01f, 0.5f)]
    public float scratchWidth = 0.1f; // Larghezza del solco
    [Range(0.01f, 0.2f)]
    public float scratchDepth = 0.05f; // Profondità del solco (quanto scava)
    
    [Header("Configurazione")]
    public bool applyOnStart = true;
    public Color scratchColor = new Color(0.3f, 0.3f, 0.3f); // Colore scuro per l'interno del graffio

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
        Color[] colors = new Color[vertices.Length]; // Prepariamo i colori dei vertici

        // Inizializza i colori a bianco (o al colore attuale se esiste)
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;

        // 2. Genera i solchi
        for (int n = 0; n < numberOfScratches; n++)
        {
            // Scegliamo due punti casuali nella bounding box dell'oggetto per definire la linea del graffio
            Vector3 startPoint = GetRandomPointOnMesh(clonedMesh);
            Vector3 endPoint = startPoint + (Random.insideUnitSphere * 0.5f); // Il graffio finisce poco lontano

            // Calcoliamo la direzione del graffio
            Vector3 scratchDir = (endPoint - startPoint).normalized;
            float scratchLength = Vector3.Distance(startPoint, endPoint);

            // Controlliamo ogni vertice: è vicino alla linea del graffio?
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertexPos = vertices[i];
                
                // Matematica per trovare la distanza di un punto da una linea
                Vector3 project = Vector3.Project(vertexPos - startPoint, scratchDir);
                float distOnLine = Vector3.Dot(vertexPos - startPoint, scratchDir);

                // Se il vertice è "lungo" la linea del graffio (non prima o dopo)
                if (distOnLine > 0 && distOnLine < scratchLength)
                {
                    // Calcola quanto è distante lateralmente dalla linea centrale del graffio
                    float distFromLine = Vector3.Distance(vertexPos, startPoint + project);

                    // Se è dentro la larghezza del graffio, scava!
                    if (distFromLine < scratchWidth)
                    {
                        // Più siamo vicini al centro del graffio, più scendiamo giù (forma a V)
                        float depthFactor = 1f - (distFromLine / scratchWidth);
                        
                        // Sposta il vertice in basso (opposto alla normale)
                        vertices[i] -= normals[i] * (scratchDepth * depthFactor);

                        // Colora il vertice di scuro (così il graffio si vede anche col colore)
                        colors[i] = scratchColor;
                    }
                }
            }
        }

        // 3. Applica tutto
        clonedMesh.vertices = vertices;
        clonedMesh.colors = colors; // Applica i colori ai vertici (richiede shader che supporti Vertex Color)
        
        clonedMesh.RecalculateNormals(); // Fondamentale per la luce
        clonedMesh.RecalculateBounds();
        
        meshFilter.mesh = clonedMesh;
        
        // Se usi un MeshCollider, aggiornalo
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null) mc.sharedMesh = clonedMesh;

        // Avviso per lo shader
        Debug.Log("Graffi applicati! Assicurati che il tuo materiale/shader supporti i 'Vertex Colors' se vuoi vedere il colore scuro nel solco.");
    }

    // Funzione di aiuto per trovare un punto a caso
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