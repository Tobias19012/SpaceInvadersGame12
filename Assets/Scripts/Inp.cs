using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Hjælper til tastaturinput, der virker uanset om projektet bruger det gamle eller det nye Input System.
// Tastenavne er tekst: "A".."Z", "0".."9", "Space", "Enter", "Escape", "LeftArrow", "RightArrow", "UpArrow", "DownArrow".
// Du behøver ikke ændre noget her. Brug bare Inp.Held("A") og Inp.Pressed("Space").
public static class Inp
{
#if ENABLE_INPUT_SYSTEM
    static readonly Dictionary<string, Key> cache = new Dictionary<string, Key>();

    static bool TryKey(string name, out Key k)
    {
        if (cache.TryGetValue(name, out k)) return true;
        string n = (name.Length == 1 && char.IsDigit(name[0])) ? "Digit" + name : name;
        if (Enum.TryParse(n, out k))
        {
            cache[name] = k;
            return true;
        }
        return false;
    }

    public static bool Held(string name)
    {
        var kb = Keyboard.current;
        Key k;
        if (kb == null || !TryKey(name, out k)) return false;
        return kb[k].isPressed;
    }

    public static bool Pressed(string name)
    {
        var kb = Keyboard.current;
        Key k;
        if (kb == null || !TryKey(name, out k)) return false;
        return kb[k].wasPressedThisFrame;
    }
#else
    static readonly Dictionary<string, KeyCode> cache = new Dictionary<string, KeyCode>();

    static bool TryKey(string name, out KeyCode k)
    {
        if (cache.TryGetValue(name, out k)) return true;
        string n = name;
        if (name.Length == 1 && char.IsDigit(name[0])) n = "Alpha" + name;
        else if (name == "Enter") n = "Return";
        if (Enum.TryParse(n, out k))
        {
            cache[name] = k;
            return true;
        }
        return false;
    }

    public static bool Held(string name)
    {
        KeyCode k;
        return TryKey(name, out k) && Input.GetKey(k);
    }

    public static bool Pressed(string name)
    {
        KeyCode k;
        return TryKey(name, out k) && Input.GetKeyDown(k);
    }
#endif
}
