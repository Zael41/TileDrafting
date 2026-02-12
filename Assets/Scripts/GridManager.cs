using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public class GridManager : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private Sprite lockedVault;
    [SerializeField] private Sprite openVault;
    [SerializeField] private Sprite startTile;

    [Header("Game Variables")]
    [SerializeField] private int gridSize;
    [SerializeField] private Vector2Int playerPos;
    [SerializeField] private int health;
    [SerializeField] private int alert;
    [SerializeField] private int vaultNumber;

    [Header("References")]
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private TMP_Text alertText;
    
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

    private void Update() // There has to be a way to unify all this repeating code
    {
        if (generatingTiles) return;
        if (keyboard.sKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[2])
        {
            nextTile = tiles[playerPos.x, playerPos.y - 1];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated)
            {
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen)
                {
                    nextTile.generated = true;
                    nextTile.directions = new bool[4] {true, false, false, false};
                    player.transform.position += new Vector3(0f, -1f, 0f);
                    playerPos += new Vector2Int(0, -1);
                    vaultNumber--;
                }
                else
                {
                    tileSelector.ShowMenu();
                    tileSelector.GenerateTiles();
                    tileSelector.requiredDirection = 0;
                    generatingTiles = true;
                }
            }
            else
            {
                //Check if tile is locked to prevent movement
                player.transform.position += new Vector3(0f, -1f, 0f);
                playerPos += new Vector2Int(0, -1);
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
        if (keyboard.wKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[0])
        {
            nextTile = tiles[playerPos.x, playerPos.y + 1];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated)
            {
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen)
                {
                    nextTile.generated = true;
                    nextTile.directions = new bool[4] { false, false, true, false };
                    player.transform.position += new Vector3(0f, 1f, 0f);
                    playerPos += new Vector2Int(0, 1);
                    vaultNumber--;
                }
                else
                {
                    tileSelector.ShowMenu();
                    tileSelector.GenerateTiles();
                    tileSelector.requiredDirection = 2;
                    generatingTiles = true;
                }
            }
            else
            {
                player.transform.position += new Vector3(0f, 1f, 0f);
                playerPos += new Vector2Int(0, 1);
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
        if (keyboard.aKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[3])
        {
            nextTile = tiles[playerPos.x - 1, playerPos.y];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated)
            {
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen)
                {
                    nextTile.generated = true;
                    nextTile.directions = new bool[4] { false, true, false, false };
                    player.transform.position += new Vector3(-1f, 0f, 0f);
                    playerPos += new Vector2Int(-1, 0);
                    vaultNumber--;
                }
                else
                {
                    tileSelector.ShowMenu();
                    tileSelector.GenerateTiles();
                    tileSelector.requiredDirection = 1;
                    generatingTiles = true;
                }
            }
            else
            {
                player.transform.position += new Vector3(-1f, 0f, 0f);
                playerPos += new Vector2Int(-1, 0);
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
        if (keyboard.dKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[1])
        {
            nextTile = tiles[playerPos.x + 1, playerPos.y];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated)
            {
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen)
                {
                    nextTile.generated = true;
                    nextTile.directions = new bool[4] { false, false, false, true };
                    player.transform.position += new Vector3(1f, 0f, 0f);
                    playerPos += new Vector2Int(1, 0);
                    vaultNumber--;
                }
                else
                {
                    tileSelector.ShowMenu();
                    tileSelector.GenerateTiles();
                    tileSelector.requiredDirection = 3;
                    generatingTiles = true;
                }
            }
            else
            {
                player.transform.position += new Vector3(1f, 0f, 0f);
                playerPos += new Vector2Int(1, 0);
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
                default:
                    break;
            }
        }
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
        alert++;
        alertText.text = "Alert: " + alert;
        nextTile.cameraEnabled = false;
        nextTile.tileObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }
}
