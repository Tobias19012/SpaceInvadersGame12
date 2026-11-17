using UnityEngine;

public enum Role { Gunner, Shielder }

// Styrer én spiller. Brug samme script på begge spillere og vælg rolle og taster i Inspector.
// Objektet skal have: SpriteRenderer, BoxCollider2D (Is Trigger), Rigidbody2D (Kinematic) og PaletteTint.
public class PlayerController : MonoBehaviour
{
    [Header("Rolle")]
    public Role role = Role.Gunner;
    public int playerNumber = 1;

    [Header("Taster (tekst, fx A, D, W, S, Space, LeftArrow, UpArrow)")]
    public string leftKey = "A";
    public string rightKey = "D";
    public string fireKey = "W";
    public string abilityKey = "S";

    [Header("Værdier")]
    public float moveSpeed = 7f;
    public float moveLimit = 7.8f;
    public float fireInterval = 0.22f;
    public float abilityCooldown = 5f;
    public float invulnerableTime = 2f;

    [Header("Skud")]
    public Bullet bulletPrefab;
    public float bulletSpeed = 12f;
    [Tooltip("Valgfrit tomt child-objekt foran på skibet. Hvis tomt, skydes der lidt over skibet.")]
    public Transform firePoint;

    [Header("Gunner: Burst")]
    public int burstCount = 5;
    public float burstSpreadDegrees = 10f;

    [Header("Shielder: Repair")]
    public int repairAmount = 4;

    float fireTimer, abilityTimer, invTimer;
    SpriteRenderer sr;

    public float AbilityTimeLeft { get { return Mathf.Max(0f, abilityTimer); } }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    Vector3 Origin()
    {
        return firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing)
        {
            sr.enabled = true;
            return;
        }

        float dt = Time.deltaTime;

        float move = 0f;
        if (Inp.Held(leftKey)) move -= 1f;
        if (Inp.Held(rightKey)) move += 1f;
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x + move * moveSpeed * dt, -moveLimit, moveLimit);
        transform.position = p;

        fireTimer -= dt;
        abilityTimer -= dt;
        invTimer -= dt;

        if (Inp.Held(fireKey) && fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireInterval * gm.FireMult;
        }

        if (Inp.Pressed(abilityKey) && abilityTimer <= 0f) UseAbility();

        // Blink mens spilleren er usårlig
        sr.enabled = invTimer <= 0f || ((int)(Time.time * 12f) % 2 == 0);
    }

    void Shoot()
    {
        Bullet b = Instantiate(bulletPrefab, Origin(), Quaternion.identity);
        b.Launch(Vector2.up * bulletSpeed);
        GameManager.Instance.Shots++;
        AudioManager.Play(Sound.Shoot, 0.6f);
    }

    void UseAbility()
    {
        GameManager gm = GameManager.Instance;

        if (role == Role.Gunner)
        {
            // Burst: flere skud i vifte
            for (int i = 0; i < burstCount; i++)
            {
                float angle = (i - (burstCount - 1) / 2f) * burstSpreadDegrees * Mathf.Deg2Rad;
                Bullet b = Instantiate(bulletPrefab, Origin(), Quaternion.identity);
                b.Launch(new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * (bulletSpeed - 1f));
            }
            gm.Shots += burstCount;
            abilityTimer = abilityCooldown * gm.AbilityMult;
            AudioManager.Play(Sound.Ability);
            Telemetry.Log("ability", gm.Wave, gm.Score, gm.Lives, "burst");
        }
        else
        {
            // Repair: reparér det skjold, der er mest ødelagt
            Bunker worst = null;
            foreach (Bunker b in Bunker.All)
                if (b.Missing > 0 && (worst == null || b.Hp < worst.Hp)) worst = b;

            if (worst == null) return; // intet at reparere, så evnen bruges ikke

            worst.Repair(repairAmount);
            abilityTimer = abilityCooldown * gm.AbilityMult;
            AudioManager.Play(Sound.Repair);
            Telemetry.Log("ability", gm.Wave, gm.Score, gm.Lives, "repair");
        }
    }

    // Kaldes af Bullet, når et alien-skud rammer spilleren
    public void Hit()
    {
        if (invTimer > 0f) return;
        invTimer = invulnerableTime;
        GameManager.Instance.LoseLife(role, "alien_bullet");
    }
}
