using System.Collections.Generic;
using UnityEngine;

public struct TurtleState
{
    public Vector3 position;
    public Vector3 direction;
    public float length;
    public float width;
    public Material material;
}

public class LSystemInterpreter
{
    private readonly float angle, widthScale, lengthScale;
    private readonly List<Material> materials;

    public LSystemInterpreter(float angle, float widthScale, float lengthScale, List<Material> materials)
    {
        this.angle = angle;
        this.widthScale = widthScale;
        this.lengthScale = lengthScale;
        this.materials = materials;
    }

    public List<LSystemRenderer.Segment> Interpret(List<LSystemParser.SymbolToken> tokens, Vector3 startDir, float initLength, float initWidth, Material defaultMat)
    {
        var segments = new List<LSystemRenderer.Segment>();
        var stack = new Stack<TurtleState>();

        Vector3 pos = Vector3.zero, dir = startDir.normalized, prev = Vector3.zero;
        float len = initLength, width = initWidth;
        Material mat = defaultMat;

        foreach (var token in tokens)
        {
            char c = token.symbol;
            float? p = token.parameter;

            switch (c)
            {
                case 'F':
                    float l = p ?? len;
                    Vector3 next = pos + dir * l;
                    segments.Add(new LSystemRenderer.Segment { start = pos, end = next, width = width, material = mat });
                    prev = pos = next;
                    break;

                case '+': dir = Quaternion.Euler(0, 0, p ?? angle) * dir; break;
                case '-': dir = Quaternion.Euler(0, 0, -(p ?? angle)) * dir; break;
                case '&': dir = Quaternion.Euler(p ?? angle, 0, 0) * dir; break;
                case '^': dir = Quaternion.Euler(-(p ?? angle), 0, 0) * dir; break;
                case '/': dir = Quaternion.Euler(0, p ?? angle, 0) * dir; break;
                case '\\': dir = Quaternion.Euler(0, -(p ?? angle), 0) * dir; break;

                case '!': width *= widthScale; break;
                case '?': width /= widthScale; break;
                case '\"': len *= lengthScale; break;
                case '_': len /= lengthScale; break;

                case '[':
                    stack.Push(new TurtleState { position = pos, direction = dir, length = len, width = width, material = mat });
                    break;

                case ']':
                    var s = stack.Pop();
                    pos = s.position; dir = s.direction; len = s.length; width = s.width; mat = s.material;
                    prev = pos;
                    break;

                case 'M':
                    int idx = p.HasValue ? Mathf.FloorToInt(p.Value) : 0;
                    if (idx >= 0 && idx < materials.Count) mat = materials[idx];
                    break;
            }
        }

        return segments;
    }
}
