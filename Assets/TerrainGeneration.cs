using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Generation Settings")]
    public int chunkSize = 16;
    public int worldSize = 100;
    public int heightAddition = 25;
    public bool generateCaves = true;


    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;

    [Header("Ore Settings")]
    public OreClass[] ores;


    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private BiomeClass curBiome;
    private Color[] biomeCols;

    private void OnValidate()
    {
        DrawBiomeMap();
    }

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        
        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }

        //DrawTextures();
        DrawBiomeMap();
        DrawCavesAndOres();
        
        CreateChunks();
        GenerateTerrain();
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x,y, col);
            }
        }
        
        biomeMap.Apply();
    }
    
    public void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D (worldSize, worldSize);
        float v;
        float o;
        
        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                {
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    caveNoiseTexture.SetPixel(x, y, Color.black);
                }
                
                for (int i = 0; i < curBiome.ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i  + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                        {
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                        }
                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        } 

        caveNoiseTexture.Apply();
    }

    public void DrawTextures()
    {
        for (int i = 0; i < biomes.Length; i++)
        {

            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTextures(biomes[i].ores[o].frequency, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }
    }
    
    private void GenerateNoiseTextures( float frequency, float limit, Texture2D noiseTexture)
    {
        float v;
        
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
            }
        }
        noiseTexture.Apply();
    }
    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject(i.ToString());
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        Color targetColor = biomeMap.GetPixel(x, y);
        float minDistance = float.MaxValue;
        BiomeClass closestBiome = null;

        for (int i = 0; i < biomes.Length; i++)
        {
            float distance = ColorDistance(targetColor, biomes[i].biomeCol);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestBiome = biomes[i];
            }
        }

        // Optional: log warning if nothing close enough was found
        if (closestBiome == null)
            Debug.LogWarning($"No matching biome found at pixel ({x}, {y})");

        return closestBiome;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.r - b.r, 2) +
            Mathf.Pow(a.g - b.g, 2) +
            Mathf.Pow(a.b - b.b, 2)
        );
    }


    public void GenerateTerrain()
    {
        Sprite[] tileSprites;
        for (int x = 0; x < worldSize; x++)
        {
            curBiome = GetCurrentBiome(x, 0);
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;
            for (int y = 0; y <= height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileSprites = curBiome.tileAtlas.stone.tileSprites;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileSprites = tileAtlas.coal.tileSprites;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileSprites = tileAtlas.iron.tileSprites;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                        tileSprites = tileAtlas.gold.tileSprites;
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                        tileSprites = tileAtlas.diamond.tileSprites;
                }
                else if (y < height - 1)
                {
                    tileSprites = curBiome.tileAtlas.dirt.tileSprites;
                }
                else
                {
                    //layer top
                    tileSprites = curBiome.tileAtlas.grass.tileSprites;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprites, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y);
                }

                if (y > height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);
                    if (t == 1)
                    {
                        //generate tree
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            if (curBiome.biomeName == "Desert")
                                GenerateCactus(curBiome.tileAtlas, Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight + 1), x,
                                    y + 1);
                            else
                                GenerateTree(Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight + 1), x,
                                    y + 1);
                        }
                    }
                    else
                    {
                        //generate grass
                        int i = Random.Range(0, curBiome.tallGrassChance);
                        if (i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                    PlaceTile(curBiome.tileAtlas.tallGrass.tileSprites, x, y + 1);
                            }
                        }
                    }
                }
            }
        }
    }

    void GenerateCactus(TileAtlas atlas, int treeHeight, int x, int y)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(atlas.log.tileSprites, x, y + i);
        }
        
    }
    
    void GenerateTree(int treeHeight, int x, int y)
    {
        //generate log
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.tileSprites, x, y + i);
        }
        //generate leaf
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight + 1);
    }
    
    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        Vector2 tilePosition = new Vector2(x, y);
        if (!worldTiles.Contains(tilePosition))
        {
            GameObject newTile = new GameObject("tile");
            int chunkCoord = Mathf.FloorToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;
            
            if (chunkCoord >= 0 && chunkCoord < worldChunks.Length)
            {
                newTile.transform.parent = worldChunks[chunkCoord].transform;
            }
            else
            {
                Debug.LogWarning($"Chunk index {chunkCoord} out of bounds for x = {x}");
                return;
            }

            SpriteRenderer renderer = newTile.AddComponent<SpriteRenderer>();
            int spriteIndex = Random.Range(0, tileSprites.Length);
            renderer.sprite = tileSprites[spriteIndex];

            newTile.name = tileSprites[spriteIndex].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            worldTiles.Add(tilePosition);
        }
    }
}