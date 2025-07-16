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

    private Dictionary<char, string> m_rules = new Dictionary<char, string>();

    private GameObject m_meshParent;
    private GameObject m_lineParent;

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
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < m_iterations; i++)
        {
            builder.Clear();

            foreach (char c in lsystem)
            {
                if (m_rules.TryGetValue(c, out string replacement))
                    builder.Append(replacement);
                else
                    builder.Append(c);
            }

            lsystem = builder.ToString();
        }

        return lsystem;
    }

    private List<SymbolToken> ParseLSystemString(string input)
    {
        List<SymbolToken> tokens = new List<SymbolToken>();
        int i = 0;

        while (i < input.Length)
        {
            char c = input[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (char.IsLetterOrDigit(c) || "+-&^/\\[]!\"?_".Contains(c))
            {
                float? param = null;

                if (i + 1 < input.Length && input[i + 1] == '(')
                {
                    int start = i + 2;
                    int end = input.IndexOf(')', start);
                    if (end > start)
                    {
                        string paramStr = input.Substring(start, end - start);
                        if (float.TryParse(paramStr, out float parsed))
                            param = parsed;
                        i = end + 1;
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }

                tokens.Add(new SymbolToken { symbol = c, parameter = param });
            }
            else
            {
                i++;
            }
        }

        return tokens;
    }

    private void DrawLSystem(string lsystem)
    {
        Vector3 currentPosition = Vector3.zero;
        Vector3 currentDirection = m_initialDirection.normalized;

        List<Segment> segments = new List<Segment>();
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();

        Vector3 previousPosition = currentPosition;

        float currentLength = m_length;
        float currentWidth = m_width;
        Material currentMaterial = m_materials != null && m_materials.Count > 0 ? m_materials[0] : null;

        foreach (var token in ParseLSystemString(lsystem))
        {
            char c = token.symbol;
            float? p = token.parameter;

            if (c == 'F')
            {
                float len = p ?? currentLength;
                currentPosition += currentDirection * len;
                segments.Add(new Segment
                {
                    start = previousPosition,
                    end = currentPosition,
                    width = currentWidth,
                    material = currentMaterial
                });

                previousPosition = currentPosition;
            }
            else if (c == '+')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(0, 0, angle) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '-')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(0, 0, -angle) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '&')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(angle, 0, 0) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '^')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(-angle, 0, 0) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '/')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(0, angle, 0) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '\\')
            {
                float angle = p ?? m_angle;
                currentDirection = Quaternion.Euler(0, -angle, 0) * currentDirection;
                currentDirection = currentDirection.normalized;
            }
            else if (c == '[')
            {
                transformStack.Push(new TransformInfo
                {
                    position = currentPosition,
                    direction = currentDirection,
                    length = currentLength,
                    width = currentWidth,
                    material = currentMaterial
                });
            }
            else if (c == ']')
            {
                TransformInfo info = transformStack.Pop();
                currentPosition = info.position;
                currentDirection = info.direction;
                currentLength = info.length;
                currentWidth = info.width;
                currentMaterial = info.material;
                previousPosition = currentPosition;
            }
            else if (c == '!')
            {
                currentWidth *= m_widthScale;
            }
            else if (c == '?')
            {
                currentWidth /= m_widthScale;
            }
            else if (c == '"')
            {
                currentLength *= m_lengthScale;
            }
            else if (c == '_')
            {
                currentLength /= m_lengthScale;
            }
            else if (c == 'M')
            {
                int index = p.HasValue ? Mathf.FloorToInt(p.Value) : 0;
                if (index >= 0 && index < m_materials.Count)
                {
                    currentMaterial = m_materials[index];
                }
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

    private void DrawLines(List<Segment> segments)
    {
        if (segments == null || segments.Count == 0)
            return;

        for (int i = 0; i < segments.Count; i++)
        {
            GameObject lineObj = new GameObject($"LineSegment_{i}");
            lineObj.transform.parent = m_lineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, segments[i].start);
            lr.SetPosition(1, segments[i].end);

            lr.startWidth = segments[i].width;
            lr.endWidth = segments[i].width;
            lr.useWorldSpace = true;
            lr.material = segments[i].material != null
                ? segments[i].material
                : new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
        }
    }

    private void DrawMesh(List<Segment> segments)
    {
        if (segments == null || segments.Count == 0)
            return;

        foreach (var segment in segments)
        {
            CreateCylinder(segment.start, segment.end, segment.width, segment.material);
        }
    }

    private void CreateCylinder(Vector3 start, Vector3 end, float width, Material material)
    {
        Vector3 direction = end - start;
        float length = direction.magnitude;

        if (length == 0) return;

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = start + (direction / 2.0f);
        cylinder.transform.up = direction.normalized;
        cylinder.transform.localScale = new Vector3(width, length / 2.0f, width);
        cylinder.transform.parent = m_meshParent != null ? m_meshParent.transform : this.transform;

        if (material != null)
        {
            Renderer renderer = cylinder.GetComponent<Renderer>();
            renderer.material = material;
        }
    }

    public void EnsureLineRendererIfNeeded()
    {
        if (m_renderMode == RenderModeType.LineRenderer)
        {
            ClearMeshObjects();
            ClearLineObjects();

            if (m_meshParent != null)
            {
                DestroyImmediate(m_meshParent);
                m_meshParent = null;
            }

            if (m_lineParent == null)
            {
                m_lineParent = new GameObject("LineParent");
                m_lineParent.transform.parent = this.transform;
                m_lineParent.transform.localPosition = Vector3.zero;
                m_lineParent.transform.localRotation = Quaternion.identity;
                m_lineParent.transform.localScale = Vector3.one;
            }
        }
        else
        {
            if (m_lineParent != null)
            {
                DestroyImmediate(m_lineParent);
                m_lineParent = null;
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
            ClearLineObjects();
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

    private void ClearLineObjects()
    {
        if (m_lineParent == null) return;

        for (int i = m_lineParent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = m_lineParent.transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    private struct TransformInfo
    {
        public Vector3 position;
        public Vector3 direction;
        public float length;
        public float width;
        public Material material;
    }

    private struct Segment
    {
        public Vector3 start;
        public Vector3 end;
        public float width;
        public Material material;
    }

    private struct SymbolToken
    {
        public char symbol;
        public float? parameter;
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
