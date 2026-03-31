using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    #region "Variables"
    [Header("Game Variables")]
    [SerializeField] private int gridSize; // Only odd numbers allowed
    [SerializeField] private int health;
    [SerializeField] private int vaultNumber;
    [SerializeField] private int nextAlertCounter;

    [Header("Assets")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject guard;
    [SerializeField] private Sprite lockedVault;
    [SerializeField] private Sprite openVault;
    [SerializeField] private Sprite oneGearSprite;
    [SerializeField] private Sprite twoGearSprite;
    [SerializeField] private List<StartTile> startTiles;

    [Header("References")]
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text rerollsText;
    [SerializeField] private TMP_Text holdsText;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject[] alertSegments;
    [SerializeField] private List<GameObject> objectives;

    [Header("Testing")]
    [SerializeField] private List<Tile> testtiles;

    [HideInInspector] public Vector2Int playerPos;
    [HideInInspector] public Tile[,] tiles;
    [HideInInspector] public List<Guard> guards;
    [HideInInspector] public bool generatingTiles;
    [HideInInspector] public Tile nextTile;

    private List<Vector2Int> availableDirections;
    private int gearAmount;
    private int rerolls;
    private int holds;
    private int toNextAlertLevel;
    private int alertLevel;
    private bool openVaultWhenSpawned;
    private Tile vaultTile;
    private bool gameOver;
    private bool currentlyMoving;
    private Keyboard keyboard;
    private bool prevGuardsDone;
    private bool guardsDone
    {
        get
        {
            if (guards.Count <= 0) return true;
            else
            {
                foreach (Guard g in guards)
                {
                    if (!g.turnDone) return false;
                }
                return true;
            }
        }
    }
    #endregion

    private void Start()
    {
        testtiles = new List<Tile>();
        tiles = new Tile[gridSize,gridSize];
        guards = new List<Guard>();
        for (int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                if (i == (gridSize - 1) / 2 && j == (gridSize - 1) / 2) //Change this if grid size changes
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

        availableDirections = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        playerPos = new Vector2Int((gridSize - 1) / 2, (gridSize - 1) / 2);
        keyboard = Keyboard.current;
    }

    private Vector2Int GetRandomVaultPos()
    {
        Vector2Int vaultPos = new Vector2Int();

        int upOrDown = Random.Range(0, 2);
        int leftOrRight = Random.Range(0, 2);

        if (upOrDown <= 0) vaultPos.x = Random.Range(gridSize - 3, gridSize);
        else vaultPos.x = Random.Range(0, 2);

        if (leftOrRight <= 0) vaultPos.y = Random.Range(gridSize - 3, gridSize);
        else vaultPos.y = Random.Range(0, 2);

        return vaultPos;
    }

    private void Update()
    {
        if (keyboard.pKey.wasPressedThisFrame) // Restart the game
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (gameOver || currentlyMoving || !guardsDone) return;

        NewMovement(keyboard.downArrowKey, 2);
        NewMovement(keyboard.upArrowKey, 0);
        NewMovement(keyboard.leftArrowKey, 3);
        NewMovement(keyboard.rightArrowKey, 1);

        if (keyboard.fKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].tileType != TileTypes.Default && tiles[playerPos.x, playerPos.y].remainingUses > 0 && !generatingTiles)
        {
            int gearCost = Mathf.Abs(tiles[playerPos.x, playerPos.y].gearAmount);
            switch (tiles[playerPos.x, playerPos.y].tileType)
            {
                case TileTypes.Control_Room:
                    if (gearAmount < gearCost) break;
                    SpawnVault();
                    UpdateRoomAfterUse(gearCost);
                    break;
                case TileTypes.Key_Room:
                    if (gearAmount < gearCost) break;
                    if (vaultTile != null)
                    {
                        vaultTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = openVault;
                        vaultTile.vaultOpen = true;
                    }
                    else openVaultWhenSpawned = true;
                    objectives[1].SetActive(true);
                    AudioManager.instance.PlaySound("markObjective", 0.5f);
                    UpdateRoomAfterUse(gearCost);
                    break;
                case TileTypes.Surveillance:
                    if (gearAmount < gearCost) break;
                    toNextAlertLevel = 0;
                    UpdateRoomAfterUse(gearCost);
                    break;
                case TileTypes.Med_Bay:
                    if (gearAmount < gearCost) break;
                    health++;
                    UpdateRoomAfterUse(gearCost);
                    break;
                case TileTypes.Chief_Office:
                    if (gearAmount < gearCost) break;
                    rerolls += 2;
                    UpdateRoomAfterUse(gearCost);
                    break;
                case TileTypes.Archives:
                    if (gearAmount < gearCost) break;
                    holds++;
                    UpdateRoomAfterUse(gearCost);
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
            AudioManager.instance.PlaySound("reroll", 0.5f);
        }
        if (keyboard.hKey.wasPressedThisFrame && generatingTiles && holds > 0) // Hold function
        {
            tileSelector.LockSelected();
            holds--;
            UpdateUI();
            AudioManager.instance.PlaySound("holdTile", 0.5f);
        }
        /*if (keyboard.zKey.wasPressedThisFrame) //Cheats, remove after
        {
            rerolls++;
            holds++;
            gearAmount += 2;
            UpdateUI();
        }*/

        if (guardsDone && !prevGuardsDone && guards.Count > 1) //Displace any guards on the same spot
        {
            List<Guard> guardsToMove = new List<Guard>();
            foreach (Guard g in guards)
            {
                foreach (Guard g2 in guards)
                {
                    if (g != g2 && g.GetCurrentPos() == g2.GetCurrentPos())
                    {
                        guardsToMove.Add(g);
                        guardsToMove.Add(g2);
                    }
                }
            }
            guardsToMove = guardsToMove.Distinct().ToList();
            if (guardsToMove.Count > 1)
            {
                guardsToMove[0].transform.position -= new Vector3(0.2f, 0, 0);
                guardsToMove[1].transform.position += new Vector3(0.2f, 0, 0);
            }
        }
        prevGuardsDone = guardsDone;
    }

    private void UpdateRoomAfterUse(int gearCost)
    {
        tiles[playerPos.x, playerPos.y].remainingUses--;
        gearAmount -= gearCost;
        tiles[playerPos.x, playerPos.y].tileObject.transform.GetChild(3).gameObject.SetActive(true);
        tiles[playerPos.x, playerPos.y].tileObject.transform.GetChild(2).gameObject.SetActive(false);
        AudioManager.instance.PlaySound("activateRoom", 0.5f);
    }

    private void UpdateUI()
    {
        alertText.text = alertLevel.ToString();
        healthText.text = "Health: " + health;
        gearText.text = "Kits: " + gearAmount;
        rerollsText.text = "Rerolls: " + rerolls;
        holdsText.text = "Holds: " + holds;
        for (int i = 0; i < alertSegments.Length; i++)
        {
            if (i <  toNextAlertLevel) alertSegments[i].SetActive(true);
            else alertSegments[i].SetActive(false);
        }
    }

    private void NewMovement(KeyControl key, int directionsIndex)
    {
        if (generatingTiles) return;
        if (key.wasReleasedThisFrame && tiles[playerPos.x, playerPos.y].directions[directionsIndex])
        {
            Vector2Int nextTilePosition = playerPos + availableDirections[directionsIndex];
            if (!CheckBounds(nextTilePosition)) return;
            nextTile = tiles[nextTilePosition.x, nextTilePosition.y];
            if (nextTile.tileType == TileTypes.Vault && !nextTile.vaultOpen)
            {
                return;
            }
            if (!nextTile.generated && tileSelector.GetTilesLeft() > 0)
            {
                tileSelector.StartSelection(GetOppositeDirection(directionsIndex));
                generatingTiles = true;
            }
            else if (nextTile.directions[GetOppositeDirection(directionsIndex)]) //Check if both directions are valid to let you move
            {
                Vector2Int previousPos = playerPos;
                StartCoroutine(SmoothMove(player.transform.position, nextTilePosition, 0.25f));
                playerPos = nextTilePosition;

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
                    AudioManager.instance.PlaySound("markObjective", 0.5f);
                    nextTile.remainingUses--;
                    AudioManager.instance.PlaySound("clearVault", 0.5f);
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
                    AudioManager.instance.PlaySound("gears", 0.5f);
                    UpdateUI();
                }
                if (nextTile.tileType == TileTypes.Start && vaultNumber <= 0)
                {
                    objectives[3].SetActive(true);
                    AudioManager.instance.PlaySound("markObjective", 0.5f);
                    gameOver = true;
                    winScreen.SetActive(true);
                    AudioManager.instance.PlaySound("win", 0.5f);
                }
            }
        }
    }
    private int GetOppositeDirection(int direction)
    {
        int result = 0;
        int totalNumber = direction + 2;
        if (totalNumber >= availableDirections.Count)
        {
            result = direction - availableDirections.Count + 2;
        }
        else result = totalNumber;

        return result;
    }

    private IEnumerator SmoothMove(Vector3 startPos, Vector2Int nextTilePos, float seconds)
    {
        currentlyMoving = true;
        float time = 0f;
        while (time < 1.0)
        {
            time += Time.deltaTime / seconds;
            player.transform.position = Vector3.Lerp(startPos, new Vector3(nextTilePos.x, nextTilePos.y, 0f), Mathf.SmoothStep(0f, 1f, time));
            yield return null;
        }
        currentlyMoving = false;
    }

    private void SpawnVault()
    {
        Vector2Int vault1Pos = GetRandomVaultPos();
        while (tiles[vault1Pos.x, vault1Pos.y].generated)
        {
            vault1Pos = GetRandomVaultPos();
        }
        int randomDirection = Random.Range(0, availableDirections.Count);
        Vector2Int vaultNeighbor = new Vector2Int(vault1Pos.x + availableDirections[randomDirection].x, vault1Pos.y + availableDirections[randomDirection].y);
        while (!CheckBounds(vaultNeighbor))
        {
            randomDirection = Random.Range(0, availableDirections.Count);
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
        AudioManager.instance.PlaySound("markObjective", 0.5f);
    }

    public bool CheckBounds(Vector2Int nextTilePos)
    {
        if (nextTilePos.x < 0 || nextTilePos.x >= gridSize) return false;
        if (nextTilePos.y < 0 || nextTilePos.y >= gridSize) return false;
        return true;
    }

    public void PlaceTile(Sprite sprite, Quaternion rotation, bool camera, bool[] directions, TileTypes tileType, int gearAmount, bool placeSound = false)
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
        else nextTile.tileObject.transform.GetChild(1).gameObject.SetActive(false);
        if (placeSound) AudioManager.instance.PlaySound("placeTile", 0.5f);
        else AudioManager.instance.PlaySound("selectTile", 0.5f);
    }

    public void SetTileGears(Tile tile)
    {
        switch (tile.gearAmount)
        {
            case 0:
                tile.tileObject.transform.GetChild(2).gameObject.SetActive(false);
                break;
            case 1:
                tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = oneGearSprite;
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
                break;
            case 2:
                tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = twoGearSprite;
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
                break;
            case -1:
                tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = oneGearSprite;
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                break;
            case -2:
                tile.tileObject.transform.GetChild(2).gameObject.SetActive(true);
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = twoGearSprite;
                tile.tileObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                break;
            default:
                break;
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
            AudioManager.instance.PlaySound("alarmUp", 0.5f);
        }
        else
        {
            AudioManager.instance.PlaySound("alarmTick", 0.5f);
        }
        UpdateUI();
        nextTile.cameraEnabled = false;
        nextTile.tileObject.transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
    }

    private void SpawnGuard()
    {
        List<Tile> generatedTiles = new List<Tile>();
        int maxDistance = -1;
        foreach (Tile t in tiles)
        {
            if (t.generated)
            {
                generatedTiles.Add(t);
                if (ManhattanDistance(playerPos, t.position) > maxDistance) maxDistance = ManhattanDistance(playerPos, t.position);
            }
        }
        Tile randomTile = generatedTiles[Random.Range(0, generatedTiles.Count)];

        if (maxDistance >= 2)
        {
            while (ManhattanDistance(randomTile.position, playerPos) < 2) //Spawn at least 2 tiles away if possible
            {
                randomTile = generatedTiles[Random.Range(0, generatedTiles.Count)];
            }
        }
        else
        {
            while (randomTile.position == playerPos)
            {
                randomTile = generatedTiles[Random.Range(0, generatedTiles.Count)];
            }
        }
        
        GameObject g = Instantiate(guard, new Vector3(randomTile.position.x, randomTile.position.y, 0f), Quaternion.identity);
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
        AudioManager.instance.PlaySound("lostLife", 0.5f);
        if (health < 0)
        {
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
