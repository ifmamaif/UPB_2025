using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for rendering L-System segments as either cylinders or line renderers.
/// </summary>
public static class LSystemRenderer
{
    /// <summary>
    /// Represents a geometric segment of the L-System.
    /// </summary>
    public struct Segment
    {
        public Vector3 start;
        public Vector3 end;
        public float width;
        public Material material;
    }

    /// <summary>
    /// Draws each segment as 3D cylinder meshes, parented under the given transform.
    /// </summary>
    public static void DrawMeshes(List<Segment> segments, Transform parent)
    {
        foreach (var s in segments)
        {
            // Create a Unity primitive cylinder.
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            // Position it between start and end.
            Vector3 direction = s.end - s.start;
            cyl.transform.position = s.start + direction / 2f;

            // Orient along the direction vector.
            cyl.transform.up = direction.normalized;

            // Set the diameter and height.
            cyl.transform.localScale = new Vector3(s.width, (s.end - s.start).magnitude / 2f, s.width);

            // Set hierarchy.
            cyl.transform.parent = parent;

            // Assign material if available.
            if (s.material != null)
                cyl.GetComponent<Renderer>().material = s.material;
        }
    }

    /// <summary>
    /// Draws each segment using a LineRenderer component, parented under the given transform.
    /// </summary>
    public static void DrawLines(List<Segment> segments, Transform parent)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            // Create a child GameObject with a LineRenderer.
            var go = new GameObject($"LineSegment_{i}");
            go.transform.parent = parent;

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, segments[i].start);
            lr.SetPosition(1, segments[i].end);

            // Set uniform width.
            lr.startWidth = lr.endWidth = segments[i].width;

            // Enable world space rendering.
            lr.useWorldSpace = true;

            // Assign material if available.
            if (segments[i].material != null)
                lr.material = segments[i].material;
        }
    }
}
