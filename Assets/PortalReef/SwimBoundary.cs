using UnityEngine;

public class SwimBoundary : MonoBehaviour
{
    [SerializeField] private Vector3 size = new Vector3(10f, 5f, 10f);
    [SerializeField, Min(0.01f)] private float wallThickness = 0.25f;
    [SerializeField, Min(0.001f)] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = new Color(0f, 0.8f, 1f, 1f);
    [SerializeField] private BoxCollider leftWall;
    [SerializeField] private BoxCollider rightWall;
    [SerializeField] private BoxCollider bottomWall;
    [SerializeField] private BoxCollider topWall;
    [SerializeField] private BoxCollider backWall;
    [SerializeField] private BoxCollider frontWall;

    private static readonly int[,] EdgeIndices =
    {
        { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 },
        { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 },
        { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
    };

    private void Awake()
    {
        UpdateColliderWalls();
        CreateVisibleEdges();
    }

    private void OnValidate()
    {
        UpdateColliderWalls();
    }

    private void UpdateColliderWalls()
    {
        ConfigureWall(leftWall, new Vector3(-size.x * 0.5f, 0f, 0f),
            new Vector3(wallThickness, size.y, size.z));
        ConfigureWall(rightWall, new Vector3(size.x * 0.5f, 0f, 0f),
            new Vector3(wallThickness, size.y, size.z));
        ConfigureWall(bottomWall, new Vector3(0f, -size.y * 0.5f, 0f),
            new Vector3(size.x, wallThickness, size.z));
        ConfigureWall(topWall, new Vector3(0f, size.y * 0.5f, 0f),
            new Vector3(size.x, wallThickness, size.z));
        ConfigureWall(backWall, new Vector3(0f, 0f, -size.z * 0.5f),
            new Vector3(size.x, size.y, wallThickness));
        ConfigureWall(frontWall, new Vector3(0f, 0f, size.z * 0.5f),
            new Vector3(size.x, size.y, wallThickness));
    }

    private static void ConfigureWall(BoxCollider wall, Vector3 center, Vector3 wallSize)
    {
        if (wall == null)
            return;

        wall.transform.localPosition = center;
        wall.size = wallSize;
    }

    private void CreateVisibleEdges()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            Debug.LogError("[ 🧱 SwimBoundary.Awake ] URP Unlit shader was not found.");
            return;
        }

        Material lineMaterial = new Material(shader);
        lineMaterial.color = lineColor;

        Vector3 half = size * 0.5f;
        Vector3[] corners =
        {
            new Vector3(-half.x, -half.y, -half.z),
            new Vector3( half.x, -half.y, -half.z),
            new Vector3( half.x, -half.y,  half.z),
            new Vector3(-half.x, -half.y,  half.z),
            new Vector3(-half.x,  half.y, -half.z),
            new Vector3( half.x,  half.y, -half.z),
            new Vector3( half.x,  half.y,  half.z),
            new Vector3(-half.x,  half.y,  half.z)
        };

        for (int edgeIndex = 0; edgeIndex < EdgeIndices.GetLength(0); edgeIndex++)
        {
            GameObject edge = new GameObject($"Edge {edgeIndex + 1}");
            edge.transform.SetParent(transform, false);

            LineRenderer line = edge.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.sharedMaterial = lineMaterial;
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.SetPosition(0, corners[EdgeIndices[edgeIndex, 0]]);
            line.SetPosition(1, corners[EdgeIndices[edgeIndex, 1]]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}
