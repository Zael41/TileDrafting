using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int gridSize;
    [SerializeField] private Vector2Int playerPos;
    [SerializeField] private GameObject player;
    [SerializeField] private Sprite vaultTile;
    [SerializeField] private Sprite startTile;
    [SerializeField] private TileSelector tileSelector;
    [SerializeField] private List<Tile> testtiles;
    private Tile[,] tiles;
    private Keyboard keyboard;
    private Tile nextTile;
    public bool generatingTiles;

    private void Start()
    {
        Vector2Int vault1Pos = GetRandomVaultPos();
        Vector2Int vault2Pos = GetRandomVaultPos();
        while (vault2Pos == vault1Pos)
        {
            vault2Pos = GetRandomVaultPos();
        }
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
                }
                else if (i == vault1Pos.x && j == vault1Pos.y)
                {
                    tiles[i, j] = new Tile(vault1Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), true);
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = vaultTile;
                }
                else if (i == vault2Pos.x && j == vault2Pos.y)
                {
                    tiles[i, j] = new Tile(vault2Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), true);
                    tiles[i, j].tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = vaultTile;
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
        if (generatingTiles) return;
        if (keyboard.sKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[2])
        {
            nextTile = tiles[playerPos.x, playerPos.y - 1];
            if (!nextTile.generated)
            {
                tileSelector.ShowMenu();
                tileSelector.GenerateTiles();
                tileSelector.requiredDirection = 0;
                generatingTiles = true;
            }
            else
            {
                player.transform.position += new Vector3(0f, -1f, 0f);
                playerPos += new Vector2Int(0, -1);
            }
        }
        if (keyboard.wKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[0])
        {
            nextTile = tiles[playerPos.x, playerPos.y + 1];
            if (!nextTile.generated)
            {
                tileSelector.ShowMenu();
                tileSelector.GenerateTiles();
                tileSelector.requiredDirection = 2;
                generatingTiles = true;
            }
            else
            {
                player.transform.position += new Vector3(0f, 1f, 0f);
                playerPos += new Vector2Int(0, 1);
            }
        }
        if (keyboard.aKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[3])
        {
            nextTile = tiles[playerPos.x - 1, playerPos.y];
            if (!nextTile.generated)
            {
                tileSelector.ShowMenu();
                tileSelector.GenerateTiles();
                tileSelector.requiredDirection = 1;
                generatingTiles = true;
            }
            else
            {
                player.transform.position += new Vector3(-1f, 0f, 0f);
                playerPos += new Vector2Int(-1, 0);
            }
        }
        if (keyboard.dKey.wasPressedThisFrame && tiles[playerPos.x, playerPos.y].directions[1])
        {
            nextTile = tiles[playerPos.x + 1, playerPos.y];
            if (!nextTile.generated)
            {
                tileSelector.ShowMenu();
                tileSelector.GenerateTiles();
                tileSelector.requiredDirection = 3;
                generatingTiles = true;
            }
            else
            {
                player.transform.position += new Vector3(1f, 0f, 0f);
                playerPos += new Vector2Int(1, 0);
            }
        }
    }

    public void PlaceTile(Sprite sprite, Quaternion rotation, bool camera, bool[] directions)
    {
        nextTile.tileObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
        nextTile.tileObject.transform.GetChild(0).rotation = rotation;
        nextTile.generated = true;
        nextTile.camera = camera;
        nextTile.directions = directions;
        if (camera) nextTile.tileObject.transform.GetChild(1).gameObject.SetActive(true);
    }
}
