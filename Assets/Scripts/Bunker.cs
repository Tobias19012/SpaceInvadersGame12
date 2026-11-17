using System.Collections.Generic;
using UnityEngine;

// Skjold. Tager skade af alien-skud, og knuses hvis aliens rører det.
// Objektet skal have: SpriteRenderer (bunker.png) og BoxCollider2D (Is Trigger).
public class Bunker : MonoBehaviour
{
    public static readonly List<Bunker> All = new List<Bunker>();

    int hp;
    SpriteRenderer sr;
    Collider2D col;

    public int Hp { get { return hp; } }
    public int Missing { get { return GameManager.Instance.BunkerMaxHp - hp; } }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start()
    {
        hp = GameManager.Instance.BunkerMaxHp;
    }

    void Update()
    {
        // Vis/skjul og farv skjoldet efter hvor meget liv det har tilbage
        bool alive = hp > 0;
        if (sr.enabled != alive) sr.enabled = alive;
        if (col.enabled != alive) col.enabled = alive;

        float k = 0.35f + 0.65f * Mathf.Clamp01(hp / (float)Mathf.Max(1, GameManager.Instance.BunkerMaxHp));
        Color c = Palette.Get(PaletteRole.Bunker);
        sr.color = new Color(c.r * k, c.g * k, c.b * k, 1f);
    }

    public void ResetFull()
    {
        hp = GameManager.Instance.BunkerMaxHp;
    }

    public void TakeHit()
    {
        hp = Mathf.Max(0, hp - 1);
        AudioManager.Play(Sound.Hit, 0.5f);
    }

    public void Crush()
    {
        hp = 0;
    }

    public void Repair(int amount)
    {
        hp = Mathf.Min(GameManager.Instance.BunkerMaxHp, hp + amount);
    }
}
