using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, Upgrade, GameOver }

// Hjernen i spillet: tilstand, point, liv, bølger, upgrades og telemetri.
// Læg scriptet på ét tomt objekt i scenen ("GameManager").
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    static bool autoStart; // sættes af Restart, så menuen springes over

    [Header("Referencer (træk ind)")]
    public AlienFormation formation;
    public UIManager ui;

    [Header("Startværdier")]
    public int startLives = 5;
    public int startBunkerHp = 8;

    public GameState State { get; private set; }
    public int Score { get; private set; }
    public int Wave { get; private set; }
    public int Lives { get; private set; }
    public int BunkerMaxHp { get; private set; }
    public float FireMult { get; private set; }
    public float AbilityMult { get; private set; }
    public float PlayTime { get; private set; }
    [HideInInspector] public int Kills;
    [HideInInspector] public int Shots;

    float waveStartTime;

    void Awake()
    {
        Instance = this;
        Lives = startLives;
        BunkerMaxHp = startBunkerHp;
        FireMult = 1f;
        AbilityMult = 1f;
        AudioListener.volume = PlayerPrefs.GetFloat("si_volume", 0.7f);
    }

    void Start()
    {
        if (autoStart)
        {
            autoStart = false;
            StartGame();
        }
        else
        {
            SetState(GameState.Menu);
        }
    }

    void Update()
    {
        switch (State)
        {
            case GameState.Menu:
                if (Inp.Pressed("Enter") || Inp.Pressed("Space")) StartGame();
                break;
            case GameState.Playing:
                PlayTime += Time.deltaTime;
                if (Inp.Pressed("Escape")) SetState(GameState.Paused);
                break;
            case GameState.Paused:
                if (Inp.Pressed("Escape")) SetState(GameState.Playing);
                break;
            case GameState.Upgrade:
                for (int i = 0; i < 4; i++)
                {
                    if (Inp.Pressed((i + 1).ToString())) { ChooseUpgrade(i); break; }
                }
                break;
            case GameState.GameOver:
                if (Inp.Pressed("Enter")) Restart();
                break;
        }
    }

    void SetState(GameState s)
    {
        State = s;
        Time.timeScale = (s == GameState.Playing) ? 1f : 0f; // pause alt undtagen når vi spiller
        if (ui != null) ui.OnStateChanged(s);
    }

    // ---------------------------------------------------------------- spilforløb

    // Kobles til Start-knappen
    public void StartGame()
    {
        Score = 0;
        Wave = 1;
        Lives = startLives;
        BunkerMaxHp = startBunkerHp;
        FireMult = 1f;
        AbilityMult = 1f;
        Kills = 0;
        Shots = 0;
        PlayTime = 0f;

        foreach (Bunker b in Bunker.All) b.ResetFull();

        Telemetry.NewSession();
        Telemetry.Log("game_start", 0, 0, Lives, "");
        BeginWave();
    }

    void BeginWave()
    {
        Wave++;
        Bullet.ClearAll();
        formation.SpawnWave(Wave);
        waveStartTime = PlayTime;

        bool boss = (Wave % 5 == 0);
        Telemetry.Log("wave_start", Wave, Score, Lives, boss ? "boss" : "normal");
        if (ui != null) ui.ShowBanner(boss ? "BOSS WAVE " + Wave : "WAVE " + Wave);
        SetState(GameState.Playing);
    }

    // Kaldes af AlienFormation, når sidste alien er dræbt
    public void WaveCleared()
    {
        if (State != GameState.Playing) return;
        Score += 100 * Wave;
        Telemetry.Log("wave_clear", Wave, Score, Lives,
            "wave_time_s=" + (PlayTime - waveStartTime).ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
        AudioManager.Play(Sound.WaveClear);
        Bullet.ClearAll();
        if (ui != null) ui.ShowUpgrade(Wave);
        SetState(GameState.Upgrade);
    }

    // Kobles til upgrade-knapperne (0 = fire rate, 1 = extra life, 2 = shields, 3 = ability cooldown)
    public void ChooseUpgrade(int i)
    {
        if (State != GameState.Upgrade) return;

        string name = "";
        switch (i)
        {
            case 0: FireMult *= 0.85f; name = "fire_rate"; break;
            case 1: Lives++; name = "extra_life"; break;
            case 2:
                BunkerMaxHp += 2;
                foreach (Bunker b in Bunker.All) b.ResetFull();
                name = "shields";
                break;
            case 3: AbilityMult *= 0.8f; name = "ability_cooldown"; break;
        }
        Telemetry.Log("upgrade", Wave, Score, Lives, name);
        AudioManager.Play(Sound.Upgrade);
        BeginWave();
    }

    public void AddKill(int points)
    {
        Score += points;
        Kills++;
    }

    public void LoseLife(Role role, string cause)
    {
        if (State != GameState.Playing) return;
        Lives--;
        Telemetry.Log("life_lost", Wave, Score, Lives, "role=" + role.ToString().ToLower() + ";cause=" + cause);
        AudioManager.Play(Sound.Hurt);
        if (Lives <= 0) GameOver(cause);
    }

    public void GameOver(string cause)
    {
        if (State == GameState.GameOver) return;
        Telemetry.Log("game_over", Wave, Score, Lives,
            "cause=" + cause + ";kills=" + Kills + ";shots=" + Shots + ";play_time_s=" +
            PlayTime.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
        AudioManager.Play(Sound.GameOver);
        if (ui != null) ui.ShowGameOver(Score, Wave, cause, Kills, Shots, PlayTime);
        SetState(GameState.GameOver);
    }

    // ---------------------------------------------------------------- knapper i UI

    public void ResumeGame() { SetState(GameState.Playing); }

    public void Restart()
    {
        autoStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToMenu()
    {
        autoStart = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame() { Application.Quit(); }

    public void CycleColours()
    {
        Palette.Next();
        if (ui != null) ui.RefreshColourLabels();
    }

    public void SetVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("si_volume", v);
    }
}
