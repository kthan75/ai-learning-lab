using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    private float deltaTime = 0.0f;
    private bool showDebug = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Restart scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Toggle debug overlay
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebug = !showDebug;
        }

        // Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        deltaTime = Mathf.Lerp(deltaTime, Time.unscaledDeltaTime, 0.1f);
    }

    void OnGUI()
    {
        if (!showDebug) return;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 0.02f);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        float fps = deltaTime > 0 ? 1.0f / deltaTime : 0.0f;
        string text = $"FPS: {fps:0.} | Scene: {SceneManager.GetActiveScene().name}";

        GUI.Label(rect, text, style);
    }
}