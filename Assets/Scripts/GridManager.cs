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

    [Header("Game Variables")]
    [SerializeField] private int gridSize;
    [SerializeField] private Vector2Int playerPos;
    [SerializeField] private int health;
    [SerializeField] private int vaultNumber;
    [SerializeField] private int nextAlertCounter;
    private int toNextAlertLevel;
    private int alertLevel;

    [Header("References")]
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private TMP_Text healthText;
    
    [SerializeField] private List<Tile> testtiles;

    private Tile[,] tiles;
    private List<Tile> vaultTiles;
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
        Vector2Int vault1Pos = GetRandomVaultPos();
        Vector2Int vault2Pos = GetRandomVaultPos();
        while (vault2Pos == vault1Pos)
        {
            vault2Pos = GetRandomVaultPos();
        }
        testtiles = new List<Tile>();
        vaultTiles = new List<Tile>();
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
                else if (i == vault1Pos.x && j == vault1Pos.y)
                {
                    tiles[i, j] = new Tile(vault1Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), TileTypes.Vault);
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lockedVault;
                    vaultTiles.Add(tiles[i, j]);
                }
                else if (i == vault2Pos.x && j == vault2Pos.y)
                {
                    tiles[i, j] = new Tile(vault2Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), TileTypes.Vault);
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lockedVault;
                    vaultTiles.Add(tiles[i, j]);
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
        NewMovement(keyboard.sKey, 2, new Vector2Int(0, -1), 0);
        NewMovement(keyboard.wKey, 0, new Vector2Int(0, 1), 2);
        NewMovement(keyboard.aKey, 3, new Vector2Int(-1, 0), 1);
        NewMovement(keyboard.dKey, 1, new Vector2Int(1, 0), 3);

        if (keyboard.fKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].tileType != TileTypes.Default && tiles[playerPos.x, playerPos.y].remainingUses > 0) //Check for coins too
        {
            switch (tiles[playerPos.x, playerPos.y].tileType)
            {
                case TileTypes.Control_Room:
                    if (vaultTiles.Count <= 0) break;
                    int choice = Random.Range(0, vaultTiles.Count);
                    vaultTiles[choice].vaultOpen = true;
                    vaultTiles[choice].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = openVault;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    vaultTiles.RemoveAt(choice);
                    break;
                case TileTypes.Surveillance:
                    toNextAlertLevel = 0;
                    alertText.text = "Alert: " + alertLevel + "\n" + "Counter: " + toNextAlertLevel + "/" + nextAlertCounter;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    break;
                case TileTypes.Med_Bay:
                    health++;
                    healthText.text = "Health: " + health;
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    break;
                default:
                    break;
            }
        }
    }

    private void NewMovement(KeyControl key, int directionsIndex, Vector2Int directionVector, int oppositeDirectionIndex)
    {
        // Draft tiles before even moving
        if (generatingTiles) return;
        if (key.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[directionsIndex])
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
                if (nextTile.tileType == TileTypes.Start && vaultNumber <= 0)
                {
                    Debug.Log("you win");
                }
            }
        }
    }

    private bool CheckBounds(Vector2Int nextTilePos)
    {
        if (nextTilePos.x < 0 || nextTilePos.x >= gridSize) return false;
        if (nextTilePos.y < 0 || nextTilePos.y >= gridSize) return false;
        return true;
    }

    public void PlaceTile(Sprite sprite, Quaternion rotation, bool camera, bool[] directions, TileTypes tileType)
    {
        nextTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
        nextTile.tileObject.transform.GetChild(0).rotation = rotation;
        nextTile.generated = true;
        nextTile.camera = camera;
        nextTile.directions = directions;
        nextTile.tileType = tileType;
        if (camera) nextTile.tileObject.transform.GetChild(1).gameObject.SetActive(true);
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
        alertText.text = "Alert: " + alertLevel + "\n" + "Counter: " + toNextAlertLevel + "/" + nextAlertCounter;
        nextTile.cameraEnabled = false;
        nextTile.tileObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }
}
