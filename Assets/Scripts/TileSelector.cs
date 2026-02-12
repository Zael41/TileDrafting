using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private List<UITiles> UITiles; //Change this for another custom class called "UITiles" that also stores their info
    [SerializeField] private List<AvailableTiles> availableTiles;
    [SerializeField] private List<SelectableTiles> selectableTiles;
    private int selectedTile;
    private Keyboard keyboard;
    public int requiredDirection;

    private void Start()
    {
        selectableTiles = new List<SelectableTiles>();
        foreach (AvailableTiles tile in availableTiles)
        {
            for (int i = 0; i < tile.amountWithoutCamera; i++)
            {
                selectableTiles.Add(new SelectableTiles(tile.tileSprite, tile.tileType, false, tile.directions));
            }
            for (int i = 0; i < tile.amountWithCamera; i++)
            {
                selectableTiles.Add(new SelectableTiles(tile.tileSprite, tile.tileType, true, tile.directions));
            }
        }
        keyboard = Keyboard.current;
        selectedTile = -1;
    }

    private void Update()
    {
        if (selectedTile != -1)
        {
            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(false);
                selectedTile++;
                if (selectedTile > 2) selectedTile = 0;
                UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(true);
            }
            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(false);
                selectedTile--;
                if (selectedTile < 0) selectedTile = 2;
                UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(true);
            }
            if (keyboard.enterKey.wasPressedThisFrame && UITiles[selectedTile].directions[requiredDirection]) //Checks if it connects
            {
                gridManager.PlaceTile(UITiles[selectedTile].tileObject.transform.GetChild(0).GetComponent<Image>().sprite, UITiles[selectedTile].tileObject.transform.GetChild(0).rotation, UITiles[selectedTile].camera, UITiles[selectedTile].directions, UITiles[selectedTile].tileType);
                selectedTile = -1;
                HideMenu();
                gridManager.generatingTiles = false;
                //Remove the placed tile from the pool
            }
            if (keyboard.rKey.wasPressedThisFrame)
            {
                UITiles[selectedTile].tileObject.transform.GetChild(0).Rotate(0, 0, 90);
                bool[] newDirections = new bool[4];
                for (int i = 0; i < UITiles[selectedTile].directions.Length; i++)
                {
                    int nextIndex = i + 1;
                    if (nextIndex > 3) nextIndex = 0;
                    newDirections[i] = UITiles[selectedTile].directions[nextIndex];
                }
                UITiles[selectedTile].directions = newDirections;
            }
        }
    }

    public void ShowMenu()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void HideMenu()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void GenerateTiles()
    {
        foreach (UITiles obj in UITiles)
        {
            obj.tileObject.transform.GetChild(0).rotation = Quaternion.identity; //Reset rotation
            obj.tileObject.transform.GetChild(1).gameObject.SetActive(false); //Reset selection
            int randomIndex = Random.Range(0, selectableTiles.Count);
            SelectableTiles randomTile = selectableTiles[randomIndex];
            obj.tileObject.transform.GetChild(0).GetComponent<Image>().sprite = randomTile.tileSprite;
            obj.tileType = randomTile.tileType;
            obj.camera = randomTile.camera;
            obj.directions = randomTile.directions;
            if (randomTile.camera) obj.tileObject.transform.GetChild(2).gameObject.SetActive(true);
            else obj.tileObject.transform.GetChild(2).gameObject.SetActive(false);
        }
        selectedTile = 0;
        UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(true);
    }
}

[System.Serializable]
public class AvailableTiles //Used by the developer to tell the game how many tiles to generate
{
    public Sprite tileSprite;
    public TileTypes tileType;
    public bool[] directions;
    public int amountWithoutCamera;
    public int amountWithCamera;
}

[System.Serializable]
public class SelectableTiles //The actual X amount of tiles that the player will choose from
{
    public Sprite tileSprite;
    public TileTypes tileType;
    public bool camera;
    public bool[] directions;

    public SelectableTiles(Sprite tileSprite, TileTypes tileType, bool camera, bool[] directions)
    {
        this.tileSprite = tileSprite;
        this.tileType = tileType;
        this.camera = camera;
        this.directions = directions;
    }
}

[System.Serializable]
public class UITiles // The three tiles that appear on the screen
{
    public GameObject tileObject;
    public TileTypes tileType;
    public bool camera;
    public bool[] directions;
}
