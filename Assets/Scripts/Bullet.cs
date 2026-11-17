using System.Collections.Generic;
using UnityEngine;

// Skud. Bruges på både spillerens skud og alien-skud (to prefabs, samme script).
// Prefaben skal have: SpriteRenderer, BoxCollider2D (Is Trigger), Rigidbody2D (Body Type: Kinematic).
public class Bullet : MonoBehaviour
{
    public static readonly List<Bullet> All = new List<Bullet>();

    [Tooltip("Sæt flueben på spillerens skud. Fjern det på alien-skud.")]
    public bool fromPlayer = true;
    public int damage = 1;

    Vector2 velocity;
    bool used;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    // Kaldes af den, der skyder
    public void Launch(Vector2 vel)
    {
        velocity = vel;
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        Vector3 p = transform.position;
        if (p.y > 7f || p.y < -7f || Mathf.Abs(p.x) > 12f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        if (fromPlayer)
        {
            Alien a = other.GetComponent<Alien>();
            if (a != null)
            {
                used = true;
                a.TakeHit(damage);
                Destroy(gameObject);
            }
        }
        else
        {
            Bunker b = other.GetComponent<Bunker>();
            if (b != null)
            {
                used = true;
                b.TakeHit();
                Destroy(gameObject);
                return;
            }

            PlayerController p = other.GetComponent<PlayerController>();
            if (p != null)
            {
                used = true;
                p.Hit();
                Destroy(gameObject);
            }
        }
    }

    public static void ClearAll()
    {
        for (int i = All.Count - 1; i >= 0; i--)
            if (All[i] != null) Destroy(All[i].gameObject);
    }
}
