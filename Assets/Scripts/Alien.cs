using UnityEngine;

// En enkelt alien (også bossen). Prefaben skal have: SpriteRenderer, BoxCollider2D (Is Trigger),
// Rigidbody2D (Body Type: Kinematic) og PaletteTint.
[RequireComponent(typeof(PaletteTint))]
public class Alien : MonoBehaviour
{
    [Header("Værdier")]
    public int points = 10;
    public bool isBoss;

    [Header("Animation (to billeder)")]
    public Sprite frameA;
    public Sprite frameB;

    [Header("Valgfri")]
    [Tooltip("Valgfri prefab med et ParticleSystem, der spawnes når aliens dør")]
    public GameObject deathEffect;

    int hp;
    AlienFormation formation;
    SpriteRenderer sr;
    PaletteTint tint;
    BoxCollider2D box;

    public int Row { get; private set; }
    public float HalfWidth { get { return box.size.x * Mathf.Abs(transform.lossyScale.x) * 0.5f; } }
    public float HalfHeight { get { return box.size.y * Mathf.Abs(transform.lossyScale.y) * 0.5f; } }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        tint = GetComponent<PaletteTint>();
        box = GetComponent<BoxCollider2D>();
    }

    // Kaldes af AlienFormation lige efter alien er oprettet
    public void Setup(AlienFormation owner, int row, int startHp)
    {
        formation = owner;
        Row = row;
        hp = startHp;
        if (!isBoss) tint.SetRole((PaletteRole)((int)PaletteRole.Alien0 + (row % 4)));
    }

    public void SetFrame(int i)
    {
        if (frameA != null && frameB != null) sr.sprite = (i == 0) ? frameA : frameB;
    }

    public void TakeHit(int damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
        else AudioManager.Play(Sound.Hit, 0.6f);
    }

    void Die()
    {
        int pts = isBoss ? points : points + (3 - Mathf.Clamp(Row, 0, 3)) * 5;
        GameManager.Instance.AddKill(pts);
        AudioManager.Play(Sound.Kill);
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        formation.NotifyDead(this);
        Destroy(gameObject);
    }

    // Aliens, der rører et skjold, knuser det
    void OnTriggerEnter2D(Collider2D other)
    {
        Bunker b = other.GetComponent<Bunker>();
        if (b != null) b.Crush();
    }
}
