using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    
    [Header("Tree Generation")]
    public int treeChance = 15;
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;
    
    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int chunkSize = 16;
    public int worldSize = 100;
    
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 25f;
    public int heightAddition = 25;
    
    [Header("Noise Settings")]
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;
    public float seed;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;
    
    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    private void OnValidate()
    {
        
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);
        
        //* Cave Determination *
        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        
        //* Ores *
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);
    }

    private void Start()
    {
        //* Set a random Seed *
        seed = Random.Range(-10000, 10000);
        
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);
        
        //* Cave Determination *
        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        
        //* Ores *
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);
        
        //* Terrain *
        CreateChunks();
        GenerateTerrain();
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

    public void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;
            
            for (int y = 0; y < height; y++)
            {
                Sprite[] tileSprites;
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.stone.tileSprites;
                    
                    // *Are we below dirt level*?
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
                    // *Are we below grass level*?
                    tileSprites = tileAtlas.dirt.tileSprites;
                }
                else
                {
                    // *Top layer of Terrain*
                    tileSprites = tileAtlas.grass.tileSprites;
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
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        // *Generate Tree*
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(x, y + 1);
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, tallGrassChance);
                        if (i == 1)
                        {
                            //*Generate Grass*
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                PlaceTile(tileAtlas.tallGrass.tileSprites, x, y + 1);
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

    void GenerateTree(int x, int y)
    {
        // *How does the tree Look?*
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight);

        // *Log Generation*
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.tileSprites, x, y +i);
        }
        
        // *Generate Leaves*
        PlaceTile(tileAtlas.leaf.tileSprites,x,y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites,x,y + treeHeight +1);
        PlaceTile(tileAtlas.leaf.tileSprites,x,y + treeHeight +2);
        
        PlaceTile(tileAtlas.leaf.tileSprites,x -1,y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites,x -1,y + treeHeight +1);
        
        PlaceTile(tileAtlas.leaf.tileSprites,x +1,y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites,x +1,y + treeHeight +1);
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
