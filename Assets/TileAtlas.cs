using UnityEngine;

[CreateAssetMenu(fileName = "newTileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    public TileClass dirt;
    public TileClass grass;
    public TileClass stone;
    public TileClass log;
    public TileClass leaf;
    
    public TileClass tallGrass;
    
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
}
