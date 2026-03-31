using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    #region "Variables"
    [Header("Assets")]
    [SerializeField] private Sprite oneGearSprite;
    [SerializeField] private Sprite twoGearSprite;
    [SerializeField] private Sprite emptyTile;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TMP_Text title;

    [Header("Game Variables")]
    [SerializeField] private List<UITiles> UITiles;
    [SerializeField] private List<AvailableTiles> availableTiles;

    private List<SelectableTiles> selectableTiles;
    private int selectedTile;
    private Keyboard keyboard;
    private int initialTilesCount;
    private int extraCRKRrerolls;

    [HideInInspector] public int requiredDirection;
    [HideInInspector] public bool currentlySelecting;
    #endregion

    private void Start()
    {
        selectableTiles = new List<SelectableTiles>();
        foreach (AvailableTiles tile in availableTiles)
        {
            foreach (AvailableTileInfo tileInfo in tile.tileInfo)
            {
                selectableTiles.Add(new SelectableTiles(tile.tileSprite, tile.tileType, tileInfo.camera, tile.directions, tileInfo.gearAmount));
            }
        }
        initialTilesCount = selectableTiles.Count;
        keyboard = Keyboard.current;
        selectedTile = -1;
        extraCRKRrerolls = -5;
        GenerateTiles();
    }

    private void Update()
    {
        if (selectedTile != -1 && currentlySelecting)
        {
            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                SelectUITile(false);
            }
            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                SelectUITile(true);
            }
            if (keyboard.enterKey.wasPressedThisFrame && UITiles[selectedTile].directions[requiredDirection] && !UITiles[selectedTile].empty) //Checks if it connects
            {
                gridManager.PlaceTile(UITiles[selectedTile].tileObject.transform.GetChild(0).GetComponent<Image>().sprite, UITiles[selectedTile].tileObject.transform.GetChild(0).rotation, UITiles[selectedTile].camera, UITiles[selectedTile].directions, UITiles[selectedTile].tileType, UITiles[selectedTile].gearAmount, true);
                gridManager.nextTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                UITiles[selectedTile].locked = false;
                UITiles[selectedTile].tileObject.transform.GetChild(4).gameObject.SetActive(false);
                selectableTiles.Remove(UITiles[selectedTile].tileFromList); //Remove the placed tile from the pool
                StopSelection();
                GenerateTiles();
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
                gridManager.PlaceTile(UITiles[selectedTile].tileObject.transform.GetChild(0).GetComponent<Image>().sprite, UITiles[selectedTile].tileObject.transform.GetChild(0).rotation, UITiles[selectedTile].camera, UITiles[selectedTile].directions, UITiles[selectedTile].tileType, UITiles[selectedTile].gearAmount);
            }
        }
    }

    private void SelectUITile(bool up)
    {
        UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(false);
        selectedTile += up ? -1 : 1;
        if (selectedTile < 0) selectedTile = UITiles.Count - 1;
        else if (selectedTile > UITiles.Count - 1) selectedTile = 0;
        UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(true);
        gridManager.PlaceTile(UITiles[selectedTile].tileObject.transform.GetChild(0).GetComponent<Image>().sprite, UITiles[selectedTile].tileObject.transform.GetChild(0).rotation, UITiles[selectedTile].camera, UITiles[selectedTile].directions, UITiles[selectedTile].tileType, UITiles[selectedTile].gearAmount);
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

    public int GetTilesLeft()
    {
        return selectableTiles.Count;
    }

    public void GenerateTiles()
    {
        List<SelectableTiles> usedTiles = new List<SelectableTiles>();
        List<UITiles> lockedUITiles = LockedTiles();

        foreach (UITiles t in UITiles) //Reset UI tiles
        {
            t.tileObject.transform.GetChild(1).gameObject.SetActive(false);
            if (t.locked) //Don't touch locked tiles
            {
                usedTiles.Add(t.tileFromList);
                continue;
            }
            t.tileObject.transform.GetChild(0).rotation = Quaternion.identity;
            t.tileObject.transform.GetChild(0).GetComponent<Image>().sprite = emptyTile;
            t.tileObject.transform.GetChild(2).gameObject.SetActive(false);
            t.tileObject.transform.GetChild(3).gameObject.SetActive(false);
            t.empty = true;
        }
        if (selectableTiles.Count == lockedUITiles.Count) //Cases: 1 tile left and locked, 2 tiles left and both locked
        {
            return;
        }
        else if (selectableTiles.Count < 3) //Cases: 1 tile left not locked, 2 tiles left 1 locked, 2 tiles left none locked
        {
            List<SelectableTiles> remainingTiles = new List<SelectableTiles>();
            foreach (SelectableTiles tile in selectableTiles)
            {
                remainingTiles.Add(tile);
            }
            if (lockedUITiles.Count > 0) remainingTiles.Remove(lockedUITiles[0].tileFromList);

            int remainingTilesCounter = 0;

            for (int i = 0; i < UITiles.Count; i++)
            {
                if (UITiles[i].locked || remainingTilesCounter >= remainingTiles.Count) //Don't touch locked tiles
                {
                    continue;
                }
                SetUITileValues(UITiles[i], remainingTiles[remainingTilesCounter]);
                remainingTilesCounter++;
            }
        }
        else //Cases: Always 3 tiles, all 3 can be locked or not
        {
            int currentRerollCounter = extraCRKRrerolls;
            foreach (UITiles obj in UITiles)
            {
                if (obj.locked) //Don't touch locked tiles
                {
                    continue;
                }
                int randomIndex = Random.Range(0, selectableTiles.Count);
                bool showKRCR = initialTilesCount - selectableTiles.Count > 5; //Show control and key rooms after 5 drafts
                if (!showKRCR)
                {
                    while (usedTiles.Contains(selectableTiles[randomIndex]) || selectableTiles[randomIndex].tileType == TileTypes.Control_Room || selectableTiles[randomIndex].tileType == TileTypes.Key_Room)
                    {
                        randomIndex = Random.Range(0, selectableTiles.Count);
                    }
                }
                else
                {
                    do
                    {
                        randomIndex = FindTileIndex(randomIndex, usedTiles);
                        currentRerollCounter--;
                    } while (currentRerollCounter > 0 && selectableTiles[randomIndex].tileType != TileTypes.Control_Room && selectableTiles[randomIndex].tileType != TileTypes.Key_Room);

                    if (selectableTiles[randomIndex].tileType == TileTypes.Control_Room || selectableTiles[randomIndex].tileType == TileTypes.Key_Room)
                    {
                        extraCRKRrerolls = 0;
                        currentRerollCounter = 0;
                    }
                }
                SelectableTiles randomTile = selectableTiles[randomIndex];
                usedTiles.Add(randomTile);
                SetUITileValues(obj, randomTile);
            }
            extraCRKRrerolls++;
        }
    }

    private int FindTileIndex(int initialIndex, List<SelectableTiles> usedTiles)
    {
        int randomIndex = initialIndex;
        do
        {
            randomIndex = Random.Range(0, selectableTiles.Count);
        } while (usedTiles.Contains(selectableTiles[randomIndex]));
        return randomIndex;
    }

    private List<UITiles> LockedTiles()
    {
        List<UITiles> lockedTiles = new List<UITiles>();
        foreach (UITiles t in UITiles)
        {
            if (t.locked) lockedTiles.Add(t);
        }
        return lockedTiles;
    }

    private void SetUITileValues(UITiles uiTile, SelectableTiles tile)
    {
        uiTile.tileObject.transform.GetChild(0).GetComponent<Image>().sprite = tile.tileSprite;
        uiTile.tileType = tile.tileType;
        uiTile.camera = tile.camera;
        uiTile.directions = tile.directions;
        uiTile.gearAmount = tile.gearAmount;
        uiTile.tileFromList = tile;
        SetUITileGears(uiTile);
        uiTile.empty = false;
        if (tile.camera) uiTile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
        else uiTile.tileObject.transform.GetChild(2).gameObject.SetActive(false);
    }

    public void SetUITileGears(UITiles tile)
    {
        tile.tileObject.transform.GetChild(3).gameObject.SetActive(tile.gearAmount == 0 ? false : true);
        tile.tileObject.transform.GetChild(3).GetComponent<Image>().sprite = Mathf.Abs(tile.gearAmount) == 2 ? twoGearSprite : oneGearSprite; // Doesn't matter if it's 0 because it won't be shown
        tile.tileObject.transform.GetChild(3).GetComponent<Image>().color = tile.gearAmount < 0 ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f); // Same here
    }

    public void StartSelection(int requiredDirection)
    {
        this.requiredDirection = requiredDirection;
        currentlySelecting = true;
        selectedTile = 0;
        UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(true);
        title.text = "Choose One: ";
        gridManager.PlaceTile(UITiles[selectedTile].tileObject.transform.GetChild(0).GetComponent<Image>().sprite, UITiles[selectedTile].tileObject.transform.GetChild(0).rotation, UITiles[selectedTile].camera, UITiles[selectedTile].directions, UITiles[selectedTile].tileType, UITiles[selectedTile].gearAmount);
        gridManager.nextTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
    }

    public void StopSelection()
    {
        requiredDirection = -1;
        currentlySelecting = false;
        UITiles[selectedTile].tileObject.transform.GetChild(1).gameObject.SetActive(false);
        selectedTile = -1;
        gridManager.generatingTiles = false;
        title.text = "Next Choices";
    }

    public void LockSelected()
    {
        UITiles[selectedTile].locked = true;
        UITiles[selectedTile].tileObject.transform.GetChild(4).gameObject.SetActive(true);
    }
}

