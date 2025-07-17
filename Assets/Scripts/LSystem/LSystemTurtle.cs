using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LSystemTurtle : MonoBehaviour
{
    [SerializeField] private RenderModeType m_renderMode = RenderModeType.LineRenderer;

    [SerializeField] private string m_axiom = "F";

    [Range(0, 10)]
    [SerializeField] private int m_iterations = 4;

    [Range(0.0f, 360.0f)]
    [SerializeField] private float m_angle = 25.0f;

    [Range(0.1f, 5.0f)]
    [SerializeField] private float m_length = 1.0f;

    [Range(0.1f, 5.0f)]
    [SerializeField] private float m_width = 0.1f;

    [SerializeField] private float m_widthScale = 2.0f;

    [SerializeField] private float m_lengthScale = 1.5f;

    [SerializeField] private Vector3 m_initialDirection = Vector3.up;

    [SerializeField] private List<Rule> m_rulesList;
    [SerializeField] private List<Material> m_materials;

    [SerializeField] private bool m_autoUpdate = false;

    private GameObject m_lSystemContainer;
    private Material m_defaultMaterial;

    public void GenerateInEditor()
    {
        LSystemGenerator generator = new(m_rulesList);
        string lsystem = generator.Generate(m_axiom, m_iterations);

        List<LSystemParser.SymbolToken> tokens = LSystemParser.Parse(lsystem);
        LSystemInterpreter interpreter = new(m_angle, m_widthScale, m_lengthScale, m_materials);

        List<LSystemRenderer.Segment> segments = interpreter.Interpret(tokens, m_initialDirection, m_length, m_width, m_defaultMaterial);

        if (m_renderMode == RenderModeType.LineRenderer)
        {
            LSystemRenderer.DrawLines(segments, m_lSystemContainer.transform);
        }
        else
        {
            LSystemRenderer.DrawMeshes(segments, m_lSystemContainer.transform);
        }
    }

    public void SetupRenderParent()
    {
        if (m_lSystemContainer == null)
        {
            m_lSystemContainer = new GameObject("LSystemContainer");
            m_lSystemContainer.transform.SetParent(this.transform, false);
        }
    }

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
