using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIPointsAndLevels : MonoBehaviour
{
    private int level = 1;
    private int points = 0;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] public InfoBoard hintBoard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DoSetLevel(string lvl)
    {
        try
        {
            level = Convert.ToInt32(lvl);
            if (levelText)
            {
                levelText.text = level.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    public int DoGetLevel()
    {
        return level;
    }

    public void DoAddPoints(string pnts)
    {
        try
        {
            points += Convert.ToInt32(pnts);
            if (pointsText)
            {
                pointsText.text = points.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    public int DoGetPoints()
    {
        return points;
    }
}
