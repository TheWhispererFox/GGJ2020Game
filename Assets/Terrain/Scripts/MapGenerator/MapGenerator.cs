using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    #region Params
    public const int mapChunkSize = 241;

    [Range(0,6)]
    public int editorLevelOfSimplification;

    public LayerMask terrainLayer;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;
    public GameObject waterObject;

    public List<GameObject> terrainObjectsToSpawn;
    public List<BuildingInfo> buildingsToSpawn;

    public bool autoUpdate;
    public MapData map;

    public const float minGroundHeight = 0.15f;
    public const float maxGroundHeight = 0.35f;

    private float[,] fallOffMap;
    private MapDisplay mapDisplay;
    private PathFinding pathFinding;
    private Grid grid;

    private Queue<MapThreadInfo<MapData>> mapDataThreadQueue = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadQueue = new Queue<MapThreadInfo<MeshData>>();
    #endregion

    public void DrawMapInEditor()
    {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(mapChunkSize, terrainData.fallOffSmoothness, terrainData.fallOffOffset);

        map = GenerateMapData(Vector2.zero);

        if (mapDisplay == null)
            mapDisplay = GetComponent<MapDisplay>();

        mapDisplay.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(
                    map.heightMap,
                    mapChunkSize,
                    terrainData.meshHeightMultiplier,
                    terrainData.meshHeightCurve,
                    editorLevelOfSimplification));
    }

    public void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (terrainData != null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
                terrainData.OnValuesUpdated -= OnTextureValuesUpdated;
                terrainData.OnValuesUpdated += OnTextureValuesUpdated;
            }

            if (noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
                noiseData.OnValuesUpdated -= OnTextureValuesUpdated;
                noiseData.OnValuesUpdated += OnTextureValuesUpdated;
            }

            if (textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    public void RequestMeshData(MapData mapData, int levelOfSimplification, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, levelOfSimplification, callback);
        };

        new Thread(threadStart).Start();
    }

    public bool CheckFreePlace(int x, int y, int radius)
    {
        if (x < 0 || x >= mapChunkSize || y < 0 || y >= mapChunkSize || radius <= 0)
            return false;
        if (x - radius < 0 || x + radius >= mapChunkSize || y - radius < 0 || y + radius >= mapChunkSize)
            return false;

        for (int i = x - radius; i < x + radius; i++)
        {
            for (int j = y - radius; j < y + radius; j++)
            {
                float heightValue = terrainData.meshHeightCurve.Evaluate(map.heightMap[i, j]);
                
                if (heightValue < minGroundHeight || heightValue > maxGroundHeight)
                    return false;
            }
        }

        return true;
    }

    public void FlatBuildArea(int x, int y, int radius)
    {
        float averageValue = 0;
        int count = 0;
        for (int i = x - radius; i < x + radius; i++)
        {
            for (int j = y - radius; j < y + radius; j++)
            {
                averageValue += map.heightMap[i, j];
                count++;
            }
        }
        averageValue /= count;

        for (int i = x - radius; i < x + radius; i++)
        {
            for (int j = y - radius; j < y + radius; j++)
            {
                map.heightMap[i, j] = averageValue;
            }
        }

        mapDisplay.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(
                    map.heightMap,
                    mapChunkSize,
                    terrainData.meshHeightMultiplier,
                    terrainData.meshHeightCurve,
                    editorLevelOfSimplification));
    }

    public Vector2 WorldToMapPosition(Vector3 position)
    {
        float startX = transform.position.x - (mapChunkSize - 1) * terrainData.uniformScale / 2;
        float startZ = transform.position.z - (mapChunkSize - 1) * terrainData.uniformScale / 2;

        float posX = Mathf.Round((position.x - startX) / terrainData.uniformScale);
        float posY = Mathf.Round((position.z - startZ) / terrainData.uniformScale);

        if (posX < 0)
            posX = 0;
        if (posX >= mapChunkSize)
            posX = mapChunkSize - 1;
        if (posY < 0)
            posY = 0;
        if (posY >= mapChunkSize)
            posY = mapChunkSize - 1;

        posY = mapChunkSize - posY - 1;

        return new Vector2(posX, posY);
    }

    private MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapChunkSize,
            noiseData.seed,
            noiseData.noiseScale,
            noiseData.octaves,
            noiseData.persistance,
            noiseData.lacunarity, 
            center + noiseData.offset,
            noiseData.normalizeMode);

        for (int x = 0; x < mapChunkSize; x++)
        {
            for (int y = 0; y < mapChunkSize; y++)
            {
                if (terrainData.useFallOff)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                else if (terrainData.useHeightBorder)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] + fallOffMap[x, y] / 2);
            }
        }

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

        return new MapData(noiseMap);
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadQueue)
        {
            mapDataThreadQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    private void MeshDataThread(MapData mapData, int levelOfSimplification, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            mapData.heightMap,
            mapChunkSize,
            terrainData.meshHeightMultiplier,
            terrainData.meshHeightCurve,
            levelOfSimplification);
        lock (meshDataThreadQueue)
        {
            meshDataThreadQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    private void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    private void Start()
    {
        pathFinding = GetComponent<PathFinding>();
        grid = GetComponent<Grid>();

        //noiseData.seed = UnityEngine.Random.Range(0, int.MaxValue);

        fallOffMap = FallOffGenerator.GenerateFallOffMap(mapChunkSize, terrainData.fallOffSmoothness, terrainData.fallOffOffset);
        map = GenerateMapData(Vector2.zero);

        if (mapDisplay == null)
            mapDisplay = GetComponent<MapDisplay>();

        mapDisplay.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(
                    map.heightMap,
                    mapChunkSize,
                    terrainData.meshHeightMultiplier,
                    terrainData.meshHeightCurve,
                    editorLevelOfSimplification));

        if (grid.CreateTerrainGrid(this))
        {
            //TrySpawnBuildings();
            //TrySpawnTrees();
        }

        Application.focusChanged += delegate
        {
            textureData.ApplyToMaterial(terrainMaterial);
        };

        GameObject waterPlane = Instantiate(
            waterObject,
            new Vector3(transform.position.x - (mapChunkSize * terrainData.uniformScale / 2) - 10f,
                transform.position.y + 5,
                transform.position.z - (mapChunkSize * terrainData.uniformScale / 2) - 10f),
            Quaternion.identity);
        waterPlane.GetComponent<Water>().Dimension = Mathf.RoundToInt(mapChunkSize);
        waterPlane.transform.localScale = Vector3.one * terrainData.uniformScale;
    }

    private void Update()
    {
        
    }

    private bool TrySpawnBuildings()
    {
        bool result = false;

        float topLeftX = (mapChunkSize - 1) / -2f * terrainData.uniformScale;
        float topLeftZ = (mapChunkSize - 1) / 2f * terrainData.uniformScale;
        List<GameObject> builded = new List<GameObject>();
        Vector2 enemyPos = Vector2.zero;
        Vector2 playerPos = Vector2.zero;

        int tryCount = 0;
        while (tryCount < 100)
        {
            enemyPos = new Vector2(UnityEngine.Random.Range(0, mapChunkSize), UnityEngine.Random.Range(0, mapChunkSize));
            if (!CheckFreePlace((int)enemyPos.x, (int)enemyPos.y, 20))
            {
                tryCount++;
                continue;
            }

            playerPos = Vector2.zero;
            bool isPlayerPosFound = false;
            int playerTryCount = 0;
            while (playerTryCount < 100)
            {
                playerPos = new Vector2(UnityEngine.Random.Range(0, mapChunkSize), UnityEngine.Random.Range(0, mapChunkSize));
                if ((playerPos - enemyPos).sqrMagnitude < 10000 || !CheckFreePlace((int)playerPos.x, (int)playerPos.y, 20) || !pathFinding.IsPathExists(enemyPos, playerPos))
                {
                    playerTryCount++;
                    continue;
                }
                isPlayerPosFound = true;
                break;
            }

            if (!isPlayerPosFound)
            {
                tryCount++;
                continue;
            }

            FlatBuildArea((int)enemyPos.x, (int)enemyPos.y, buildingsToSpawn[0].radius);
            FlatBuildArea((int)playerPos.x, (int)playerPos.y, buildingsToSpawn[0].radius);
            
            float heightValue = terrainData.meshHeightCurve.Evaluate(map.heightMap[(int)enemyPos.x, (int)enemyPos.y]) * terrainData.meshHeightMultiplier * terrainData.uniformScale;
            builded.Add(Instantiate(
                buildingsToSpawn[0].building,
                new Vector3(
                    topLeftX + enemyPos.x * terrainData.uniformScale,
                    heightValue,
                    topLeftZ - enemyPos.y * terrainData.uniformScale),
                Quaternion.identity));
            
            heightValue = terrainData.meshHeightCurve.Evaluate(map.heightMap[(int)playerPos.x, (int)playerPos.y]) * terrainData.meshHeightMultiplier * terrainData.uniformScale;
            builded.Add(Instantiate(
                buildingsToSpawn[0].building,
                new Vector3(
                    topLeftX + playerPos.x * terrainData.uniformScale,
                    heightValue,
                    topLeftZ - playerPos.y * terrainData.uniformScale),
                Quaternion.identity));

            result = true;
            break;
        }

        if (result)
        {
            grid.SetUnwalkable((int)enemyPos.x, (int)enemyPos.y, buildingsToSpawn[0].unwalkableRadius);
            grid.SetUnwalkable((int)playerPos.x, (int)playerPos.y, buildingsToSpawn[0].unwalkableRadius);
            
            builded.Clear();
            result = false;
            int maxBuildCount = UnityEngine.Random.Range(5, 10);
            
            int buildCount = 0;
            int maxBuildDist = UnityEngine.Random.Range(10, 20);
            int sqrMBD = maxBuildDist * maxBuildDist;
            List<Vector2> buldedPositions = new List<Vector2>();
            for (int x = (int)enemyPos.x - maxBuildDist; x < (int)enemyPos.x + maxBuildDist; x++)
            {
                if (buildCount > maxBuildCount)
                    break;
                for (int y = (int)enemyPos.y - maxBuildDist; y < (int)enemyPos.y + maxBuildDist; y++)
                {
                    if (buildCount > maxBuildCount)
                        break;
                    int p = UnityEngine.Random.Range(0, 100);
                    if (p < 1)
                    {
                        int index = UnityEngine.Random.Range(1, buildingsToSpawn.Count);
                        
                        BuildingInfo b = buildingsToSpawn[index];
                        if (!CheckFreePlace(x, y, b.radius))
                            continue;

                        Vector2 pos = new Vector2(x, y);
                        if (buldedPositions.Any(bP => (bP - pos).sqrMagnitude < b.radius * b.radius))
                            continue;

                        FlatBuildArea(x, y, b.radius);

                        buildCount++;
                        float heightValue = terrainData.meshHeightCurve.Evaluate(map.heightMap[x, y]) * terrainData.meshHeightMultiplier * terrainData.uniformScale;
                        builded.Add(Instantiate(
                            b.building,
                            new Vector3(
                                topLeftX + x * terrainData.uniformScale,
                                heightValue,
                                topLeftZ - y * terrainData.uniformScale),
                            Quaternion.identity));

                        grid.SetUnwalkable(x, y, b.unwalkableRadius);
                    }
                }
            }

            return true;
        }
        else
        {
            foreach (GameObject b in builded)
            {
                grid.NodeFromWorldPoint(b.transform.position).walkable = true;
                Destroy(b);
            }
            return false;
        }
    }

    private void TrySpawnTrees()
    {
        float topLeftX = (mapChunkSize - 1) / -2f * terrainData.uniformScale;
        float topLeftZ = (mapChunkSize - 1) / 2f * terrainData.uniformScale;

        int treesCount = 1000;
        int tryCount = 10000;
        while (treesCount > 0 && tryCount > 0)
        {
            tryCount--;

            int x = UnityEngine.Random.Range(0, mapChunkSize);
            int y = UnityEngine.Random.Range(0, mapChunkSize);

            if (!grid.grid[x, y].walkable)
                continue;

            treesCount--;
            int index = UnityEngine.Random.Range(0, terrainObjectsToSpawn.Count);
            float heightValue = terrainData.meshHeightCurve.Evaluate(map.heightMap[x, y]) * terrainData.meshHeightMultiplier * terrainData.uniformScale;
            GameObject tree = Instantiate(
                terrainObjectsToSpawn[index],
                new Vector3(
                    topLeftX + x * terrainData.uniformScale,
                    heightValue,
                    topLeftZ - y * terrainData.uniformScale),
                Quaternion.identity);
            grid.grid[x, y].walkable = false;
        }
    }

    private struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


public struct MapData
{
    public float[,] heightMap;
    public List<GameObject> spawnedObjects;

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
        spawnedObjects = new List<GameObject>();
    }
}

[System.Serializable]
public struct BuildingInfo
{
    public GameObject building;
    public int radius;
    public int unwalkableRadius;

    public BuildingInfo(GameObject bulding, int radius, int unwalkableRadius)
    {
        this.building = bulding;
        this.radius = radius;
        this.unwalkableRadius = unwalkableRadius;
    }
}