[System.Serializable]
public class AvailableTiles //Used by the developer to tell the game how many tiles to generate
{
    public Sprite tileSprite;
    public TileTypes tileType;
    public bool[] directions;
    public List<AvailableTileInfo> tileInfo;
}

[System.Serializable]
public class AvailableTileInfo // Info of how many tiles of that type have cameras or gears
{
    public bool camera;
    public int gearAmount;

    public AvailableTileInfo(bool camera, int gearAmount)
    {
        this.camera = camera;
        this.gearAmount = gearAmount;
    }
}

[System.Serializable]
public class SelectableTiles //The actual X amount of tiles that the player will choose from
{
    public Sprite tileSprite;
    public TileTypes tileType;
    public bool camera;
    public bool[] directions;
    public int gearAmount;

    public SelectableTiles(Sprite tileSprite, TileTypes tileType, bool camera, bool[] directions, int gearAmount)
    {
        this.tileSprite = tileSprite;
        this.tileType = tileType;
        this.camera = camera;
        this.directions = directions;
        this.gearAmount = gearAmount;
    }
}

[System.Serializable]
public class UITiles // The three tiles that appear on the screen
{
    public GameObject tileObject;
    public TileTypes tileType;
    public bool camera;
    public bool[] directions;
    public int gearAmount;
    public SelectableTiles tileFromList;
    public bool locked;
    public bool empty;
}
