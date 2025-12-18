using UnityEngine;

public static class Unlocks
{
    private const string MusicModeKeyP1 = "MusicModeUnlocked";
    private const string MusicModeKeyP2 = "MusicModeUnlocked2";
    
    private const string Effect0Key = "Effect0Unlocked";
    private const string Effect1Key = "Effect1Unlocked";

    public static bool IsMusicModeUnlockedP1()
    {
        return PlayerPrefs.GetInt(MusicModeKeyP1, 0) == 1;
    }

    public static bool IsMusicModeUnlockedP2()
    {
        return PlayerPrefs.GetInt(MusicModeKeyP2, 0) == 1;
    }
    
    public static void UnlockMusicModeP1()
    {
        PlayerPrefs.SetInt(MusicModeKeyP1, 1);
        PlayerPrefs.Save();
    }

    public static void UnlockMusicModeP2()
    {
        PlayerPrefs.SetInt(MusicModeKeyP2, 1);
        PlayerPrefs.Save();
    }

    private static bool IsEffect0Unlocked()
    {
        return PlayerPrefs.GetInt(Effect0Key, 0) == 1;
    }

    private static bool IsEffect1Unlocked()
    {
        return PlayerPrefs.GetInt(Effect1Key, 0) == 1;
    }

    public static void UnlockEffect0()
    {
        PlayerPrefs.SetInt(Effect0Key, 1);
        PlayerPrefs.Save();
    }

    public static void UnlockEffect1()
    {
        PlayerPrefs.SetInt(Effect1Key, 1);
        PlayerPrefs.Save();
    }
    
    public static bool IsEffectUnlocked(string unlockKey)
    {
        switch (unlockKey)
        {
            case "Effect0":
                return IsEffect0Unlocked();
            case "Effect1":
                return IsEffect1Unlocked();
            default:
                return false;
        }
    }

    public static void ResetAllUnlocks()
    {
        PlayerPrefs.DeleteKey(MusicModeKeyP1);
        PlayerPrefs.DeleteKey(MusicModeKeyP2);
        PlayerPrefs.DeleteKey(Effect0Key);
        PlayerPrefs.DeleteKey(Effect1Key);
        PlayerPrefs.Save();
    }
}
