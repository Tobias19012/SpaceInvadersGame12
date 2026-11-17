using System.Collections.Generic;
using UnityEngine;

// Styrer hele alien-flokken: spawner en bølge, flytter den sidelæns og ned, øger tempoet,
// lader aliens skyde og håndterer bossen. Læg scriptet på et tomt objekt i position (0,0,0).
public class AlienFormation : MonoBehaviour
{
    [Header("Prefabs")]
    public Alien alienPrefab;
    public Alien bossPrefab;
    public Bullet enemyBulletPrefab;

    [Header("Gitter")]
    public int columns = 8;
    public int rows = 4;
    public float spacingX = 1.3f;
    public float spacingY = 0.9f;
    public float topY = 3.6f;

    [Header("Bevægelse")]
    public float baseSpeed = 0.9f;
    public float speedPerWave = 0.18f;
    public float dropDistance = 0.35f;
    public float leftLimit = -7.8f;
    public float rightLimit = 7.8f;
    [Tooltip("Hvis en alien kommer ned til denne y-værdi, er det game over")]
    public float invadeY = -3.7f;

    [Header("Skydning")]
    public float baseFireInterval = 1.1f;
    public float minFireInterval = 0.3f;
    public float baseBulletSpeed = 4.5f;
    public float maxBulletSpeed = 7f;

    [Header("Boss (hver 5. bølge)")]
    public int bossBaseHp = 40;
    public int bossHpPerBoss = 15;

    readonly List<Alien> aliens = new List<Alien>();
    Alien boss;
    int wave, waveTotal, frame, marchNote;
    float dir = 1f, animTimer, fireTimer, bossTimer;

    public void SpawnWave(int w)
    {
        ClearAliens();
        wave = w;
        dir = 1f;
        transform.position = Vector3.zero;
        fireTimer = 1.5f;
        bossTimer = 2f;
        animTimer = 0f;
        frame = 0;

        float startX = -(columns - 1) * spacingX / 2f;

        if (w % 5 == 0)
        {
            // Boss-bølge: to rækker aliens og en stor boss øverst
            for (int r = 0; r < 2; r++)
                for (int c = 0; c < columns; c++)
                    Spawn(alienPrefab, r + 2, startX + c * spacingX, 1.5f - r * spacingY, 1);

            boss = Spawn(bossPrefab, 0, 0f, 3.5f, bossBaseHp + bossHpPerBoss * (w / 5 - 1));
        }
        else
        {
            float y0 = topY - Mathf.Min(w - 1, 3) * 0.25f;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    Spawn(alienPrefab, r, startX + c * spacingX, y0 - r * spacingY, 1);
        }
        waveTotal = aliens.Count;
    }

    Alien Spawn(Alien prefab, int row, float x, float y, int hp)
    {
        Alien a = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
        a.Setup(this, row, hp);
        aliens.Add(a);
        return a;
    }

    void ClearAliens()
    {
        foreach (Alien a in aliens)
            if (a != null) Destroy(a.gameObject);
        aliens.Clear();
        boss = null;
    }

    public void NotifyDead(Alien a)
    {
        aliens.Remove(a);
        if (a == boss) boss = null;
        if (aliens.Count == 0) GameManager.Instance.WaveCleared();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing || aliens.Count == 0) return;

        float dt = Time.deltaTime;

        // Tempoet stiger, jo færre aliens der er tilbage
        float frac = 1f - (float)aliens.Count / Mathf.Max(1, waveTotal);
        float speed = (baseSpeed + speedPerWave * (wave - 1)) * (1f + frac * 2.5f);
        transform.position += Vector3.right * dir * speed * dt;

        // Find yderkanter og laveste punkt
        float minX = float.MaxValue, maxX = float.MinValue, lowest = float.MaxValue;
        foreach (Alien a in aliens)
        {
            Vector3 p = a.transform.position;
            minX = Mathf.Min(minX, p.x - a.HalfWidth);
            maxX = Mathf.Max(maxX, p.x + a.HalfWidth);
            lowest = Mathf.Min(lowest, p.y - a.HalfHeight);
        }

        if ((dir > 0f && maxX > rightLimit) || (dir < 0f && minX < leftLimit))
        {
            dir = -dir;
            transform.position += Vector3.down * dropDistance;
        }

        if (lowest <= invadeY)
        {
            gm.GameOver("invasion");
            return;
        }

        // Animation og march-lyd
        animTimer += dt;
        float interval = Mathf.Clamp(0.65f / (1f + frac * 3f + (wave - 1) * 0.15f), 0.07f, 0.65f);
        if (animTimer >= interval)
        {
            animTimer = 0f;
            frame = 1 - frame;
            marchNote = (marchNote + 1) % 4;
            foreach (Alien a in aliens) a.SetFrame(frame);
            AudioManager.PlayMarch(marchNote);
        }

        // Tilfældig alien skyder
        fireTimer -= dt;
        if (fireTimer <= 0f)
        {
            fireTimer = Mathf.Max(minFireInterval, baseFireInterval - 0.06f * wave) * Random.Range(0.7f, 1.3f);
            Alien shooter = aliens[Random.Range(0, aliens.Count)];
            if (!shooter.isBoss) Fire(shooter.transform.position + Vector3.down * 0.4f, 0f);
        }

        // Bossen skyder 3 skud i vifte
        if (boss != null)
        {
            bossTimer -= dt;
            if (bossTimer <= 0f)
            {
                bossTimer = 1f;
                Vector3 pos = boss.transform.position + Vector3.down * 1.2f;
                for (int k = -1; k <= 1; k++) Fire(pos, k * 15f);
            }
        }
    }

    void Fire(Vector3 pos, float angleDeg)
    {
        float spd = Mathf.Min(maxBulletSpeed, baseBulletSpeed + 0.2f * wave);
        float a = angleDeg * Mathf.Deg2Rad;
        Bullet b = Instantiate(enemyBulletPrefab, pos, Quaternion.identity);
        b.Launch(new Vector2(Mathf.Sin(a), -Mathf.Cos(a)) * spd);
    }
}
