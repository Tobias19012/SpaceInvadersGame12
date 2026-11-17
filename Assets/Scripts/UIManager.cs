using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Viser og skjuler paneler og opdaterer tekster. Læg scriptet på Canvas og træk alle felter ind i Inspector.
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

    void Start()
    {
        foreach (Slider s in volumeSliders) s.SetValueWithoutNotify(AudioListener.volume);
        RefreshColourLabels();
        if (bannerText != null) bannerText.text = "";
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        scoreText.text = "SCORE " + gm.Score.ToString("D5");
        waveText.text = "WAVE " + gm.Wave;
        livesText.text = "SHARED LIVES " + gm.Lives;
        p1AbilityText.text = AbilityLine(player1, "BURST");
        p2AbilityText.text = AbilityLine(player2, "REPAIR");

        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.deltaTime;
            if (bannerTimer <= 0f) bannerText.text = "";
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
        menuPanel.SetActive(s == GameState.Menu);
        hudPanel.SetActive(s != GameState.Menu);
        pausePanel.SetActive(s == GameState.Paused);
        upgradePanel.SetActive(s == GameState.Upgrade);
        gameOverPanel.SetActive(s == GameState.GameOver);
    }

    public void ShowBanner(string text)
    {
        bannerText.text = text;
        bannerTimer = 2f;
    }

    public void ShowUpgrade(int wave)
    {
        upgradeTitle.text = "WAVE " + wave + " CLEARED";
    }

    public void ShowGameOver(int score, int wave, string cause, int kills, int shots, float time)
    {
        gameOverText.text = "GAME OVER\nScore " + score + "   Wave " + wave + "\nCause: " + cause +
                            "\nKills " + kills + "   Shots " + shots + "   Time " + time.ToString("F1") + "s";
        if (telemetryPathText != null) telemetryPathText.text = "Telemetry: " + Telemetry.FilePath;
    }

    public void RefreshColourLabels()
    {
        foreach (TMP_Text t in colourLabels) t.text = "Colours: " + Palette.Names[Palette.Index];
    }
}
