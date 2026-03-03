using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class GridManager : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject guard;
    [SerializeField] private Sprite lockedVault;
    [SerializeField] private Sprite openVault;
    [SerializeField] private List<StartTile> startTiles;
    [SerializeField] private Sprite oneGearSprite;
    [SerializeField] private Sprite twoGearSprite;

    [Header("Game Variables")]
    [SerializeField] private int gridSize;
    public Vector2Int playerPos;
    [SerializeField] private int health;
    [SerializeField] private int vaultNumber;
    [SerializeField] private int nextAlertCounter;
    [SerializeField] private List<Vector2Int> availableDirections;
    private int gearAmount;
    private int rerolls;
    private int holds;
    private int toNextAlertLevel;
    private int alertLevel;
    private bool openVaultWhenSpawned;
    private Tile vaultTile;
    private bool gameOver;

    [Header("References")]
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text rerollsText;
    [SerializeField] private TMP_Text holdsText;
    [SerializeField] private List<GameObject> objectives;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    [SerializeField] private List<Tile> testtiles;

    public Tile[,] tiles;
    public List<Guard> guards;
    private Keyboard keyboard;
    private Tile nextTile;
    [HideInInspector] public bool generatingTiles;

    private void Start()
    {
        testtiles = new List<Tile>();
        tiles = new Tile[gridSize,gridSize];
        guards = new List<Guard>();
        for (int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                if (i == 4 && j == 4) //Change this if grid size changes
                {
                    StartTile randomStartTile = startTiles[Random.Range(0, startTiles.Count)];
                    tiles[i, j] = new Tile(new Vector2Int(i, j), Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform));
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = randomStartTile.tileSprite;
                    tiles[i, j].directions = randomStartTile.directions;
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
        if (keyboard.pKey.wasPressedThisFrame) // Restart the game
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (gameOver) return;

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
                    SpawnVault();
                    tiles[playerPos.x, playerPos.y].remainingUses--;
                    gearAmount -= 2;
                    break;
                case TileTypes.Key_Room:
                    if (gearAmount < 2) break;
                    if (vaultTile != null)
                    {
                        vaultTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = openVault;
                        vaultTile.vaultOpen = true;
                    }
                    else openVaultWhenSpawned = true;
                    objectives[1].SetActive(true);
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
        if (keyboard.qKey.wasPressedThisFrame && rerolls > 0) // Reroll function
        {
            tileSelector.GenerateTiles();
            if (generatingTiles) tileSelector.StartSelection(tileSelector.requiredDirection);
            rerolls--;
            UpdateUI();
        }
        if (keyboard.hKey.wasPressedThisFrame && generatingTiles && holds > 1) // Hold function
        {
            tileSelector.LockSelected();
            Debug.Log("locked");
        }
        if (keyboard.zKey.wasPressedThisFrame) //Cheats, remove after
        {
            rerolls++;
            holds++;
            gearAmount += 2;
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
            if (!nextTile.generated && tileSelector.GetTilesLeft() > 0)
            {
                tileSelector.StartSelection(oppositeDirectionIndex);
                generatingTiles = true;
            }
            else if (nextTile.directions[oppositeDirectionIndex]) //Check if both directions are valid to let you move
            {
                Vector2Int previousPos = playerPos;
                player.transform.position += new Vector3(directionVector.x, directionVector.y, 0f);
                playerPos += directionVector;
                if (guards.Count > 0)
                {
                    List<Guard> guardsThatCollided = new List<Guard>();
                    foreach (var guard in guards)
                    {
                        bool collided = guard.Movement(previousPos);
                        if (collided) guardsThatCollided.Add(guard);
                    }
                    if (guardsThatCollided.Count > 0)
                    {
                        foreach (var g in guardsThatCollided)
                        {
                            guards.Remove(g);
                            Destroy(g.gameObject);
                        }
                    }
                }
                if (nextTile.tileType == TileTypes.Vault && nextTile.vaultOpen && nextTile.remainingUses > 0)
                {
                    vaultNumber--;
                    RaiseAlertVault();
                    objectives[2].SetActive(true);
                }
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
                    objectives[3].SetActive(true);
                    gameOver = true;
                    winScreen.SetActive(true);
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
        int randomDirection = Random.Range(0, 4);
        Vector2Int vaultNeighbor = new Vector2Int(vault1Pos.x + availableDirections[randomDirection].x, vault1Pos.y + availableDirections[randomDirection].y);
        while (!CheckBounds(vaultNeighbor))
        {
            randomDirection = Random.Range(0, 4);
            vaultNeighbor = new Vector2Int(vault1Pos.x + availableDirections[randomDirection].x, vault1Pos.y + availableDirections[randomDirection].y);
        }
        vaultTile = tiles[vault1Pos.x, vault1Pos.y];
        vaultTile.tileType = TileTypes.Vault;
        vaultTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = openVaultWhenSpawned ? openVault : lockedVault;
        vaultTile.vaultOpen = openVaultWhenSpawned;
        vaultTile.directions[randomDirection] = true;
        for (int i = 0; i < randomDirection; i++)
        {
            vaultTile.tileObject.transform.GetChild(0).Rotate(0, 0, -90);
        }
        vaultTile.generated = true;
        objectives[0].SetActive(true);
    }

    public bool CheckBounds(Vector2Int nextTilePos)
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
            SpawnGuard();
        }
        UpdateUI();
        nextTile.cameraEnabled = false;
        nextTile.tileObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }

    private void SpawnGuard()
    {
        int maxDistance = -1;
        foreach (Tile t in tiles)
        {
            if (t.generated)
            {
                if (ManhattanDistance(playerPos, t.position) > maxDistance) maxDistance = ManhattanDistance(playerPos, t.position);
            }
        }

        Vector2Int randomGuardPos = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));

        if (maxDistance >= 2)
        {
            while (!tiles[randomGuardPos.x, randomGuardPos.y].generated || ManhattanDistance(randomGuardPos, playerPos) < 2) //Spawn at least 2 tiles away if possible
            {
                randomGuardPos = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
            }
        }
        else
        {
            while (!tiles[randomGuardPos.x, randomGuardPos.y].generated || randomGuardPos == playerPos)
            {
                randomGuardPos = new Vector2Int(Random.Range(0, 8), Random.Range(0, 8));
            }
        }
        
        GameObject g = Instantiate(guard, new Vector3(randomGuardPos.x, randomGuardPos.y, 0f), Quaternion.identity);
        guards.Add(g.GetComponent<Guard>());
    }

    private void RaiseAlertVault()
    {
        alertLevel++;
        SpawnGuard();
        UpdateUI();
    }

    public void TakeDamage()
    {
        health--;
        UpdateUI();
        if (health < 0)
        {
            Debug.Log("You Lose");
            gameOver = true;
            loseScreen.SetActive(true);
        }
    }

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }
}

[System.Serializable]
public class StartTile
{
    public Sprite tileSprite;
    public bool[] directions; // 0 - up, 1 - right, 2 - down, 3 - left
}
