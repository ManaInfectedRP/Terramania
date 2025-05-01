using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile/Sprites")]
    public Sprite dirt;
    public Sprite grass;
    public Sprite stone;
    public Sprite log;
    public Sprite leaf;
    
    [Header("Tree Generation")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;
    
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
    public Texture2D noiseTexture;

    public GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
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
                Sprite tileSprite;
                if (y < height - dirtLayerHeight)
                {
                    // *Are we below dirt level*?
                    tileSprite = stone;
                }
                else if (y < height - 1)
                {
                    // *Are we below grass level*?
                    tileSprite = dirt;
                }
                else
                {
                    // *Top layer of Terrain*
                    tileSprite = grass;
                }

                if (generateCaves)
                {
                    if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                    {
                        PlaceTile(tileSprite, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, x, y);
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
                }
            }
        }
    }
    
    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveFreq, (y+seed) * caveFreq);
                noiseTexture.SetPixel(x,y, new Color(v,v,v));
                
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
            PlaceTile(log, x, y +i);
        }
        
        // *Generate Leaves*
        PlaceTile(leaf,x,y + treeHeight);
        PlaceTile(leaf,x,y + treeHeight +1);
        PlaceTile(leaf,x,y + treeHeight +2);
        
        PlaceTile(leaf,x -1,y + treeHeight);
        PlaceTile(leaf,x -1,y + treeHeight +1);
        
        PlaceTile(leaf,x +1,y + treeHeight);
        PlaceTile(leaf,x +1,y + treeHeight +1);
    }
    
    public void PlaceTile(Sprite tileSprite, int x, int y)
    {
        GameObject newTile = new GameObject();

        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;
        Debug.Log(chunkCoord);
        newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
        
        
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
        
        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
    }
}
