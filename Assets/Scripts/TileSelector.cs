using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    [SerializeField] List<GameObject> tileObjects;
    [SerializeField] List<AvailableTiles> availableTiles;
    [SerializeField] List<SelectableTiles> selectableTiles;

    private void Start()
    {
        selectableTiles = new List<SelectableTiles>();
        foreach (AvailableTiles tile in availableTiles)
        {
            for (int i = 0; i < tile.amountWithoutCamera; i++)
            {
                selectableTiles.Add(new SelectableTiles(tile.tileSprite, false));
            }
            for (int i = 0; i < tile.amountWithCamera; i++)
            {
                selectableTiles.Add(new SelectableTiles(tile.tileSprite, true));
            }
        }
    }

    public void ShowMenu()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }

    public void GenerateTiles()
    {
        foreach (GameObject obj in tileObjects)
        {

        }
    }
}

[System.Serializable]
public class AvailableTiles //Used by the developer to tell the game how many tiles to generate
{
    public Sprite tileSprite;
    public int amountWithoutCamera;
    public int amountWithCamera;
}

[System.Serializable]
public class SelectableTiles //The actual X amount of tiles that the player will choose from
{
    public Sprite tileSprite;
    public bool camera;

    public SelectableTiles(Sprite tileSprite, bool camera)
    {
        this.tileSprite = tileSprite;
        this.camera = camera;
    }
}
