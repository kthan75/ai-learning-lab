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
        "Between rounds, spend credits to upgrade your agents.\n\n" +
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

        // How-to — dark panel + logo + concise instructions
        _howto = UIHelper.PanelStretch(canvasRoot, "IntroHowTo", new Color(0.04f, 0.05f, 0.10f, 1f));
        var t = _howto.transform;

        var logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
        logoGo.transform.SetParent(t, false);
        UIHelper.SetRect(logoGo, new Vector2(0, 320), new Vector2(620, 240));
        var logoImg = logoGo.GetComponent<Image>();
        logoImg.raycastTarget = false;
        var logo = ArtLoader.Load("logo.png");
        if (logo != null) { logoImg.sprite = logo; logoImg.preserveAspect = true; }
        else logoImg.color = new Color(0, 0, 0, 0); // invisible if missing

        UIHelper.Label(t, "HOW TO PLAY", 30, UIHelper.AccentBlue,
                       new Vector2(0, 150), new Vector2(800, 44), TextAnchor.MiddleCenter, FontStyle.Bold);

        var body = UIHelper.Label(t, HowToText, 24, UIHelper.ColText,
                                  new Vector2(0, -20), new Vector2(1300, 380), TextAnchor.MiddleCenter);
        body.lineSpacing = 1.35f;

        UIHelper.Label(t, "Press any key or click to begin", 22, UIHelper.ColSubtext,
                       new Vector2(0, -430), new Vector2(800, 32), TextAnchor.MiddleCenter);
        _howto.SetActive(false);
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
