using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class DelayedPipelineInitiator : MonoBehaviour
{
    [SerializeField]
    private float segmentLength = 5f;
    [SerializeField]
    [Range(15f, 30f)]
    private float generatePipelineRange = 15f;
    [SerializeField]
    [Range(0.1f, 1f)]
    private float delay;

    string savePath;
    private PipelineMeshGenerator pipeMesh;
    private List<Vector3> allPipelinePoints;


    void Start()
    {
        Init();
    }

    void Init()
    {
        allPipelinePoints = new List<Vector3>();
        allPipelinePoints.Add(Vector3.zero);

        savePath = Application.dataPath + "/../Data/PipelineData/PipeData.dat";

        pipeMesh = GetComponent<PipelineMeshGenerator>();
        pipeMesh.PipeMaterial = RandomizeMaterial();

        LoadSavedPipe();

        StartCoroutine(DelayedGenerate());
    }

    IEnumerator DelayedGenerate()
    {
        Vector3 targetPos = allPipelinePoints[allPipelinePoints.Count - 1];

        int i = 0;

        while (true)
        {
            if (GetNextPoint(ref targetPos))
            {
                AddSector(targetPos);

                SavePipe();

                i++;

                yield return new WaitForSeconds(delay);
            }
            else
            {
                Restart();
                yield break;
            }
        }
    }

    void Restart()
    {
        allPipelinePoints.Clear();
        allPipelinePoints.Add(Vector3.zero);

        pipeMesh.NonColinearPipePoints.Clear();
        pipeMesh.PipeMaterial = RandomizeMaterial();

        StartCoroutine(DelayedGenerate());
    }

    void AddSector(Vector3 targetPos)
    {
        pipeMesh.NonColinearPipePoints.Add(targetPos);
        allPipelinePoints.Add(targetPos);

        pipeMesh.GeneratePipe();
    }

    bool GetNextPoint(ref Vector3 currentPoint, int recurse = 0)
    {
        recurse++;

        Vector3 direction = RandomUtils.GetRandomBasisDirection();
        Vector3 nextPoint = currentPoint + direction * segmentLength;

        if (!allPipelinePoints.Contains(nextPoint) && Vector3.Distance(nextPoint, Vector3.zero) < generatePipelineRange)
        {
            currentPoint = nextPoint;
            return true;
        }
        else if (recurse < 100)
        {
            return GetNextPoint(ref currentPoint, recurse);
        }
        else
        {
            return false;
        }
    }

    void LoadSavedPipe()
    {
        Vector3[] tempPoints;

        if (DataUtils.LoadVector3Array(out tempPoints, savePath))
        {
            allPipelinePoints = tempPoints.ToList();
        }

        pipeMesh.NonColinearPipePoints = new List<Vector3>(allPipelinePoints);
    }

    void SavePipe()
    {
        DataUtils.SaveVector3Array(allPipelinePoints.ToArray(), savePath);
    }

    private Material RandomizeMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), 1);

        return material;
    }
}
