using System.Collections.Generic;
using UnityEngine;

public static class LSystemRenderer
{
    public struct Segment
    {
        public Vector3 start;
        public Vector3 end;
        public float width;
        public Material material;
    }

    public static void DrawMeshes(List<Segment> segments, Transform parent)
    {
        foreach (var s in segments)
        {
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.position = s.start + (s.end - s.start) / 2f;
            cyl.transform.up = (s.end - s.start).normalized;
            cyl.transform.localScale = new Vector3(s.width, (s.end - s.start).magnitude / 2f, s.width);
            cyl.transform.parent = parent;

            if (s.material != null)
                cyl.GetComponent<Renderer>().material = s.material;
        }
    }

    public static void DrawLines(List<Segment> segments, Transform parent)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var go = new GameObject($"LineSegment_{i}");
            go.transform.parent = parent;
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, segments[i].start);
            lr.SetPosition(1, segments[i].end);
            lr.startWidth = lr.endWidth = segments[i].width;
            lr.useWorldSpace = true;
            lr.material = segments[i].material ?? new Material(Shader.Find("Sprites/Default"));
        }
    }
}
