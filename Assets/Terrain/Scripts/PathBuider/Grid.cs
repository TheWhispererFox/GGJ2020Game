using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask unwalkableLayerMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public bool displayGizmos;
    public MapGenerator mapGenerator;
    public float gridHeightOffset;

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public Node[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private void Awake()
    {
        //mapGenerator = FindObjectOfType<MapGenerator>();
        //CreateGrid();
    }

    private void CreateGrid()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableLayerMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public bool CreateTerrainGrid(MapGenerator _mapGenerator)
    {
        mapGenerator = _mapGenerator;
        float scale = mapGenerator.terrainData.uniformScale;
        gridWorldSize = new Vector2(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize) * scale;
        int simplification = mapGenerator.editorLevelOfSimplification == 0 ? 1 : mapGenerator.editorLevelOfSimplification * 2;
        gridSizeX = gridSizeY = (MapGenerator.mapChunkSize - 1) / simplification + 1;
        grid = new Node[gridSizeX, gridSizeY];
        float topLeftX = (MapGenerator.mapChunkSize - 1) / -2f * scale;
        float topLeftZ = (MapGenerator.mapChunkSize - 1) / 2f * scale;

        for (int x = 0; x < MapGenerator.mapChunkSize; x += simplification)
        {
            for (int y = 0; y < MapGenerator.mapChunkSize; y += simplification)
            {
                Vector3 worldPoint = new Vector3(topLeftX + x * scale, transform.position.y + gridHeightOffset, topLeftZ - y * scale);
                float heightValue = mapGenerator.terrainData.meshHeightCurve.Evaluate(mapGenerator.map.heightMap[x, y]);
                bool walkable = (heightValue >= MapGenerator.minGroundHeight && heightValue <= MapGenerator.maxGroundHeight);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }

        nodeRadius = simplification / 2f;
        nodeDiameter = simplification;
        return true;
    }

    public Node GetNearestWalkableNode(Node node)
    {
        int distX = gridSizeX - node.gridX;
        int distY = gridSizeY - node.gridY;
        if (distX > node.gridX)
            distX = node.gridX;
        if (distY > node.gridY)
            distY = node.gridY;
        int dist = Mathf.Min(distX, distY);

        for (int radius = 1; radius < dist; radius++)
        {
            List<Node> walkableNodes = new List<Node>();

            int j = node.gridY - radius;
            if (j < 0)
                j = 0;

            for (int i = node.gridX - radius; i <= node.gridX + radius; i++)
            {
                if (i < 0 || i > gridSizeX)
                    continue;

                if (grid[i, j].walkable)
                    walkableNodes.Add(grid[i, j]);
            }

            j = node.gridY + radius;
            if (j >= gridSizeY)
                j = gridSizeY - 1;

            for (int i = node.gridX - radius; i <= node.gridX + radius; i++)
            {
                if (i < 0 || i > gridSizeX)
                    continue;

                if (grid[i, j].walkable)
                    walkableNodes.Add(grid[i, j]);
            }

            j = node.gridX - radius;
            if (j < 0)
                j = 0;

            for (int i = node.gridY - radius; i <= node.gridY + radius; i++)
            {
                if (i < 0 || i > gridSizeX)
                    continue;

                if (grid[j, i].walkable)
                    walkableNodes.Add(grid[j, i]);
            }

            j = node.gridX + radius;
            if (j >= gridSizeY)
                j = gridSizeY - 1;

            for (int i = node.gridY - radius; i <= node.gridY + radius; i++)
            {
                if (i < 0 || i > gridSizeX)
                    continue;

                if (grid[j, i].walkable)
                    walkableNodes.Add(grid[j, i]);
            }

            if (walkableNodes.Count > 0)
            {
                Node result = walkableNodes.OrderBy(n => (n.gridX - node.gridX) + (n.gridY - node.gridY)).First();
                return result;
            }
        }
        return null;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX + x >= 0 && checkX < gridSizeX && checkY + y >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 position)
    {
        float percentX = (position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (position.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, gridSizeY - y - 1];
    }

    public void SetUnwalkable(int x, int y, int radius)
    {
        for (int i = x - radius; i <= x + radius; i++)
        {
            for (int j = y - radius; j <= y + radius; j++)
            {
                if (i >= 0 && i < gridSizeX && j >= 0 && j < gridSizeY)
                {
                    grid[i, j].walkable = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.white;
                if (!n.walkable)
                    Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.01f) + Vector3.up * 100f);
                //Gizmos.color = n.walkable ? Color.white : Color.red;
            }
        }
    }
}
