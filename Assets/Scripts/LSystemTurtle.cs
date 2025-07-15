using System.Collections.Generic;
using UnityEngine;

public class LSystemTurtle : MonoBehaviour
{
    [SerializeField] private RenderMode m_renderMode = RenderMode.LineRenderer;

    [SerializeField] public string m_axiom = "F";
    [SerializeField] public int m_iterations = 4;
    [SerializeField] public float m_angle = 25.0f;
    [SerializeField] public float m_length = 1.0f;
    [SerializeField] private List<Rule> m_rulesList = new List<Rule>();

    [SerializeField] private bool m_autoUpdate = false;

    private Dictionary<char, string> m_rules = new Dictionary<char, string>();
    private string m_currentString;

    private Vector3 m_currentPosition = Vector3.zero;
    private Stack<TransformInfo> m_transformStack = new Stack<TransformInfo>();
    private Vector3 m_currentDirection = Vector3.up;

    private LineRenderer m_lineRenderer;
    private List<Vector3> m_positions = new List<Vector3>();

    public void GenerateInEditor()
    {
        m_rules.Clear();
        foreach (Rule rule in m_rulesList)
        {
            if (!m_rules.ContainsKey(rule.key))
                m_rules.Add(rule.key, rule.value);
        }

        GenerateLsystem();
        DrawLSystem();
    }

    private void GenerateLsystem()
    {
        m_currentString = m_axiom;

        for (int i = 0; i < m_iterations; i++)
        {
            string nextString = "";
            foreach (char c in m_currentString)
            {
                if (m_rules.ContainsKey(c))
                {
                    nextString += m_rules[c];
                }
                else
                {
                    nextString += c;
                }
            }

            m_currentString = nextString;
        }
    }

    private void DrawLSystem()
    {
        m_currentPosition = Vector3.zero;
        m_currentDirection = Vector3.up;
        m_positions.Clear();
        m_transformStack.Clear();

        m_positions.Add(m_currentPosition);
        Vector3 previousPosition = m_currentPosition;

        foreach (char c in m_currentString)
        {
            if (c == 'F')
            {
                m_currentPosition += m_currentDirection * m_length;
                if (m_renderMode == RenderMode.LineRenderer)
                {
                    m_positions.Add(m_currentPosition);
                }
                else if (m_renderMode == RenderMode.Mesh)
                {
                    CreateCylinderBetween(previousPosition, m_currentPosition);
                }
                previousPosition = m_currentPosition;
            }
            else if (c == '+')
            {
                m_currentDirection = Quaternion.Euler(0, 0, m_angle) * m_currentDirection;
            }
            else if (c == '-')
            {
                m_currentDirection = Quaternion.Euler(0, 0, -m_angle) * m_currentDirection;
            }
            else if (c == '[')
            {
                m_transformStack.Push(new TransformInfo { position = m_currentPosition, direction = m_currentDirection });
            }
            else if (c == ']')
            {
                TransformInfo info = m_transformStack.Pop();
                m_currentPosition = info.position;
                m_currentDirection = info.direction;
                previousPosition = m_currentPosition;
                if (m_renderMode == RenderMode.LineRenderer)
                {
                    m_positions.Add(m_currentPosition);
                }
            }
        }

        if (m_renderMode == RenderMode.LineRenderer)
        {
            if (m_lineRenderer == null)
                m_lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

            m_lineRenderer.positionCount = m_positions.Count;
            m_lineRenderer.SetPositions(m_positions.ToArray());
            m_lineRenderer.startWidth = 0.1f;
            m_lineRenderer.endWidth = 0.1f;
            m_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            m_lineRenderer.startColor = Color.white;
            m_lineRenderer.endColor = Color.white;
        }
        else if (m_lineRenderer != null)
        {
            DestroyImmediate(m_lineRenderer);
        }
    }

    private void CreateCylinderBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;

        if (length == 0) return;

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = start + (direction / 2.0f);
        cylinder.transform.up = direction.normalized;
        cylinder.transform.localScale = new Vector3(0.1f, length / 2.0f, 0.1f);
        cylinder.transform.parent = this.transform;
    }

    private struct TransformInfo
    {
        public Vector3 position;
        public Vector3 direction;
    }

    [System.Serializable]
    public class Rule
    {
        public char key;
        public string value;
    }

    public enum RenderMode
    {
        LineRenderer,
        Mesh
    }
}
