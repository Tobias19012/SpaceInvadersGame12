using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Viser og skjuler paneler og opdaterer tekster. Læg scriptet på Canvas og træk felterne ind i Inspector.
// Alle felter er valgfrie: er et felt tomt, springes det bare over. Det gør, at UI'et kan bygges et stykke ad gangen.
public class UIManager : MonoBehaviour
{
    [Header("Paneler")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject upgradePanel;
    public GameObject gameOverPanel;

    [Header("HUD-tekster")]
    public TMP_Text scoreText;
    public TMP_Text waveText;
    public TMP_Text livesText;
    public TMP_Text p1AbilityText;
    public TMP_Text p2AbilityText;
    public TMP_Text bannerText;

    [Header("Andre tekster")]
    public TMP_Text upgradeTitle;
    public TMP_Text gameOverText;
    public TMP_Text telemetryPathText;

    [Header("Indstillinger (kan være flere, fx i menu og pause)")]
    public TMP_Text[] colourLabels;
    public Slider[] volumeSliders;

    [Header("Spillere (til evne-tekst)")]
    public PlayerController player1;
    public PlayerController player2;

    float bannerTimer;

    static void SetActive(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    static void SetText(TMP_Text t, string s)
    {
        if (t != null) t.text = s;
    }

    void Start()
    {
        if (volumeSliders != null)
            foreach (Slider s in volumeSliders)
                if (s != null) s.SetValueWithoutNotify(AudioListener.volume);

        RefreshColourLabels();
        SetText(bannerText, "");
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        SetText(scoreText, "SCORE " + gm.Score.ToString("D5"));
        SetText(waveText, "WAVE " + gm.Wave);
        SetText(livesText, "SHARED LIVES " + gm.Lives);
        SetText(p1AbilityText, AbilityLine(player1, "BURST"));
        SetText(p2AbilityText, AbilityLine(player2, "REPAIR"));

        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.unscaledDeltaTime;
            if (bannerTimer <= 0f) SetText(bannerText, "");
        }
    }

    string AbilityLine(PlayerController p, string abilityName)
    {
        if (p == null) return "";
        string key = p.abilityKey.Replace("Arrow", "").ToUpper();
        float left = p.AbilityTimeLeft;
        string status = left <= 0f ? "READY" : left.ToString("F1") + "s";
        return "P" + p.playerNumber + " " + p.role.ToString().ToUpper() + "  [" + key + "] " + abilityName + ": " + status;
    }

    public void OnStateChanged(GameState s)
    {
        SetActive(menuPanel, s == GameState.Menu);
        SetActive(hudPanel, s != GameState.Menu);
        SetActive(pausePanel, s == GameState.Paused);
        SetActive(upgradePanel, s == GameState.Upgrade);
        SetActive(gameOverPanel, s == GameState.GameOver);
    }

    public void ShowBanner(string text)
    {
        SetText(bannerText, text);
        bannerTimer = 2f;
    }

    public void ShowUpgrade(int wave)
    {
        SetText(upgradeTitle, "WAVE " + wave + " CLEARED");
    }

    public void ShowGameOver(int score, int wave, string cause, int kills, int shots, float time)
    {
        SetText(gameOverText, "GAME OVER\nScore " + score + "   Wave " + wave + "\nCause: " + cause +
                              "\nKills " + kills + "   Shots " + shots + "   Time " + time.ToString("F1") + "s");
        SetText(telemetryPathText, "Telemetry: " + Telemetry.FilePath);
    }

    public void RefreshColourLabels()
    {
        if (colourLabels == null) return;
        foreach (TMP_Text t in colourLabels)
            if (t != null) t.text = "Colours: " + Palette.Names[Palette.Index];
    }
}
