using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    private Vector2Int currentPos;
    private int lastDirection; //Because we check this before it's ever set the guard doesn't like moving up on the first move, not a big deal though
    private int moveCounter; //Need to implement this
    [SerializeField] private List<Vector2Int> availableDirections;
    private int randomIndex;
    private Tile nextTile;
    private Vector2Int nextTilePos;
    private int nextDirection;
    private GridManager gridManager;
    public bool turnDone;

    //Pathfinding
    List<Tile> searchedTiles;
    List<Tile> tilesToSearch;
    List<Tile> finalPath;

    private void Start()
    {
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        currentPos = new Vector2Int((int)(transform.position.x), (int)(transform.position.y));
        MovementPrediction();
    }

    public void MovementPrediction()
    {
        int manhattanDist = ManhattanDistance(currentPos, gridManager.playerPos);
        //manhattanDist = 9;
        if (manhattanDist <= 5) //Pseudo-random movement
        {
            int lastDirectionCounter = 0;
            CalculateMovement();
            while (randomIndex == GetOppositeDirection(lastDirection) && lastDirectionCounter < 5) //Prevent going back unless necessary or unlucky
            {
                CalculateMovement();
                lastDirectionCounter++;
            }
            nextDirection = randomIndex;
            gameObject.transform.rotation = Quaternion.identity;
            for (int i = 0; i < randomIndex; i++)
            {
                gameObject.transform.Rotate(0, 0, -90);
            }
        }
        else //A star pathfinding to get closer
        {
            ResetGridValues();
            Tile currentTile = gridManager.tiles[currentPos.x, currentPos.y];
            Tile playerTile = gridManager.tiles[gridManager.playerPos.x, gridManager.playerPos.y];
            searchedTiles = new List<Tile>();
            tilesToSearch = new List<Tile>() { currentTile };
            finalPath = new List<Tile>();

            currentTile.gCost = 0;
            currentTile.hCost = GetDistance(currentPos, gridManager.playerPos);
            currentTile.fCost = GetDistance(currentPos, gridManager.playerPos);

            while (tilesToSearch.Count > 0)
            {
                Tile tileToSearch = tilesToSearch[0];

                foreach (Tile tile in tilesToSearch)
                {
                    if (tile.fCost < tileToSearch.fCost || tile.fCost == tileToSearch.fCost && tile.hCost < tileToSearch.hCost)
                    {
                        tileToSearch = tile;
                    }
                }

                tilesToSearch.Remove(tileToSearch);
                searchedTiles.Add(tileToSearch);

                if (tileToSearch == playerTile)
                {
                    while (playerTile.position != currentPos)
                    {
                        //Debug.Log(playerTile.position);
                        finalPath.Add(playerTile);
                        playerTile = playerTile.path;
                    }
                    nextTilePos = finalPath[finalPath.Count - 1].position;
                    gameObject.transform.rotation = Quaternion.identity;
                    Vector2Int direction = nextTilePos - currentPos;
                    int directionIndex = availableDirections.IndexOf(direction);
                    lastDirection = directionIndex;
                    for (int i = 0; i < directionIndex; i++)
                    {
                        gameObject.transform.Rotate(0, 0, -90);
                    }
                    turnDone = true;
                    return;
                }

                SearchNeighbors(tileToSearch, playerTile);

            }
        }
        turnDone = true;
        return;
    }

    public bool Movement(Vector2Int prevPlayerPos)
    {
        bool collided = false;
        turnDone = false;
        if (currentPos == gridManager.playerPos && prevPlayerPos == nextTilePos) //Passing damage
        {
            gridManager.TakeDamage();
            collided = true;
        }
        if (nextTilePos == gridManager.playerPos) //Direct hit damage
        {
            gridManager.TakeDamage();
            collided = true;
        }
        //Debug.Log(currentPos + " GuardPos");
        //Debug.Log(gridManager.playerPos + " PlayerPos");
        //Debug.Log(nextTilePos + " NextGuardPos");

        /*transform.position = new Vector3(nextTilePos.x, nextTilePos.y, 0f);
        currentPos = new Vector2Int(nextTilePos.x, nextTilePos.y);
        lastDirection = nextDirection;*/

        StartCoroutine(SmoothMove(transform.position, nextTilePos, 0.25f));

        return collided;
    }

    private IEnumerator SmoothMove(Vector3 startPos, Vector2Int endPos, float seconds)
    {
        float time = 0f;
        while (time < 1.0)
        {
            time += Time.deltaTime / seconds;
            transform.position = Vector3.Lerp(startPos, new Vector3(endPos.x, endPos.y, 0f), Mathf.SmoothStep(0f, 1f, time));
            yield return null;
        }
        currentPos = new Vector2Int(nextTilePos.x, nextTilePos.y);
        lastDirection = nextDirection;

        MovementPrediction();
    }

    private void ResetGridValues()
    {
        foreach (Tile t in gridManager.tiles)
        {
            t.gCost = int.MaxValue;
            t.hCost = int.MaxValue;
            t.fCost = int.MaxValue;
            t.path = null;
        }
    }

    public Vector2Int GetCurrentPos()
    {
        return currentPos;
    }

    private void SearchNeighbors(Tile currentTile, Tile endTile)
    {
        Vector2Int tilePos = currentTile.position;
        List<Vector2Int> neighborPositions = new List<Vector2Int>() { new Vector2Int(tilePos.x, tilePos.y + 1),
                                                                      new Vector2Int(tilePos.x + 1, tilePos.y),
                                                                      new Vector2Int(tilePos.x, tilePos.y - 1),
                                                                      new Vector2Int(tilePos.x - 1, tilePos.y)};
        List<Vector2Int> validNeighborPositions = new List<Vector2Int>();

        foreach (Vector2Int pos in neighborPositions)
        {
            if (gridManager.CheckBounds(pos))
            {
                validNeighborPositions.Add(pos);
            }
            else
            {
                validNeighborPositions.Add(new Vector2Int(-99, -99)); //We do this to not screw up with the indexes
            }
        }

        List<Tile> validNeighbors = new List<Tile>();

        for (int i = 0; i < validNeighborPositions.Count; i++)
        {
            if (validNeighborPositions[i].x == -99) continue;
            Tile neighborTile = gridManager.tiles[validNeighborPositions[i].x, validNeighborPositions[i].y];
            if (currentTile.directions[i] && neighborTile.directions[GetOppositeDirection(i)])
            {
                validNeighbors.Add(neighborTile);
            }
        }

        foreach (Tile tile in validNeighbors)
        {
            if (!searchedTiles.Contains(tile))
            {
                int gCostToNeighbor = currentTile.gCost + GetDistance(currentTile.position, tile.position);

                if (gCostToNeighbor < tile.gCost)
                {
                    tile.path = currentTile;
                    tile.gCost = gCostToNeighbor;
                    tile.hCost = GetDistance(tile.position, endTile.position);
                    tile.fCost = tile.gCost + tile.hCost;

                    if (!tilesToSearch.Contains(tile))
                    {
                        tilesToSearch.Add(tile);
                    }
                }
            }
        }
    }

    private void CalculateMovement()
    {
        do
        {
            do
            {
                randomIndex = Random.Range(0, availableDirections.Count);
                nextTilePos = new Vector2Int(currentPos.x + availableDirections[randomIndex].x, currentPos.y + availableDirections[randomIndex].y);
            } while (!gridManager.CheckBounds(nextTilePos));

            nextTile = gridManager.tiles[nextTilePos.x, nextTilePos.y];
        } while (!nextTile.generated || (!nextTile.directions[GetOppositeDirection(randomIndex)] || !gridManager.tiles[currentPos.x, currentPos.y].directions[randomIndex]));
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

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }

    private int GetDistance(Vector2Int a, Vector2Int b) //Not sure if calculating diagonal distance when you can't move diagonally is smart, but it works
    {
        Vector2Int distance = new Vector2Int(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        int lowest = Mathf.Min(distance.x, distance.y);
        int highest = Mathf.Max(distance.x, distance.y);

        int horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }
}
