using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Main component that generates and renders a Lindenmayer System (L-System).
/// It orchestrates generation, parsing, interpretation, and rendering.
/// </summary>
public class LSystemTurtle : MonoBehaviour
{
    [Header("L-System Parameters")]
    [SerializeField] private string m_axiom = "F";
    [SerializeField] private List<Rule> m_rulesList;
    [Range(0, 10)] [SerializeField] private int m_iterations = 4;

    [Header("Turtle Configuration")]
    [Range(0.0f, 360.0f)]   [SerializeField] private float m_angle = 25.0f;
    [Range(0.1f, 5.0f)]     [SerializeField] private float m_length = 1.0f;
    [Range(0.1f, 5.0f)]     [SerializeField] private float m_width = 0.1f;
    [Range(0.0f, 10.0f)]    [SerializeField] private float m_widthScale = 2.0f;
    [Range(0.0f, 10.0f)]    [SerializeField] private float m_lengthScale = 1.5f;
    [SerializeField] private Vector3 m_initialDirection = Vector3.up;

    [Header("Rendering")]
    [SerializeField] private RenderModeType m_renderMode = RenderModeType.LineRenderer;
    [SerializeField] private List<Material> m_materials;
    [SerializeField] private Material m_defaultMaterial;

    [Header("Editor")]
    [SerializeField] private bool m_autoUpdate = false;

    private GameObject m_lSystemContainer;

    /// <summary>
    /// Generates and renders the L-System in the editor.
    /// </summary>
    public void GenerateInEditor()
    {
        // Generate the expanded  L-System string.
        LSystemGenerator generator = new(m_rulesList);
        string lsystem = generator.Generate(m_axiom, m_iterations);

        // Parse the string into symbols with optional parameters.
        List<LSystemParser.SymbolToken> tokens = LSystemParser.Parse(lsystem);

        // Interpret the tokens into renderrable segments.
        LSystemInterpreter interpreter = new(m_angle, m_widthScale, m_lengthScale, m_materials);
        List<LSystemRenderer.Segment> segments = interpreter.Interpret(tokens, m_initialDirection, m_length, m_width, m_defaultMaterial);

        // Draw the segments.
        if (m_renderMode == RenderModeType.LineRenderer)
        {
            LSystemRenderer.DrawLines(segments, m_lSystemContainer.transform);
        }
        else
        {
            LSystemRenderer.DrawMeshes(segments, m_lSystemContainer.transform);
        }
    }

    /// <summary>
    /// Creates the L-System container if it doesn't exist.
    /// </summary>
    public void SetupRenderParent()
    {
        if (m_lSystemContainer == null)
        {
            m_lSystemContainer = new GameObject("LSystemContainer");
            m_lSystemContainer.transform.SetParent(this.transform, false);
        }
    }

    /// <summary>
    /// Destroys all children of the container GameObject.
    /// </summary>
    public void ClearChildren()
    {
        for (int i = m_lSystemContainer.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(m_lSystemContainer.transform.GetChild(i).gameObject);
        }
    }

    public enum RenderModeType
    {
        LineRenderer,
        Cylinder,
    }

    /// <summary>
    /// Ensures rule keys are valid in the editor.
    /// </summary>
    private void OnValidate()
    {
        if (m_rulesList != null)
        {
            foreach (var rule in m_rulesList)
            {
                rule.ValidateKey();
            }
        }
    }
}
