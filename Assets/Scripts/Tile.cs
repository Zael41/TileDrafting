using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int position;
    public GameObject tileObject;
    public bool generated;
    public bool camera;
    public bool[] directions; // 0 - up, 1 - right, 2 - down, 3 - left
    public TileTypes tileType;

    public Tile(Vector2Int position, GameObject tileObject, TileTypes tileType = TileTypes.Default)
    {
        this.position = position;
        this.tileObject = tileObject;
        generated = false;
        camera = false;
        directions = new bool[4];
        this.tileType = tileType;
    }
}

public enum TileTypes
{
    Default,
    Vault,
    Control_Room,
    Med_Bay,
    Surveillance
}
