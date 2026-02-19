using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    private Vector2Int currentPos;
    private Vector2Int lastDirection;
    private int moveCounter;
    [SerializeField] private List<Vector2Int> availableDirections;

    public void Movement()
    {
        //Debug.Log(transform.position);
        currentPos = new Vector2Int((int)(transform.position.x), (int)(transform.position.y));
        //Debug.Log(currentPos);
        int manhattanDist = ManhattanDistance(currentPos, GridManager.instance.playerPos);
        manhattanDist = 0;
        if (manhattanDist <= 5)
        {
            int randomIndex = Random.Range(0, availableDirections.Count);
            //Debug.Log(randomIndex);
            //Debug.Log(availableDirections[randomIndex].x);
            Vector2Int nextTilePos = new Vector2Int(currentPos.x + availableDirections[randomIndex].x, currentPos.y + availableDirections[randomIndex].y);
            //Debug.Log(nextTilePos);
            Tile nextTile = GridManager.instance.tiles[nextTilePos.x, nextTilePos.y];
            while (!nextTile.generated || (!nextTile.directions[GetOppositeDirection(randomIndex)] || !GridManager.instance.tiles[currentPos.x, currentPos.y].directions[randomIndex]))
            {
                randomIndex = Random.Range(0, availableDirections.Count);
                nextTilePos = new Vector2Int(currentPos.x + availableDirections[randomIndex].x, currentPos.y + availableDirections[randomIndex].y);
                nextTile = GridManager.instance.tiles[nextTilePos.x, nextTilePos.y];
            }
            Debug.Log(nextTile.generated);
            Debug.Log(randomIndex + " (" + nextTilePos.x + ", " + nextTilePos.y + ")"); 
            transform.position = new Vector3(nextTilePos.x, nextTilePos.y, 0f);
            currentPos = new Vector2Int(nextTilePos.x, nextTilePos.y);
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

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return (int)(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }
}
