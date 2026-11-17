using UnityEngine;

// Læg dette script på alt, der skal følge farvepaletten (sprites og kameraet).
// Vælg "Role" i Inspector. Farven opdateres automatisk, når spilleren skifter palette.
public class PaletteTint : MonoBehaviour
{
    public PaletteRole role;

    SpriteRenderer sr;
    Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        Palette.Changed += Apply;
        Apply();
    }

    void OnDisable()
    {
        Palette.Changed -= Apply;
    }

    public void SetRole(PaletteRole newRole)
    {
        role = newRole;
        Apply();
    }

    void Apply()
    {
        Color c = Palette.Get(role);
        if (sr != null) sr.color = c;
        if (cam != null) cam.backgroundColor = c;
    }
}
