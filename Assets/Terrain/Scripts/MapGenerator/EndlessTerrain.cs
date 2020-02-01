using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    private const float chunkUpdateDistThreshold = 25f;
    private const float sqrChunkUpdateDistThreshold = chunkUpdateDistThreshold * chunkUpdateDistThreshold;

    public static float maxViewDist = 450;
    public static Vector2 viewerPosition;

    public Transform viewer;
    public Material mapMaterial;

    public LODInfo[] detailLevels;

    private int chunkSize;
    private int chunksVisibleInViewDist;

    private Vector2 viewerPositionOld;
    private static MapGenerator mapGenerator;

    private Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> chunksVisibleAtLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrChunkUpdateDistThreshold)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        foreach (TerrainChunk chunk in chunksVisibleAtLastUpdate)
        {
            chunk.Visible = false;
        }

        chunksVisibleAtLastUpdate.Clear();

        int currenChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currenChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++)
        {
            for (int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currenChunkCoordX + xOffset, currenChunkCoordY + yOffset);

                if (terrainChunks.ContainsKey(viewedChunkCoord))
                {
                    terrainChunks[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        public bool Visible
        {
            get
            {
                return meshObject.activeSelf;
            }
            set
            {
                meshObject.SetActive(value);
            }
        }

        private bool mapDataReceived;
        private int prevLODIndex = -1;

        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private MapData mapData;

        private LODInfo[] detailLevels;
        private LODMesh[] meshes;

        public TerrainChunk(Vector2 coords, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coords * size;
            bounds = new Bounds(position, Vector2.one * size);

            meshObject = new GameObject("Terrain Chunk");
            meshObject.transform.position = new Vector3(position.x, 0, position.y) * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            Visible = false;

            mapGenerator.RequestMapData(position, OnMapDataReceived);

            meshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                Visible = viewerDistFromNearestEdge <= maxViewDist;

                if (Visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length; i++)
                    {
                        if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
                            lodIndex = i + 1;
                        else
                            break;
                    }

                    if (lodIndex != prevLODIndex)
                    {
                        LODMesh lodMesh = meshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            prevLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                            lodMesh.RequestMesh(mapData);
                    }

                    chunksVisibleAtLastUpdate.Add(this);
                }
            }
        }
    }

    private class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;

        private System.Action updateCallback;

        public LODMesh(int _lod, System.Action _updateCallback)
        {
            lod = _lod;
            updateCallback = _updateCallback;
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistThreshold;
    }
}