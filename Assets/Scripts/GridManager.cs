using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int gridSize;
    [SerializeField] private Vector2Int playerPos;
    [SerializeField] private Sprite vaultTile;
    [SerializeField] private List<Tile> testtiles;
    private Tile[,] tiles;
    private Keyboard keyboard;

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
                if (i == vault1Pos.x && j == vault1Pos.y)
                {
                    tiles[i, j] = new Tile(vault1Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), true);
                    tiles[i, j].tileObject.GetComponent<SpriteRenderer>().sprite = vaultTile;
                }
                else if (i == vault2Pos.x && j == vault2Pos.y)
                {
                    tiles[i, j] = new Tile(vault2Pos, Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform), true);
                    tiles[i, j].tileObject.GetComponent<SpriteRenderer>().sprite = vaultTile;
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
        if (keyboard.wKey.wasPressedThisFrame)
        {
            Debug.Log("test W key");
        }
    }
}
