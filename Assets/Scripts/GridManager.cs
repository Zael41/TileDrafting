using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.U2D;

public class GridManager : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject guard;
    [SerializeField] private Sprite lockedVault;
    [SerializeField] private Sprite openVault;
    [SerializeField] private Sprite startTile;
    [SerializeField] private Sprite oneGearSprite;
    [SerializeField] private Sprite twoGearSprite;

    [Header("Game Variables")]
    [SerializeField] private int gridSize;
    [SerializeField] private Vector2Int playerPos;
    [SerializeField] private int health;
    [SerializeField] private int vaultNumber;
    [SerializeField] private int nextAlertCounter;
    [SerializeField] private int gearAmount;
    [SerializeField] private int rerolls;
    [SerializeField] private int holds;
    private int toNextAlertLevel;
    private int alertLevel;

    [Header("References")]
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text rerollsText;
    [SerializeField] private TMP_Text holdsText;

    [SerializeField] private List<Tile> testtiles;

    private Tile[,] tiles;
    private Keyboard keyboard;
    private Tile nextTile;
    [HideInInspector] public bool generatingTiles;

    public static GridManager instance;

    private void Awake()
    {
        Debug.Log("awake");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        testtiles = new List<Tile>();
        tiles = new Tile[gridSize,gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                if (i == 4 && j == 4) //Change this if grid size changes
                {
                    tiles[i, j] = new Tile(new Vector2Int(i, j), Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform));
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = startTile;
                    tiles[i, j].directions = new bool[4] { true, true, true, true };
                    tiles[i, j].generated = true;
                    tiles[i,j].tileType = TileTypes.Start;
                }
                else
                {
                    tiles[i, j] = new Tile(new Vector2Int(i,j), Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform));
                }
                testtiles.Add(tiles[i, j]);
            }
        }

        keyboard = Keyboard.current;
    }

    private Vector2Int GetRandomVaultPos()
    {
        Vector2Int vaultPos = new Vector2Int();

        int upOrDown = Random.Range(0, 2);
        int leftOrRight = Random.Range(0, 2);

        if (upOrDown <= 0) vaultPos.x = Random.Range(6, 8);
        else vaultPos.x = Random.Range(0, 2);

        if (leftOrRight <= 0) vaultPos.y = Random.Range(6, 8);
        else vaultPos.y = Random.Range(0, 2);

        return vaultPos;
    }

    private void Update()
    {
        NewMovement(keyboard.downArrowKey, 2, new Vector2Int(0, -1), 0);
        NewMovement(keyboard.upArrowKey, 0, new Vector2Int(0, 1), 2);
        NewMovement(keyboard.leftArrowKey, 3, new Vector2Int(-1, 0), 1);
        NewMovement(keyboard.rightArrowKey, 1, new Vector2Int(1, 0), 3);

        if (keyboard.fKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].tileType != TileTypes.Default && tiles[playerPos.x, playerPos.y].remainingUses > 0 && !generatingTiles)
        {
            switch (tiles[playerPos.x, playerPos.y].tileType)
            {
                case TileTypes.Control_Room:
                    if (gearAmount < 2) break;
                    SpawnVault(); // Needs to account for directions
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount -= 2;
                    break;
                case TileTypes.Surveillance:
                    if (gearAmount < 1) break;
                    toNextAlertLevel = 0;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount--;
                    break;
                case TileTypes.Med_Bay:
                    if (gearAmount < 2) break;
                    health++;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount -= 2;
                    break;
                case TileTypes.Chief_Office:
                    if (gearAmount < 1) break;
                    rerolls += 2;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount--;
                    break;
                case TileTypes.Archives:
                    if (gearAmount < 1) break;
                    holds++;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount--;
                    break;
                default:
                    break;
            }
            UpdateUI();
        }
        if (keyboard.qKey.wasPressedThisFrame && rerolls > 0 && !generatingTiles)
        {
            tileSelector.GenerateTiles();
            rerolls--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        alertText.text = "Alert: " + alertLevel + "\n" + "Counter: " + toNextAlertLevel + "/" + nextAlertCounter;
        healthText.text = "Health: " + health;
        gearText.text = "Gears: " + gearAmount;
        rerollsText.text = "Rerolls: " + rerolls;
        holdsText.text = "Holds: " + holds;
    }

    private void NewMovement(KeyControl key, int directionsIndex, Vector2Int directionVector, int oppositeDirectionIndex)
    {
        // Draft tiles before even moving
        if (generatingTiles) return;
        if (key.wasReleasedThisFrame && tiles[playerPos.x, playerPos.y].directions[directionsIndex])
        {
            Vector2Int nextTilePosition = playerPos + directionVector;
            if (!CheckBounds(nextTilePosition)) return;
            nextTile = tiles[nextTilePosition.x, nextTilePosition.y];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated)
            {
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen)
                {
                    nextTile.generated = true;
                    nextTile.directions = new bool[4] { false, false, false, false };
                    nextTile.directions[oppositeDirectionIndex] = true;
                    player.transform.position += new Vector3(directionVector.x, directionVector.y, 0f);
                    playerPos += directionVector;
                    vaultNumber--;
                }
                else
                {
                    tileSelector.StartSelection(oppositeDirectionIndex);
                    generatingTiles = true;
                }
            }
            else //Need to check if both directions are valid to let you move
            {
                player.transform.position += new Vector3(directionVector.x, directionVector.y, 0f);
                playerPos += directionVector;
                if (nextTile.camera && nextTile.cameraEnabled)
                {
                    RaiseAlert(nextTile);
                }
                if (nextTile.gearAmount > 0)
                {
                    gearAmount += nextTile.gearAmount;
                    nextTile.gearAmount = 0;
                    nextTile.tileObject.transform.GetChild(2).gameObject.SetActive(false);
                    UpdateUI();
                }
                if (nextTile.tileType == TileTypes.Start && vaultNumber <= 0)
                {
                    Debug.Log("you win");
                }
            }
        }
    }

    private void SpawnVault()
    {
        Vector2Int vault1Pos = GetRandomVaultPos();
        while (tiles[vault1Pos.x, vault1Pos.y].generated)
        {
            vault1Pos = GetRandomVaultPos();
        }
        tiles[vault1Pos.x, vault1Pos.y].tileType = TileTypes.Vault;
        tiles[vault1Pos.x, vault1Pos.y].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lockedVault;
    }

    private bool CheckBounds(Vector2Int nextTilePos)
    {
        if (nextTilePos.x < 0 || nextTilePos.x >= gridSize) return false;
        if (nextTilePos.y < 0 || nextTilePos.y >= gridSize) return false;
        return true;
    }

    public void PlaceTile(Sprite sprite, Quaternion rotation, bool camera, bool[] directions, TileTypes tileType, int gearAmount)
    {
        nextTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
        nextTile.tileObject.transform.GetChild(0).rotation = rotation;
        nextTile.generated = true;
        nextTile.camera = camera;
        nextTile.directions = directions;
        nextTile.tileType = tileType;
        nextTile.gearAmount = gearAmount;
        SetTileGears(nextTile);
        if (camera) nextTile.tileObject.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void SetTileGears(Tile tile)
    {
        if (tile.gearAmount == 0)
        {
            tile.tileObject.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (tile.gearAmount == 1)
        {
            tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
            tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = oneGearSprite;
        }
        else
        {
            tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
            tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = twoGearSprite;
        }
    }

    private void RaiseAlert(Tile nextTile)
    {
        toNextAlertLevel++;
        if (toNextAlertLevel >= nextAlertCounter)
        {
            alertLevel++;
            toNextAlertLevel = 0;
            Vector2Int randomGuardPos = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
            while (!tiles[randomGuardPos.x, randomGuardPos.y].generated)
            {
                randomGuardPos = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
            }
            Instantiate(guard, new Vector3(randomGuardPos.x, randomGuardPos.y, 0f), Quaternion.identity);

        }
        UpdateUI();
        nextTile.cameraEnabled = false;
        nextTile.tileObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }
}
