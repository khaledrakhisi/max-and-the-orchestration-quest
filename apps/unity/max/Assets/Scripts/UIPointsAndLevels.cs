using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class UIPointsAndLevels : MonoBehaviour
{
    private int level = 1;
    private int xp_points = 0;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] public InfoBoard hintBoard;
    private ApiClient apiClient = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(apiClient.GetUser(OnUserLoaded));
    }

    void OnUserLoaded(User user)
    {
        // Debug.Log("User: " + user.username + " xp: " + user.totalXP + " level" + user.level);
        if (user != null)
        {
            DoSetLevel(user.level.ToString());
            DoAddPoints(user.totalXP.ToString());
        }
    }

    [Serializable]
    public class UpdateUserXPRequest
    {
        public int totalXP;
        public int level;
    }

    public IEnumerator UpdateUserXP(string userId, int totalXP, int level, Action<User> onSuccess = null)
    {
        string json = JsonUtility.ToJson(new UpdateUserXPRequest { totalXP = totalXP, level = level });
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest("http://127.0.0.1:8000/users/" + userId, "PATCH"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                User updated = JsonUtility.FromJson<User>(req.downloadHandler.text);
                onSuccess?.Invoke(updated);
            }
            else
            {
                Debug.LogError($"UpdateUserXP failed: {req.responseCode} - {req.error}");
            }
        }
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

            DoSaveUI();
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
            xp_points += Convert.ToInt32(pnts);
            if (pointsText)
            {
                pointsText.text = xp_points.ToString();
            }

            DoSaveUI();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    public int DoGetPoints()
    {
        return xp_points;
    }

    public void DoSaveUI()
    {
        StartCoroutine(UpdateUserXP("693843f9a8cdbf214cd36a62", xp_points, level, (updatedUser) => { Debug.Log($"New XP: {updatedUser.totalXP}"); }));
    }
}
