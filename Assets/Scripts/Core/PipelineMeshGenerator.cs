using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipelineMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private bool generateElbows = true;
    [SerializeField]
    private float pipeRadius = 1f , elbowRadius = 1f;

    private float colinearThreshold = 0.001f;

    private int pipeSegments = 8;
    private int elbowSegments = 6;


    public Material PipeMaterial { get; set; }
    public List<Vector3> NonColinearPipePoints { get; set; }


    public void GeneratePipe()
    {
        if (NonColinearPipePoints.Count < 2)
        {
            return;
        }

        RemoveColinearPoints();

        MeshFilter currentMeshFilter = GetComponent<MeshFilter>();
        MeshFilter mf = currentMeshFilter != null ? currentMeshFilter : gameObject.AddComponent<MeshFilter>();

        Mesh mesh = GenerateMesh();

        mf.mesh = mesh;

        MeshRenderer currentMeshRenderer = GetComponent<MeshRenderer>();
        MeshRenderer mr = currentMeshRenderer != null ? currentMeshRenderer : gameObject.AddComponent<MeshRenderer>();
        mr.materials = new Material[1] { PipeMaterial };
    }

    void RemoveColinearPoints()
    {
        List<int> pointsToRemove = new List<int>();

        for (int i = 0; i < NonColinearPipePoints.Count - 2; i++)
        {
            Vector3 point1 = NonColinearPipePoints[i];
            Vector3 point2 = NonColinearPipePoints[i + 1];
            Vector3 point3 = NonColinearPipePoints[i + 2];

            Vector3 dir1 = point2 - point1;
            Vector3 dir2 = point3 - point2;

            if (Vector3.Distance(dir1.normalized, dir2.normalized) < colinearThreshold)
            {
                pointsToRemove.Add(i + 1);
            }
        }

        pointsToRemove.Reverse();

        foreach (int idx in pointsToRemove)
        {
            NonColinearPipePoints.RemoveAt(idx);
        }
    }

    Mesh GenerateMesh()
    {
        Mesh m = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < NonColinearPipePoints.Count - 1; i++)
        {
            GenerateCylinder(i, vertices, normals, triangles);
        }

        if (generateElbows)
        {
            for (int i = 0; i < NonColinearPipePoints.Count - 2; i++)
            {
                Vector3 point1 = NonColinearPipePoints[i]; 
                Vector3 point2 = NonColinearPipePoints[i + 1];
                Vector3 point3 = NonColinearPipePoints[i + 2];
                GenerateElbow(i, vertices, normals, triangles, point1, point2, point3);
            }
        }

        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetNormals(normals);

        return m;
    }

    void GenerateCylinder(int index ,List<Vector3> vertices, List<Vector3> normals, List<int> triangles)
    {
        Vector3 initialPoint = NonColinearPipePoints[index];
        Vector3 endPoint = NonColinearPipePoints[index + 1];
        Vector3 direction = (NonColinearPipePoints[index + 1] - NonColinearPipePoints[index]).normalized;

        if (index > 0 && generateElbows)
        {
            initialPoint = initialPoint + direction * elbowRadius;
        }

        if (index < NonColinearPipePoints.Count - 2 && generateElbows)
        {
            endPoint = endPoint - direction * elbowRadius;
        }

        GenerateCircleAtPoint(vertices, normals, initialPoint, direction);
        GenerateCircleAtPoint(vertices, normals, endPoint, direction);
        MakeCylinderTriangles(triangles, index);
    }

    void MakeCylinderTriangles(List<int> triangles, int segmentIdx)
    {
        int offset = segmentIdx * pipeSegments * 2;
        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + i + pipeSegments);
            triangles.Add(offset + i);

            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + (i + 1) % pipeSegments + pipeSegments);
            triangles.Add(offset + i + pipeSegments);
        }
    }

    void GenerateElbow(int index, List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 point1, Vector3 point2, Vector3 point3)
    {

        Vector3 offset1 = (point2 - point1).normalized * elbowRadius;
        Vector3 offset2 = (point3 - point2).normalized * elbowRadius;
        Vector3 startPoint = point2 - offset1;
        Vector3 endPoint = point2 + offset2;

        Vector3 perpendicularToBoth = Vector3.Cross(offset1, offset2);
        Vector3 startDir = Vector3.Cross(perpendicularToBoth, offset1).normalized;
        Vector3 endDir = Vector3.Cross(perpendicularToBoth, offset2).normalized;

        Vector3 torusCenter1;
        Vector3 torusCenter2;
        Math3D.ClosestPointsOnTwoLines(out torusCenter1, out torusCenter2, startPoint, startDir, endPoint, endDir);
        Vector3 torusCenter = 0.5f * (torusCenter1 + torusCenter2);


        float actualTorusRadius = (torusCenter - startPoint).magnitude;

        float angle = Vector3.Angle(startPoint - torusCenter, endPoint - torusCenter);
        float radiansPerSegment = (angle * Mathf.Deg2Rad) / elbowSegments;
        Vector3 lastPoint = point2 - startPoint;

        for (int i = 0; i <= elbowSegments; i++)
        {
            Vector3 xAxis = (startPoint - torusCenter).normalized;
            Vector3 yAxis = (endPoint - torusCenter).normalized;
            Vector3.OrthoNormalize(ref xAxis, ref yAxis);

            Vector3 circleCenter = torusCenter +
                (actualTorusRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (actualTorusRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);

            Vector3 direction = circleCenter - lastPoint;
            lastPoint = circleCenter;

            if (i == elbowSegments)
            {
                direction = endPoint - point2;
            }
            else if (i == 0)
            {
                direction = point2 - startPoint;
            }

            GenerateCircleAtPoint(vertices, normals, circleCenter, direction);

            if (i > 0)
            {
                MakeElbowTriangles(vertices, triangles, i, index);
            }
        }
    }

    void GenerateCircleAtPoint(List<Vector3> vertices, List<Vector3> normals, Vector3 center, Vector3 direction)
    {
        float twoPi = Mathf.PI * 2;
        float radiansPerSegment = twoPi / pipeSegments;

        Plane p = new Plane(Vector3.forward, Vector3.zero);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;

        if (p.GetSide(direction))
        {
            yAxis = Vector3.left;
        }

        Vector3.OrthoNormalize(ref direction, ref xAxis, ref yAxis);

        for (int i = 0; i < pipeSegments; i++)
        {
            Vector3 currentVertex =
                center +
                (pipeRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (pipeRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices.Add(currentVertex);
            normals.Add((currentVertex - center).normalized);
        }
    }


    void MakeElbowTriangles(List<Vector3> vertices, List<int> triangles, int segmentIdx, int elbowIdx)
    {
        int offset = (NonColinearPipePoints.Count - 1) * pipeSegments * 2;
        offset += elbowIdx * (elbowSegments + 1) * pipeSegments;
        offset += segmentIdx * pipeSegments;

        Dictionary<int, int> mapping = new Dictionary<int, int>();

        List<Vector3> thisRingVertices = new List<Vector3>();
        List<Vector3> lastRingVertices = new List<Vector3>();

        for (int i = 0; i < pipeSegments; i++)
        {
            lastRingVertices.Add(vertices[offset + i - pipeSegments]);
        }

        for (int i = 0; i < pipeSegments; i++)
        {
            Vector3 minDistVertex = Vector3.zero;
            float minDist = Mathf.Infinity;
            for (int j = 0; j < pipeSegments; j++)
            {
                Vector3 currentVertex = vertices[offset + j];
                float distance = Vector3.Distance(lastRingVertices[i], currentVertex);
                if (distance < minDist)
                {
                    minDist = distance;
                    minDistVertex = currentVertex;
                }
            }
            thisRingVertices.Add(minDistVertex);
            mapping.Add(i, vertices.IndexOf(minDistVertex));
        }

        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(mapping[i]);
            triangles.Add(offset + i - pipeSegments);
            triangles.Add(mapping[(i + 1) % pipeSegments]);

            triangles.Add(offset + i - pipeSegments);
            triangles.Add(offset + (i + 1) % pipeSegments - pipeSegments);
            triangles.Add(mapping[(i + 1) % pipeSegments]);
        }
    }
}