using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    private Vector2Int currentPos;
    private int lastDirection;
    private int moveCounter; //Need to implement this
    [SerializeField] private List<Vector2Int> availableDirections;
    private int randomIndex;
    private Tile nextTile;
    private Vector2Int nextTilePos;

    public bool Movement(Vector2Int prevPlayerPos)
    {
        bool collided = false;
        currentPos = new Vector2Int((int)(transform.position.x), (int)(transform.position.y));
        int manhattanDist = ManhattanDistance(currentPos, GridManager.instance.playerPos);
        manhattanDist = 0; // Only for testing
        if (manhattanDist <= 5)
        {
            int lastDirectionCounter = 0;
            CalculateMovement();
            while (randomIndex == GetOppositeDirection(lastDirection) && lastDirectionCounter < 5) //Prevent going back unless necessary or unlucky
            {
                CalculateMovement();
                lastDirectionCounter++;
            }
            if (currentPos == GridManager.instance.playerPos && prevPlayerPos == nextTilePos) //Passing damage
            {
                GridManager.instance.TakeDamage();
                collided = true;
            }
            if (nextTilePos == GridManager.instance.playerPos) //Direct hit damage
            {
                GridManager.instance.TakeDamage();
                collided = true;
            }
            transform.position = new Vector3(nextTilePos.x, nextTilePos.y, 0f);
            currentPos = new Vector2Int(nextTilePos.x, nextTilePos.y);
            lastDirection = randomIndex;
        }
        // Add pathfinding
        return collided;
    }

    private void CalculateMovement()
    {
        do
        {
            do
            {
                randomIndex = Random.Range(0, availableDirections.Count);
                nextTilePos = new Vector2Int(currentPos.x + availableDirections[randomIndex].x, currentPos.y + availableDirections[randomIndex].y);
            } while (!GridManager.instance.CheckBounds(nextTilePos));

            nextTile = GridManager.instance.tiles[nextTilePos.x, nextTilePos.y];
        } while (!nextTile.generated || (!nextTile.directions[GetOppositeDirection(randomIndex)] || !GridManager.instance.tiles[currentPos.x, currentPos.y].directions[randomIndex]));
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
