using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private List<GameObject> tileObjects;
    [SerializeField] private List<AvailableTiles> availableTiles;
    [SerializeField] private List<SelectableTiles> selectableTiles;
    private int selectedTile;
    private Keyboard keyboard;

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
        keyboard = Keyboard.current;
        selectedTile = -1;
    }

    private void Update()
    {
        if (selectedTile != -1)
        {
            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                tileObjects[selectedTile].transform.GetChild(0).gameObject.SetActive(false);
                selectedTile++;
                if (selectedTile > 2) selectedTile = 0;
                tileObjects[selectedTile].transform.GetChild(0).gameObject.SetActive(true);
            }
            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                tileObjects[selectedTile].transform.GetChild(0).gameObject.SetActive(false);
                selectedTile--;
                if (selectedTile < 0) selectedTile = 2;
                tileObjects[selectedTile].transform.GetChild(0).gameObject.SetActive(true);
            }
            if (keyboard.enterKey.wasPressedThisFrame)
            {
                gridManager.PlaceTile(tileObjects[selectedTile].GetComponent<Image>().sprite); //Need to add camera and directions
                selectedTile = -1;
                HideMenu();
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

    public void HideMenu()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void GenerateTiles()
    {
        foreach (GameObject obj in tileObjects)
        {
            int randomIndex = Random.Range(0, selectableTiles.Count);
            SelectableTiles randomTile = selectableTiles[randomIndex];
            obj.GetComponent<Image>().sprite = randomTile.tileSprite;
        }
        selectedTile = 0;
        tileObjects[selectedTile].transform.GetChild(0).gameObject.SetActive(true);
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
