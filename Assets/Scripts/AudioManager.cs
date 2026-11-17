using UnityEngine;

public enum Sound { Shoot, Kill, Hit, Hurt, Ability, Repair, Upgrade, WaveClear, GameOver }

// Afspiller lyde. Træk lydklip fra Assets/Audio ind i felterne i Inspector.
// Andre scripts bruger den sådan her: AudioManager.Play(Sound.Shoot);
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Lydklip")]
    public AudioClip shoot;
    public AudioClip kill;
    public AudioClip hit;
    public AudioClip hurt;
    public AudioClip ability;
    public AudioClip repair;
    public AudioClip upgrade;
    public AudioClip waveClear;
    public AudioClip gameOver;
    [Tooltip("4 dybe toner, der afspilles i takt med at aliens bevæger sig")]
    public AudioClip[] march;

    AudioSource src;

    void Awake()
    {
        Instance = this;
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
    }

    public static void Play(Sound s, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.PlayClip(Instance.Get(s), volume);
    }

    public static void PlayMarch(int i)
    {
        if (Instance == null || Instance.march == null || Instance.march.Length == 0) return;
        Instance.PlayClip(Instance.march[i % Instance.march.Length], 0.5f);
    }

    void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null) src.PlayOneShot(clip, volume);
    }

    AudioClip Get(Sound s)
    {
        switch (s)
        {
            case Sound.Shoot: return shoot;
            case Sound.Kill: return kill;
            case Sound.Hit: return hit;
            case Sound.Hurt: return hurt;
            case Sound.Ability: return ability;
            case Sound.Repair: return repair;
            case Sound.Upgrade: return upgrade;
            case Sound.WaveClear: return waveClear;
            case Sound.GameOver: return gameOver;
        }
        return null;
    }
}
