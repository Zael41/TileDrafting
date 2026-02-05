using UnityEngine;

[System.Serializable]
public class Tile
{
    public Vector2Int position;
    public GameObject tileObject;
    public bool generated;
    public bool camera;
    public bool canGoUp, canGoDown, canGoLeft, canGoRight;
    public bool isVault;

    public Tile(Vector2Int position, GameObject tileObject, bool isVault = false)
    {
        this.position = position;
        this.tileObject = tileObject;
        generated = false;
        camera = false;
        canGoUp = false;
        canGoDown = false;
        canGoLeft = false;
        canGoRight = false;
        this.isVault = isVault;
    }
}
