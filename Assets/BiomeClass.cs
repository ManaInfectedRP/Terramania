using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeColor;

    public TileAtlas biomeAtlas;
    
    [Header("Noise Settings")]
    public float caveFreq = 0.08f;
    public float terrainFreq = 0.04f;
    public Texture2D caveNoiseTexture;
    
    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 25f;
    
    [Header("Tree Generation")]
    public int treeChance = 15;
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 5;
    
    [Header("Ore Settings")]
    public OreClass[] ores;
}
