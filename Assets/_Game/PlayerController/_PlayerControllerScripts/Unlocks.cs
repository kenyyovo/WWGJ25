using UnityEngine;

public static class Unlocks
{
    private const string MusicModeKeyP1 = "MusicModeUnlocked";
    private const string MusicModeKeyP2 = "MusicModeUnlocked2";

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

    public static void ResetMusicModeUnlocks()
    {
        PlayerPrefs.DeleteKey(MusicModeKeyP1);
        PlayerPrefs.DeleteKey(MusicModeKeyP2);
        PlayerPrefs.Save();
    }
}
