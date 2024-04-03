using UnityEngine;

public static class bl_StringUtility
{

    /// <summary>
    /// Get string in time format
    /// </summary>
    public static string GetTimeFormat(float m, float s)
    {
        return string.Format("{0:00}:{1:00}", m, s);
    }

    /// <summary>
    /// 
    /// </summary>
    public static string GetTimeFormat(int seconds)
    {
        int minutes = seconds > 60 ? seconds / 60 : 0;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Make a displayable name for a variable like name.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NicifyVariableName(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        string result = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (i == 0)
            {
                result += char.ToUpper(str[i]);
                continue;
            }
            if (char.IsUpper(str[i]) == true && i != 0)
            {
                result += " ";
            }

            result += str[i];
        }
        return result;
    }

    /// <summary>
    /// Generate a random string with the given length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GenerateKey(int length = 7)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghlmnopqrustowuvwxyz";
        string key = "";
        for (int i = 0; i < length; i++)
        {
            key += chars[Random.Range(0, chars.Length)];
        }
        return key;
    }
}