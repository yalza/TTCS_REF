using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using Photon.Realtime;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public static class bl_Extensions
{
    /// <summary>
    /// Post score to the given player and sync over network
    /// </summary>
    public static void PostScore(this Player player, int ScoreToAdd = 0)
    {
        int current = player.GetPlayerScore();
        current = current + ScoreToAdd;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.ScoreKey] = current;

        player.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }

    /// <summary>
    /// Get the score of the given player
    /// </summary>
    public static int GetPlayerScore(this Player player)
    {
        int s = 0;

        if (player.CustomProperties.ContainsKey(PropertiesKeys.ScoreKey))
        {
            s = (int)player.CustomProperties[PropertiesKeys.ScoreKey];
            return s;
        }

        return s;
    }

    /// <summary>
    /// 
    /// </summary>
    public static int GetKills(this Player p)
    {
        int k = 0;
        if (p.CustomProperties.ContainsKey(PropertiesKeys.KillsKey))
        {
            k = (int)p.CustomProperties[PropertiesKeys.KillsKey];
            return k;
        }
        return k;
    }

    /// <summary>
    /// 
    /// </summary>
    public static int GetDeaths(this Player p)
    {
        int d = 0;
        if (p.CustomProperties.ContainsKey(PropertiesKeys.DeathsKey))
        {
            d = (int)p.CustomProperties[PropertiesKeys.DeathsKey];
            return d;
        }
        return d;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostKill(this Player p, int kills)
    {
        int current = p.GetKills();
        current = current + kills;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.KillsKey] = current;

        p.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostDeaths(this Player p, int deaths)
    {
        int current = p.GetDeaths();
        current = current + deaths;

        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PropertiesKeys.DeathsKey] = current;

        p.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }

    /// <summary>
    /// Get the score of the given team
    /// </summary>
    /// <returns></returns>
    public static int GetRoomScore(this Room room, Team team)
    {
        object teamId;
        if (team == Team.Team1)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team1Score, out teamId))
            {
                return (int)teamId;
            }
        } else if (team == Team.Team2)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team2Score, out teamId))
            {
                return (int)teamId;
            }
        }

        return 0;
    }

    /// <summary>
    /// Add the given score to the given team and sync over network
    /// </summary>
    public static void SetTeamScore(this Room r, Team t, int scoreToAdd = 1)
    {
        if (t == Team.None) return;

        int score = 0;
        score = r.GetRoomScore(t);
        score += scoreToAdd;
        string key = (t == Team.Team1) ? PropertiesKeys.Team1Score : PropertiesKeys.Team2Score;
        Hashtable h = new Hashtable();
        h.Add(key, score);
        r.SetCustomProperties(h);
    }

    /// <summary>
    /// Get the team in which the given player is affiliated to
    /// </summary>
    /// <returns></returns>
    public static Team GetPlayerTeam(this Player p)
    {
        if (p == null) return Team.None;
        object teamId;
        string t = (string)p.CustomProperties[PropertiesKeys.TeamKey];
        if (p.CustomProperties.TryGetValue(PropertiesKeys.TeamKey, out teamId))
        {
            switch ((string)teamId)
            {
                case "Team2":
                    return Team.Team2;
                case "Team1":
                    return Team.Team1;
                case "All":
                    return Team.All;
                case "None":
                    return Team.None;

            }
        }
        return Team.None;
    }

    /// <summary>
    /// Sync the player team in which the given player is affiliated
    /// </summary>
    public static void SetPlayerTeam(this Player player, Team team)
    {
        Hashtable PlayerTeam = new Hashtable();
        PlayerTeam.Add(PropertiesKeys.TeamKey, team.ToString());
        player.SetCustomProperties(PlayerTeam);
    }

    /// <summary>
    /// Get the team name by their identifier
    /// </summary>
    /// <returns></returns>
    public static string GetTeamName(this Team t)
    {
        switch (t)
        {
            case Team.Team1:
                return bl_GameData.Instance.Team1Name;
            case Team.Team2:
                return bl_GameData.Instance.Team2Name;
            default:
                return "Solo";
        }
    }

    /// <summary>
    /// Get the team color by their identifier
    /// </summary>
    /// <returns></returns>
    public static Color GetTeamColor(this Team t, float alpha = 0)
    {
        Color c = Color.white;//default color
        switch (t)
        {
            case Team.Team1:
               c = bl_GameData.Instance.Team1Color;
                break;
            case Team.Team2:
               c = bl_GameData.Instance.Team2Color;
                break;
            case Team.All:
                c = Color.white;
                break;
        }
        if(alpha > 0) { c.a = alpha; }

        return c;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static GameModeSettings GetModeInfo(this GameMode mode)
    {
        for (int i = 0; i < bl_GameData.Instance.gameModes.Count; i++)
        {
            if(bl_GameData.Instance.gameModes[i].gameMode == mode) { return bl_GameData.Instance.gameModes[i]; }
        }
        return null;
    }

    /// <summary>
    /// Save (locally) the given player class as the default
    /// </summary>
    private const string PLAYER_CLASS_KEY = "{0}.playerclass";
    public static void SavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        PlayerPrefs.SetInt(key, (int)pc);
    }

    /// <summary>
    /// Get the locally saved player class
    /// </summary>
    /// <returns></returns>
    public static PlayerClass GetSavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        int id = PlayerPrefs.GetInt(key, 0);
        PlayerClass pclass = (PlayerClass)id;

        return pclass;
    }

    /// <summary>
    /// Get the player name along with their user role (if there's any)
    /// </summary>
    /// <returns></returns>
    public static string NickNameAndRole(this Player p)
    {
        object role = "";
        if (p.CustomProperties.TryGetValue(PropertiesKeys.UserRole, out role))
        {
            return string.Format("<b>{1}</b> {0}", p.NickName, (string)role);
        }
        return p.NickName;
    }

    /// <summary>
    /// Get the complete game mode name by their identifier
    /// </summary>
    /// <returns></returns>
    public static string GetName(this GameMode mode)
    {
        GameModeSettings info = mode.GetModeInfo();
        if(info != null) { return info.ModeName; }
        else { return string.Format("Not define: " + mode.ToString()); }
    }

    /// <summary>
    /// Get the game mode info by their identifier
    /// </summary>
    /// <returns></returns>
    public static GameModeSettings GetGameModeInfo(this GameMode mode)
    {
        return bl_GameData.Instance.gameModes.Find(x => x.gameMode == mode);
    }

    /// <summary>
    /// is this player in the same team that local player?
    /// </summary>
    /// <returns></returns>
    public static bool isTeamMate(this Player p)
    {
        bool b = false;
        if(p.GetPlayerTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()) { b = true; }
        return b;
    }

    /// <summary>
    /// Get the player list of an specific team
    /// </summary>
    /// <returns></returns>
    public static Player[] GetPlayersInTeam(this Player[] player, Team team)
    {
        List<Player> list = new List<Player>();
        for(int i = 0; i < bl_PhotonNetwork.PlayerList.Length; i++)
        {
            if(bl_PhotonNetwork.PlayerList[i].GetPlayerTeam() == team) { list.Add(bl_PhotonNetwork.PlayerList[i]); }
        }
        return list.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Player GetPlayerWithHighestScore(Player[] playerList)
    {
        if (playerList == null) return null;
        if (playerList.Length <= 1) return playerList[0];

        int high = 0, index = 0;
        for (int i = 0; i < playerList.Length; i++)
        {
            int h = playerList[i].GetPlayerScore();
            if(h > high)
            {
                high = h;
                index = i;
            }
        }

        return playerList[index];
    }

    /// <summary>
    /// Get current gamemode
    /// </summary>
    public static GameMode GetGameMode(this RoomInfo room)
    {
        string gm = (string)room.CustomProperties[PropertiesKeys.GameModeKey];
        GameMode mode = (GameMode)Enum.Parse(typeof(GameMode), gm);
        return mode;
    }

    /// <summary>
    /// Get the current room info parsed in the custom MFPS class
    /// </summary>
    /// <returns></returns>
    public static MFPSRoomInfo GetRoomInfo(this Room room)
    {
        return new MFPSRoomInfo(room);
    }

    /// <summary>
    /// Check if the given team is the local player enemy team
    /// </summary>
    /// <returns></returns>
    public static Team OppsositeTeam(this Team team)
    {
        if (team == Team.Team1) { return Team.Team2; }
        else if (team == Team.Team2) { return Team.Team1; }
        else
        {
            return Team.All;
        }
    }

    /// <summary>
    /// Better solution for invoke methods after a certain time
    /// </summary>
    public static void InvokeAfter(this MonoBehaviour mono, float time, Action callback)
    {
        mono.StartCoroutine(WaitToExecute(time, callback));
    }

    static IEnumerator WaitToExecute(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null) callback.Invoke();
    }

    /// <summary>
    /// Re-size a rawImage to fit with the parent RectTransform size
    /// </summary>
    /// <returns></returns>
    public static Vector2 SizeToParent(this RawImage image, float padding = 0)
    {
        float w = 0, h = 0;
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();

        // check if there is something to do
        if (image.texture != null)
        {
            if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }

    /// <summary>
    /// Randomize some Audio Source properties to get a different audio output result
    /// </summary>
    public static void RandomizeAudioOutput(this AudioSource source)
    {
        source.pitch = UnityEngine.Random.Range(0.92f, 1.1f);
        source.spread = UnityEngine.Random.Range(0.98f, 1.25f);
    }

    /// <summary>
    /// Check if the collider is from the local player
    /// </summary>
    /// <returns></returns>
    public static bool isLocalPlayerCollider(this Collider collider)
    {
        return collider.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG);
    }

    /// <summary>
    /// Localization addon helper
    /// Get the localized text of the given key
    /// </summary>
    /// <returns></returns>
    public static string Localized(this string str, string key, bool plural = false)
    {
#if LOCALIZATION
        if (plural) { return bl_Localization.Instance.GetTextPlural(key); }
        return bl_Localization.Instance.GetText(key);
#else
        return str;
#endif
    }

    /// <summary>
    /// Localization addon helper
    /// Get the localized text of the given key
    /// </summary>
    /// <returns></returns>
    public static string Localized(this string str, int id, bool plural = false)
    {
#if LOCALIZATION
        if (plural) { return bl_Localization.Instance.GetTextPlural(id); }
        return bl_Localization.Instance.GetText(id);
#else
        return str;
#endif
    }

    /// <summary>
    /// Get an int array as a string array separated by the give char
    /// </summary>
    /// <returns></returns>
    public static string[] AsStringArray(this int[] array, string endoint = "") => array.Select(x => (x.ToString() + endoint)).ToArray();

    /// <summary>
    /// Check if a flag is selected in the give flag enum property
    /// </summary>
    /// <returns></returns>
    public static bool IsEnumFlagPresent<T>(this T value, T lookingForFlag) where T : Enum
    {
        int intValue = (int)(object)value;
        int intLookingForFlag = (int)(object)lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static MFPSPlayer[] ToMFPSPlayerList(this Player[] list)
    {
        var mfpsList = new MFPSPlayer[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            mfpsList[i] = bl_GameManager.Instance.GetMFPSPlayer(list[i].NickName);
        }
        return mfpsList;
    }
}