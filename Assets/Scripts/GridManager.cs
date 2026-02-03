using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject vaultPrefab;
    [SerializeField] private int gridSize;
    //[SerializeField] private List<GameObject> testtiles;
    private GameObject[,] tiles;

    private void Start()
    {
        Vector2Int vault1Pos = GetRandomVaultPos();
        Vector2Int vault2Pos = GetRandomVaultPos();
        while (vault2Pos == vault1Pos)
        {
            vault2Pos = GetRandomVaultPos();
        }
        //testtiles = new List<GameObject>();
        tiles = new GameObject[gridSize,gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                if (i == vault1Pos.x && j == vault1Pos.y)
                {
                    tiles[i, j] = Instantiate(vaultPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform);
                }
                else if (i == vault2Pos.x && j == vault2Pos.y)
                {
                    tiles[i, j] = Instantiate(vaultPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform);
                }
                else
                {
                    tiles[i, j] = Instantiate(slotPrefab, new Vector3(i, j, 0f), Quaternion.identity, this.transform);
                }
                //testtiles.Add(tiles[i, j]);
            }
        }
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
}
