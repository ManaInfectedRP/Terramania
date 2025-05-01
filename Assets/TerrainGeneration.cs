using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    public int dirtLayerHeight = 5;
    
    public Sprite dirt;
    public Sprite grass;
    public Sprite stone;

    public Sprite log;
    public Sprite leaf;

    public int treeChance = 10;
    public bool generateCaves = true;

    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;
    public float heightMultiplier = 25f;
    public int heightAddition = 25;
    
    public float seed;
    public Texture2D noiseTexture;

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
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
                    tileSprite = stone;
                }
                else if (y < height - 1)
                {
                    tileSprite = dirt;
                }
                else
                {
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

    public void PlaceTile(Sprite tileSprite, float x, float y)
    {
        GameObject newTile = new GameObject();
        newTile.transform.parent = this.transform;
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
    }
}
