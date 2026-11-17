using System;
using System.Globalization;
using System.IO;
using UnityEngine;

// Logger spil-events til en CSV-fil, som kan åbnes i Excel/Google Sheets og bruges som data i rapporten.
// Filen ligger i Application.persistentDataPath (stien vises på Game Over-skærmen).
public static class Telemetry
{
    static string session = "none";

    public static string FilePath
    {
        get { return System.IO.Path.Combine(Application.persistentDataPath, "telemetry.csv"); }
    }

    public static void NewSession()
    {
        session = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
    }

    public static void Log(string evt, int wave, int score, int lives, string data)
    {
        try
        {
            bool exists = File.Exists(FilePath);
            using (var w = new StreamWriter(FilePath, true))
            {
                if (!exists) w.WriteLine("session,time_s,event,wave,score,lives,data");
                string t = Time.time.ToString("F1", CultureInfo.InvariantCulture);
                w.WriteLine(session + "," + t + "," + evt + "," + wave + "," + score + "," + lives + ",\"" + data + "\"");
            }
        }
        catch (Exception)
        {
            // Logning må aldrig crashe spillet.
        }
    }
}
