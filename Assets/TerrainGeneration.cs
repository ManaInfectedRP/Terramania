using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;
    
    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;
    
    
    [Header("Generation Settings")]
    public int chunkSize = 10;
    public int worldSize = 100;
    public bool generateCaves = true;
    
    public int heightAddition = 25;
    
    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    public BiomeClass curBiome;

    private void OnValidate()
    {
        DrawTextures();
    }

    private void Start()
    {
        //* Set a random Seed *
        seed = Random.Range(-10000, 10000);
        
        //* Generates NoiseMaps *
        DrawTextures();
        DrawCaves();
        
        //* Terrain *
        CreateChunks();
        GenerateTerrain();
    }

    public void DrawCaves()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                
                float v = Mathf.PerlinNoise((x + seed) * curBiome.caveFreq, (y+seed) * curBiome.caveFreq);
                if (v > curBiome.surfaceValue)
                {
                    caveNoiseTexture.SetPixel(x,y, Color.white);
                }
                else
                {
                    caveNoiseTexture.SetPixel(x,y, Color.black);
                }
            }
        }
        
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                for (int i = 0; i < curBiome.ores.Length; i++)
                {
                    curBiome = GetCurrentBiome(x, y);

                    float v = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].rarity,
                        (y + seed) * curBiome.ores[i].rarity);
                    if (v > curBiome.ores[i].size)
                    {
                        ores[i].spreadTexture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    }
            
                    ores[i].spreadTexture.Apply();
                }
            }
        }
        
        caveNoiseTexture.Apply();
    }

    public void DrawTextures()
    {
        biomeMap = new Texture2D(worldSize, worldSize);
        DrawBiomeTexture();
        
        for (int i = 0; i < biomes.Length; i++)
        {
        
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
            }
        
            //* Cave Determination *
            GenerateNoiseTexture(biomes[i].caveFreq, biomes[i].surfaceValue, biomes[i].caveNoiseTexture);
        
            //* Ores *
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                GenerateNoiseTexture(biomes[i].ores[o].rarity, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }
    }

    public void DrawBiomeTexture()
    {
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                Color col = biomeGradient.Evaluate(v);
                biomeMap.SetPixel(x,y, col);
            }
        }
        biomeMap.Apply();
    }
    
    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];
        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        //* Get Current Biome * Search trough biomes *
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].biomeColor == biomeMap.GetPixel(x,y))
            {
                return biomes[i];
            }
        }
        return curBiome;
    }
    
    public void GenerateTerrain()
    {
        Sprite[] tileSprites;
        for (int x = 0; x < worldSize; x++)
        {
            curBiome = GetCurrentBiome(x, 0);
            float height = Mathf.PerlinNoise((x + seed) * curBiome.terrainFreq, seed * curBiome.terrainFreq) * curBiome.heightMultiplier + heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileSprites = curBiome.biomeAtlas.stone.tileSprites;
                    
                    // *Are we below dirt level*?
                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileSprites = curBiome.biomeAtlas.coal.tileSprites;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileSprites = curBiome.biomeAtlas.iron.tileSprites; 
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                        tileSprites = curBiome.biomeAtlas.gold.tileSprites;
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                        tileSprites = curBiome.biomeAtlas.diamond.tileSprites;
                }
                else if (y < height - 1)
                {
                    // *Are we below grass level*?
                    tileSprites = curBiome.biomeAtlas.dirt.tileSprites;
                }
                else
                {
                    // *Top layer of Terrain*
                    tileSprites = curBiome.biomeAtlas.grass.tileSprites;
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

                if (y >= height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);
                    if (t == 1)
                    {
                        // *Generate Tree*
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.tallGrassChance);
                        if (i == 1)
                        {
                            //*Generate Grass*
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if(curBiome.biomeAtlas.tallGrass != null)
                                    PlaceTile(curBiome.biomeAtlas.tallGrass.tileSprites, x, y + 1);
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y+seed) * frequency);
                if (v > limit)
                {
                    noiseTexture.SetPixel(x,y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x,y, Color.black);
                }
            }
        }
        noiseTexture.Apply();
    }

    void GenerateTree(int treeHeight, int x, int y)
    {
        // *How does the tree Look?*
        // *Log Generation*
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.tileSprites, x, y +i);
        }
        
        // *Generate Leaves*
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x,y + treeHeight);
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x,y + treeHeight +1);
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x,y + treeHeight +2);
        
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x -1,y + treeHeight);
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x -1,y + treeHeight +1);
        
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x +1,y + treeHeight);
        PlaceTile(curBiome.biomeAtlas.leaf.tileSprites,x +1,y + treeHeight +1);
    }
    
    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        if(!worldTiles.Contains(new Vector2(x,y)))
        {
            GameObject newTile = new GameObject();

            float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
            newTile.AddComponent<SpriteRenderer>();
            int spriteIndex = Random.Range(0, tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];

            newTile.name = tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        }
    }
}
