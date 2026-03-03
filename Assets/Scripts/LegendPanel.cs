using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LegendPanel : MonoBehaviour
{
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private TMP_Text numberText;
    private int currentPage;

    public void TurnPageRight()
    {
        currentPage++;
        if (currentPage > pages.Count - 1) currentPage = 0;
        for (int i = 0; i < pages.Count; i++)
        {
            if (i == currentPage) pages[i].SetActive(true);
            else pages[i].SetActive(false);
        }
        numberText.text = (currentPage + 1).ToString() + "/" + pages.Count;
    }

    public void TurnPageLeft()
    {
        currentPage--;
        if (currentPage < 0) currentPage = pages.Count - 1;
        for (int i = 0; i < pages.Count; i++)
        {
            if (i == currentPage) pages[i].SetActive(true);
            else pages[i].SetActive(false);
        }
        numberText.text = (currentPage + 1).ToString() + "/" + pages.Count;
    }
}
