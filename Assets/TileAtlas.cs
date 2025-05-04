using UnityEngine;

[CreateAssetMenu(fileName = "newTileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Enviroment")]
    public TileClass grass;
    public TileClass dirt;
    public TileClass stone;
    public TileClass sand;
    public TileClass snow;
    
    public TileClass log;
    public TileClass leaf;
    
    [Header("Addons")]
    public TileClass tallGrass;
    public TileClass cactus;
    
    [Header("Ores")]
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
}
