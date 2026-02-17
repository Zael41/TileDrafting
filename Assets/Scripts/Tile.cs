using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int position;
    public GameObject tileObject;
    public bool generated;
    public bool camera;
    public bool cameraEnabled;
    public bool[] directions; // 0 - up, 1 - right, 2 - down, 3 - left
    public TileTypes tileType;
    public int gearAmount;
    public bool vaultOpen; //Vault only variable
    public int remainingUses; //Room action only variable

    public Tile(Vector2Int position, GameObject tileObject, TileTypes tileType = TileTypes.Default)
    {
        this.position = position;
        this.tileObject = tileObject;
        generated = false;
        camera = false;
        cameraEnabled = true;
        directions = new bool[4];
        this.tileType = tileType;
        vaultOpen = false;
        remainingUses = 1;
        gearAmount = 0;
    }
}

public enum TileTypes
{
    Default,
    Start,
    Vault,
    Control_Room,
    Med_Bay,
    Surveillance
}
