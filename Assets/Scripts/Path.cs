using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private float lineRendererOffset = 0.05f;

    [SerializeField]
    private GameObject[] pathPoints;

    private Vector3[] pathPositions;

    private void Awake()
    {
        if (pathPositions == null)
        {
            RefreshPathPositions();
        }
    }

    private void OnDrawGizmos()
    {
        if(pathPoints == null) { return; }

        Gizmos.color = lineRenderer ? lineRenderer.startColor : Color.red;
        for(int i = 1; i < pathPoints.Length; ++i)
        {
            Gizmos.DrawLine(pathPoints[i - 1].transform.position, pathPoints[i].transform.position);
        }
    }

    private void OnValidate()
    {
        ForcePathPointsToZeroHeight();
    }

    private void ForcePathPointsToZeroHeight()
    {
        if(pathPoints == null) { return; }
             
        for (int i = 0; i < pathPoints.Length; ++i)
        {
            pathPoints[i].transform.position =
                new Vector3(pathPoints[i].transform.position.x, 0.0f, pathPoints[i].transform.position.z);
        }
    }

    public Vector3[] RefreshPathPositions()
    {
        ForcePathPointsToZeroHeight();

        pathPositions = new Vector3[pathPoints.Length];
        var lineRendererPositions = new Vector3[pathPoints.Length];

        for (int i = 0; i < pathPoints.Length; ++i)
        {
            pathPositions[i] = pathPoints[i].transform.position;
            lineRendererPositions[i] = pathPositions[i] + new Vector3(0, lineRendererOffset, 0);
        }

        lineRenderer.positionCount = lineRendererPositions.Length;
        lineRenderer.SetPositions(lineRendererPositions);

        return pathPositions;
    }

    /// <summary>
    /// Returns world coordinates of points along the path
    /// </summary>
    public Vector3[] GetPathVertices()
    {
        return pathPositions ?? (pathPositions = RefreshPathPositions());
    }
}
