using System.Collections.Generic;
using UnityEngine;

public class LSystemTurtle : MonoBehaviour
{
    [SerializeField] private RenderModeType m_renderMode = RenderModeType.LineRenderer;

    [SerializeField] public string m_axiom = "F";

    [Range(0, 10)]
    [SerializeField] public int m_iterations = 4;

    [Range(0.0f, 360.0f)]
    [SerializeField] public float m_angle = 25.0f;

    [Range(0.1f, 5.0f)]
    [SerializeField] public float m_length = 1.0f;

    [SerializeField] private List<Rule> m_rulesList = new List<Rule>();

    [SerializeField] private bool m_autoUpdate = false;

    private Dictionary<char, string> m_rules = new Dictionary<char, string>();

    private Stack<TransformInfo> m_transformStack = new Stack<TransformInfo>();
    private Vector3 m_currentDirection = Vector3.up;

    private LineRenderer m_lineRenderer;
    private List<Vector3> m_positions = new List<Vector3>();

    private GameObject m_meshParent;

    public void GenerateInEditor()
    {
        m_rules.Clear();
        foreach (Rule rule in m_rulesList)
        {
            if (!string.IsNullOrEmpty(rule.value) && rule.key != '\0' && rule.key != ' ')
            {
                if (!m_rules.ContainsKey(rule.key))
                    m_rules.Add(rule.key, rule.value);
            }
        }

        string lsystem = GenerateLSystemString();
        DrawLSystem(lsystem);
    }

    private string GenerateLSystemString()
    {
        string lsystem = m_axiom;

        for (int i = 0; i < m_iterations; i++)
        {
            string nextString = "";
            foreach (char c in lsystem)
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

            lsystem = nextString;
        }

        return lsystem;
    }

    private void DrawLSystem(string lsystem)
    {
        Vector3 currentPosition = Vector3.zero;
        m_currentDirection = Vector3.up;
        m_positions.Clear();
        m_transformStack.Clear();

        m_positions.Add(currentPosition);
        Vector3 previousPosition = currentPosition;
        List<(Vector3, Vector3)> segments = new List<(Vector3, Vector3)>();

        foreach (char c in lsystem)
        {
            if (c == 'F')
            {
                currentPosition += m_currentDirection * m_length;
                segments.Add((previousPosition, currentPosition));
                previousPosition = currentPosition;
            }
            else if (c == '+')
            {
                m_currentDirection = Quaternion.Euler(0, 0, m_angle) * m_currentDirection;
            }
            else if (c == '-')
            {
                m_currentDirection = Quaternion.Euler(0, 0, -m_angle) * m_currentDirection;
            }
            else if (c == '&')
            {
                m_currentDirection = Quaternion.Euler(m_angle, 0, 0) * m_currentDirection;
            }
            else if (c == '^')
            {
                m_currentDirection = Quaternion.Euler(-m_angle, 0, 0) * m_currentDirection;
            }
            else if (c == '/')
            {
                m_currentDirection = Quaternion.Euler(0, m_angle, 0) * m_currentDirection;
            }
            else if (c == '\\')
            {
                m_currentDirection = Quaternion.Euler(0, -m_angle, 0) * m_currentDirection;
            }
            else if (c == '[')
            {
                m_transformStack.Push(new TransformInfo { position = currentPosition, direction = m_currentDirection });
            }
            else if (c == ']')
            {
                TransformInfo info = m_transformStack.Pop();
                currentPosition = info.position;
                m_currentDirection = info.direction;
                previousPosition = currentPosition;
            }
        }

        if (m_renderMode == RenderModeType.LineRenderer)
        {
            DrawLines(segments);
        }
        else
        {
            DrawMesh(segments);
        }
    }

    private void DrawLines(List<(Vector3 start, Vector3 end)> segments)
    {
        List<Vector3> positions = new List<Vector3>();

        positions.Add(segments[0].start);
        foreach (var segment in segments)
        {
            positions.Add(segment.end);
        }

        m_lineRenderer.positionCount = positions.Count;
        m_lineRenderer.SetPositions(positions.ToArray());
        m_lineRenderer.startWidth = 0.1f;
        m_lineRenderer.endWidth = 0.1f;
        m_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        m_lineRenderer.startColor = Color.white;
        m_lineRenderer.endColor = Color.white;
    }

    private void DrawMesh(List<(Vector3 start, Vector3 end)> segments)
    {
        foreach (var segment in segments)
        {
            CreateCylinderBetween(segment.start, segment.end);
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
        cylinder.transform.parent = m_meshParent != null ? m_meshParent.transform : this.transform;
    }

    public void EnsureLineRendererIfNeeded()
    {
        if (m_renderMode == RenderModeType.LineRenderer)
        {
            if (m_lineRenderer == null)
            {
                m_lineRenderer = GetComponent<LineRenderer>();
                if (m_lineRenderer == null)
                    m_lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            ClearMeshObjects();
            if (m_meshParent != null)
            {
                DestroyImmediate(m_meshParent);
                m_meshParent = null;
            }
        }
        else
        {
            if (m_lineRenderer != null)
            {
                DestroyImmediate(m_lineRenderer);
                m_lineRenderer = null;
            }

            if (m_meshParent == null)
            {
                m_meshParent = new GameObject("MeshParent");
                m_meshParent.transform.parent = this.transform;
                m_meshParent.transform.localPosition = Vector3.zero;
                m_meshParent.transform.localRotation = Quaternion.identity;
                m_meshParent.transform.localScale = Vector3.one;
            }

            ClearMeshObjects();
        }
    }

    private void ClearMeshObjects()
    {
        if (m_meshParent == null) return;

        int childCount = m_meshParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = m_meshParent.transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    private struct TransformInfo
    {
        public Vector3 position;
        public Vector3 direction;
    }

    [System.Serializable]
    public class Rule
    {
        [SerializeField]
        private string _key = "F";

        public char key
        {
            get
            {
                if (!string.IsNullOrEmpty(_key) && _key.Length > 0)
                    return _key[0];
                else
                    return '\0';
            }
            set
            {
                _key = value.ToString();
            }
        }

        public string value;

        public void ValidateKey()
        {
            if (!string.IsNullOrEmpty(_key) && _key.Length > 1)
            {
                _key = _key[0].ToString();
                Debug.LogWarning("Rule key reset to a single character: " + _key);
            }
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
