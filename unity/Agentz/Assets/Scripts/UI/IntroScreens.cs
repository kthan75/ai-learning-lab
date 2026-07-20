using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Launch intro sequence, shown once per app launch before round 1:
///   1. Premise screen (the full-art `premise_bg` image).
///   2. How-to-play screen (text placeholder until the annotated panels exist).
/// Each is dismissed by any key or mouse button. When both are done, <see cref="OnComplete"/>
/// fires and the game unpauses.
/// </summary>
public class IntroScreens : MonoBehaviour
{
    public event Action OnComplete;

    private const string HowToText =
        "Missions appear on the board — each needs a mix of five skills.\n" +
        "Click a mission, pick up to 3 agents, and Assign.\n" +
        "The better your team matches, the higher your success chance.\n" +
        "Earn credits on success. Let 4 missions fail and the run ends.\n" +
        "Between rounds, spend credits to upgrade your agents.\n" +
        "Survive as long as you can.";

    private GameObject _premise, _howto;
    private int   _stage;      // 0 = premise, 1 = how-to, 2 = done
    private float _cooldown;   // brief input lockout after each screen appears

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        // Premise — full-screen art (dismiss to continue)
        _premise = UIHelper.PanelStretch(canvasRoot, "IntroPremise", Color.black);
        var pImg = _premise.GetComponent<Image>();
        var pSprite = ArtLoader.Load("premise_bg.png");
        if (pSprite != null) { pImg.sprite = pSprite; pImg.color = Color.white; }
        _premise.SetActive(false);

        // How-to — dark panel; logo on top, then a 2x2 grid of bordered boxes:
        // [ HOW TO PLAY ] [ SKILLS ]   over   [ main HUD shot ] [ assign shot ].
        _howto = UIHelper.PanelStretch(canvasRoot, "IntroHowTo", new Color(0.04f, 0.05f, 0.10f, 1f));
        var t = _howto.transform;

        // Logo (top, centered)
        var logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
        logoGo.transform.SetParent(t, false);
        UIHelper.SetRect(logoGo, new Vector2(0, 470), new Vector2(460, 150));
        var logoImg = logoGo.GetComponent<Image>();
        logoImg.raycastTarget = false;
        var logo = ArtLoader.Load("logo.png");
        if (logo != null) { logoImg.sprite = logo; logoImg.preserveAspect = true; }
        else logoImg.color = new Color(0, 0, 0, 0); // invisible if missing

        // Column centres (left column wider to match the 800px main HUD shot) and row centres.
        const float lx = -365f, rx = 430f;
        const float topY = 200f, botY = -235f;
        var boxBg = new Color(0.07f, 0.08f, 0.14f, 1f);

        // ── Top-left: HOW TO PLAY ──
        var box1 = UIHelper.Panel(t, "HowToBox", boxBg, new Vector2(lx, topY), new Vector2(800, 340));
        UIHelper.AddBorder(box1.transform, UIHelper.AccentOrange, 2f);
        UIHelper.Label(box1.transform, "HOW TO PLAY", 26, UIHelper.AccentBlue,
                       new Vector2(0, 132), new Vector2(760, 34), TextAnchor.MiddleCenter, FontStyle.Bold);
        var body = UIHelper.Label(box1.transform, HowToText, 22, UIHelper.ColText,
                                  new Vector2(30, -34), new Vector2(740, 250), TextAnchor.UpperLeft);
        body.lineSpacing = 1.5f;

        // ── Top-right: SKILLS ──
        var box2 = UIHelper.Panel(t, "SkillsBox", boxBg, new Vector2(rx, topY), new Vector2(670, 340));
        UIHelper.AddBorder(box2.transform, UIHelper.AccentOrange, 2f);
        UIHelper.Label(box2.transform, "SKILLS", 26, UIHelper.AccentBlue,
                       new Vector2(0, 132), new Vector2(600, 34), TextAnchor.MiddleCenter, FontStyle.Bold);
        for (int i = 0; i < 5; i++)
        {
            float y = 74f - i * 38f;
            var ic = ArtLoader.Load($"skill_{SkillSet.SkillAbbreviations[i].ToLowerInvariant()}.png");
            if (ic != null)
            {
                var icGo = new GameObject("SkillIcon", typeof(RectTransform), typeof(Image));
                icGo.transform.SetParent(box2.transform, false);
                UIHelper.SetRect(icGo, new Vector2(-255, y), new Vector2(30, 30));
                var iimg = icGo.GetComponent<Image>();
                iimg.sprite = ic; iimg.preserveAspect = true; iimg.raycastTarget = false;
            }
            // Abbreviation and full name in separate fixed columns so every name lines up.
            UIHelper.Label(box2.transform, SkillSet.SkillAbbreviations[i], 20, UIHelper.ColText,
                           new Vector2(-200, y), new Vector2(70, 30), TextAnchor.MiddleLeft, FontStyle.Bold);
            UIHelper.Label(box2.transform, SkillSet.SkillNames[i], 20, UIHelper.ColText,
                           new Vector2(-30, y), new Vector2(260, 30), TextAnchor.MiddleLeft);
        }

        // ── Bottom row: the two screenshots, aligned, each in an orange frame ──
        MakeImageBox(t, "MainShot",   "main_howto.png",   new Vector2(lx, botY), new Vector2(800, 450));
        MakeImageBox(t, "AssignShot", "assign_howto.png", new Vector2(rx, botY), new Vector2(670, 450));

        UIHelper.Label(t, "Press any key or click to begin", 22, UIHelper.ColSubtext,
                       new Vector2(0, -505), new Vector2(800, 32), TextAnchor.MiddleCenter);
        _howto.SetActive(false);
    }

    // A screenshot filling its box, framed by a thin orange border (falls back to a
    // subtle dark box if the art is missing).
    private static void MakeImageBox(Transform parent, string name, string file,
                                     Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        UIHelper.SetRect(go, pos, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        var sprite = ArtLoader.Load(file);
        if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
        else img.color = new Color(0.07f, 0.08f, 0.14f, 1f);
        UIHelper.AddBorder(go.transform, UIHelper.AccentOrange, 2f);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show()
    {
        _stage    = 0;
        _cooldown = 0.25f;
        _premise.SetActive(true);
        _premise.transform.SetAsLastSibling();
    }

    // ── Input / advance ────────────────────────────────────────────────────────
    private void Update()
    {
        if (_stage >= 2) return;
        if (_cooldown > 0f) { _cooldown -= Time.unscaledDeltaTime; return; }
        if (AnyInput()) Advance();
    }

    private void Advance()
    {
        if (_stage == 0)
        {
            _premise.SetActive(false);
            _howto.SetActive(true);
            _howto.transform.SetAsLastSibling();
            _stage    = 1;
            _cooldown = 0.25f;
        }
        else
        {
            _howto.SetActive(false);
            _stage = 2;
            OnComplete?.Invoke();
        }
    }

    private static bool AnyInput()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.anyKey.wasPressedThisFrame) return true;
        var m = Mouse.current;
        return m != null && (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame);
    }
}
