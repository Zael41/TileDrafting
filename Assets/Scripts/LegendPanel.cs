using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LegendPanel : MonoBehaviour
{
    #region "Variables"
    [Header("References")]
    [SerializeField] private List<GameObject> pages;
    [SerializeField] private TMP_Text numberText;

    private int currentPage;
    #endregion

    public void TurnPage(bool left)
    {
        currentPage += left ? -1 : 1;
        if (currentPage > pages.Count - 1) currentPage = 0;
        else if (currentPage < 0) currentPage = pages.Count - 1;
        for (int i = 0; i < pages.Count; i++)
        {
            if (i == currentPage) pages[i].SetActive(true);
            else pages[i].SetActive(false);
        }
        numberText.text = (currentPage + 1).ToString() + "/" + pages.Count;
        AudioManager.instance.PlaySound("pageTurn", 0.5f);
    }
}
