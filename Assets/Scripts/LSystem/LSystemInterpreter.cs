using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the current state of the turtle at any point in the L-System interpretation.
/// </summary>
public struct TurtleState
{
    public Vector3 position;     // Current turtle position
    public Vector3 direction;    // Current turtle heading
    public float length;         // Current step length
    public float width;          // Current line width
    public Material material;    // Current material used for drawing
}

/// <summary>
/// Converts parsed L-System tokens into drawable segments using turtle graphics.
/// </summary>
public class LSystemInterpreter
{
    private readonly float angle;
    private readonly float widthScale;
    private readonly float lengthScale;
    private readonly List<Material> materials;

    public LSystemInterpreter(float angle, float widthScale, float lengthScale, List<Material> materials)
    {
        this.angle = angle;
        this.widthScale = widthScale;
        this.lengthScale = lengthScale;
        this.materials = materials;
    }

    /// <summary>
    /// Interprets a list of L-System tokens into drawable segments.
    /// </summary>
    /// <param name="tokens">List of parsed tokens</param>
    /// <param name="startDir">Initial direction (usually Vector3.up)</param>
    /// <param name="initLength">Initial forward movement distance</param>
    /// <param name="initWidth">Initial drawing width</param>
    /// <param name="defaultMat">Fallback material if no M(index) is specified</param>
    /// <returns>List of segments to be rendered</returns>
    public List<LSystemRenderer.Segment> Interpret(
        List<LSystemParser.SymbolToken> tokens,
        Vector3 startDir,
        float initLength,
        float initWidth,
        Material defaultMat)
    {
        var segments = new List<LSystemRenderer.Segment>();
        var stack = new Stack<TurtleState>();

        Vector3 pos = Vector3.zero;
        Vector3 dir = startDir.normalized;

        float length = initLength;
        float width = initWidth;
        Material material = defaultMat;

        foreach (var token in tokens)
        {
            char symbol = token.symbol;
            float? param = token.parameter;

            switch (symbol)
            {
                // Forward: draw a segment from current position forward.
                case 'F':
                    float step = param ?? length;
                    Vector3 nextPos = pos + dir * step;

                    segments.Add(new LSystemRenderer.Segment
                    {
                        start = pos,
                        end = nextPos,
                        width = width,
                        material = material
                    });

                    pos = nextPos;
                    break;

                // Rotations around different axes.
                case '+': dir = Quaternion.Euler(0, 0, param ?? angle) * dir; break;       // Yaw +
                case '-': dir = Quaternion.Euler(0, 0, -(param ?? angle)) * dir; break;    // Yaw -
                case '&': dir = Quaternion.Euler(param ?? angle, 0, 0) * dir; break;       // Pitch +
                case '^': dir = Quaternion.Euler(-(param ?? angle), 0, 0) * dir; break;    // Pitch -
                case '/': dir = Quaternion.Euler(0, param ?? angle, 0) * dir; break;       // Roll +
                case '\\': dir = Quaternion.Euler(0, -(param ?? angle), 0) * dir; break;   // Roll -

                // Width and length scaling.
                case '!': width *= widthScale; break;
                case '?': width /= widthScale; break;
                case '"': length *= lengthScale; break;
                case '_': length /= lengthScale; break;

                // Save state (branch start).
                case '[':
                    stack.Push(new TurtleState
                    {
                        position = pos,
                        direction = dir,
                        length = length,
                        width = width,
                        material = material
                    });
                    break;

                // Restore state (branch end).
                case ']':
                    if (stack.Count > 0)
                    {
                        var state = stack.Pop();
                        pos = state.position;
                        dir = state.direction;
                        length = state.length;
                        width = state.width;
                        material = state.material;
                    }
                    break;

                // Material override.
                case 'M':
                    int index = param.HasValue ? Mathf.FloorToInt(param.Value) : 0;
                    if (index >= 0 && index < materials.Count)
                        material = materials[index];
                    break;
            }

            // Optional: normalize direction to prevent drift over many rotations.
            dir = dir.normalized;
        }

        return segments;
    }
}
