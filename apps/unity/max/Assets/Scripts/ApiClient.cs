using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[Serializable]
public class User
{
    public string id;
    public string username;
    public string email;
    public int totalXP;
    public int level;
}

[Serializable]
public class Badge
{
    public string id;
    public string userId;
    public string badgeId;
    public string badgeName;
    public string status;
    public string achievedAt;
    public int badgeXP;
}

[Serializable]
public class Mission
{
    public string id;
    public string userId;
    public string missionId;
    public string missionName;
    public string status;
    public string startedAt;
    public string completedAt;
    public int missionXP;
}

public static class JsonHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }

    public static T[] FromJson<T>(string json)
    {
        string wrapped = "{ \"array\": " + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(wrapped).array;
    }
}


public class ApiClient : MonoBehaviour
{
    private static readonly string baseUrl = "http://127.0.0.1:8000";

    // -------- USER --------
    public IEnumerator GetUser(Action<User> onSuccess)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/user"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                User user = JsonUtility.FromJson<User>(req.downloadHandler.text);
                onSuccess?.Invoke(user);
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }

    // -------- BADGES --------
    public IEnumerator GetBadges(Action<Badge[]> onSuccess)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/badges"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Badge[] badges = JsonHelper.FromJson<Badge>(req.downloadHandler.text);
                onSuccess?.Invoke(badges);
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }

    public IEnumerator AchieveBadge(string badgeId, Action onSuccess = null)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(baseUrl + "/badges/" + badgeId + "/achieve", ""))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }

    // -------- MISSIONS --------
    public IEnumerator GetMissions(Action<Mission[]> onSuccess)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/missions"))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Mission[] missions = JsonHelper.FromJson<Mission>(req.downloadHandler.text);
                onSuccess?.Invoke(missions);
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }

    public IEnumerator StartMission(string missionId, Action onSuccess = null)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(baseUrl + "/missions/" + missionId + "/start", ""))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }

    public IEnumerator CompleteMission(string missionId, Action onSuccess = null)
    {
        using (UnityWebRequest req = UnityWebRequest.PostWwwForm(baseUrl + "/missions/" + missionId + "/complete", ""))
        {
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError(req.error);
            }
        }
    }
}