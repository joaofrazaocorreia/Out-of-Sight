using UnityEngine;

/// <summary>
/// Add this script to any GameObject to automatically add a black outline
/// without replacing the existing material.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class AddOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0f, 0.1f)] private float outlineWidth = 0.01f;
    
    [Header("Options")]
    [Tooltip("Update outline properties in real-time")]
    [SerializeField] private bool updateInRealTime = true;

    private Renderer rend;
    private Material[] originalMaterials;
    private bool outlineAdded = false;

    void OnEnable()
    {
        AddOutlineMaterial();
    }

    void Update()
    {
        if (updateInRealTime && outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }

    void AddOutlineMaterial()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) return;

        // Store original materials
        originalMaterials = rend.sharedMaterials;

        // Check if outline material is assigned
        if (outlineMaterial == null)
        {
            Debug.LogWarning($"Outline material not assigned on {gameObject.name}. Please assign an outline material.");
            return;
        }

        // Check if outline already added
        foreach (Material mat in originalMaterials)
        {
            if (mat == outlineMaterial)
            {
                outlineAdded = true;
                return;
            }
        }

        // Add outline material to the material array
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        originalMaterials.CopyTo(newMaterials, 0);
        newMaterials[newMaterials.Length - 1] = outlineMaterial;

        rend.sharedMaterials = newMaterials;
        outlineAdded = true;

        // Set initial properties
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

    void OnDisable()
    {
        RemoveOutlineMaterial();
    }

    void RemoveOutlineMaterial()
    {
        if (rend == null || !outlineAdded) return;

        // Restore original materials
        rend.sharedMaterials = originalMaterials;
        outlineAdded = false;
    }

    void OnValidate()
    {
        if (Application.isPlaying || !enabled) return;
        
        // Update material properties when values change in inspector
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }
}
