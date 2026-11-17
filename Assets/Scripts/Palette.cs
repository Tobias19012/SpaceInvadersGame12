using System;
using UnityEngine;

// Hvilken "rolle" en farve har. PaletteTint-komponenten bruger den til at slå farven op.
public enum PaletteRole
{
    Background, Player1, Player2, PlayerBullet, EnemyBullet,
    Alien0, Alien1, Alien2, Alien3, Boss, Bunker
}

// Farvepaletter (inkl. farveblind-venlig og high contrast). Valget gemmes i PlayerPrefs.
public static class Palette
{
    public static event Action Changed;

    public static readonly string[] Names = { "Neon", "Colorblind-safe", "High contrast" };

    static int index = -1;
    public static int Index
    {
        get
        {
            if (index < 0) index = Mathf.Clamp(PlayerPrefs.GetInt("si_palette", 0), 0, Names.Length - 1);
            return index;
        }
    }

    static Color H(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }

    // Rækkefølgen følger PaletteRole: Background, Player1, Player2, PlayerBullet, EnemyBullet,
    // Alien0, Alien1, Alien2, Alien3, Boss, Bunker
    static readonly Color[][] sets =
    {
        new Color[] { H("#05060F"), H("#39FF14"), H("#00E5FF"), H("#FFFFFF"), H("#FF3B6B"),
                      H("#FF4D4D"), H("#FFB000"), H("#B266FF"), H("#FF66C4"), H("#FF2D95"), H("#4DA3FF") },
        // Okabe-Ito: kan skelnes ved de fleste former for farveblindhed
        new Color[] { H("#0B0B12"), H("#56B4E9"), H("#E69F00"), H("#FFFFFF"), H("#D55E00"),
                      H("#CC79A7"), H("#F0E442"), H("#D55E00"), H("#FFFFFF"), H("#CC79A7"), H("#009E73") },
        new Color[] { H("#000000"), H("#FFFFFF"), H("#FFFF00"), H("#FFFFFF"), H("#FF00FF"),
                      H("#FF00FF"), H("#00FF00"), H("#FF8800"), H("#FF00FF"), H("#FF00FF"), H("#00FFFF") }
    };

    public static Color Get(PaletteRole role)
    {
        return sets[Index][(int)role];
    }

    public static void Next()
    {
        index = (Index + 1) % Names.Length;
        PlayerPrefs.SetInt("si_palette", index);
        PlayerPrefs.Save();
        if (Changed != null) Changed();
    }
}
