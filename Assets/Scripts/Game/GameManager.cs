using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    public int TargetCoins = 5;

    [Header("UI")]
    public Text CoinCountText;
    public GameObject EndScreenPanel;
    public Text EndScreenText;
    public Button RestartButton;

    [Header("Audio")]
    public AudioClip BackgroundMusic;
    public float BackgroundMusicVolume = 0.5f;

    private int _coinsCollected;
    private bool _gameOver;
    private CursorLockMode _initialCursorLock;
    private bool _initialCursorVisible;
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _initialCursorLock = Cursor.lockState;
        _initialCursorVisible = Cursor.visible;

        EnsureUi();
        EnsureBackgroundMusic();
        _coinsCollected = 0;
        SetEndScreen(false);
        UpdateCoinText();

        if (RestartButton != null)
        {
            RestartButton.onClick.RemoveListener(RestartScene);
            RestartButton.onClick.AddListener(RestartScene);
        }
    }

    private void EnsureUi()
    {
        if (CoinCountText != null && EndScreenPanel != null && EndScreenText != null && RestartButton != null) return;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("GameCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (CoinCountText == null)
        {
            var coinTextGo = new GameObject("CoinCountText");
            coinTextGo.transform.SetParent(canvas.transform, false);
            var rect = coinTextGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(300f, 50f);

            CoinCountText = coinTextGo.AddComponent<Text>();
            CoinCountText.font = font;
            CoinCountText.fontSize = 24;
            CoinCountText.color = Color.white;
            CoinCountText.alignment = TextAnchor.UpperLeft;
        }

        if (EndScreenPanel == null)
        {
            var panelGo = new GameObject("EndScreenPanel");
            panelGo.transform.SetParent(canvas.transform, false);
            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = panelGo.AddComponent<Image>();
            img.color = Color.black;
            EndScreenPanel = panelGo;
        }

        if (EndScreenText == null)
        {
            var textGo = new GameObject("EndScreenText");
            textGo.transform.SetParent(EndScreenPanel.transform, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(600f, 100f);

            EndScreenText = textGo.AddComponent<Text>();
            EndScreenText.font = font;
            EndScreenText.fontSize = 48;
            EndScreenText.color = Color.white;
            EndScreenText.alignment = TextAnchor.MiddleCenter;
        }

        if (RestartButton == null)
        {
            var buttonGo = new GameObject("RestartButton");
            buttonGo.transform.SetParent(EndScreenPanel.transform, false);
            var rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(160f, 50f);

            var img = buttonGo.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            RestartButton = buttonGo.AddComponent<Button>();

            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.AddComponent<Text>();
            label.font = font;
            label.fontSize = 20;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = "Restart";
        }
    }

    private void EnsureBackgroundMusic()
    {
        if (BackgroundMusic == null) return;

        if (_musicSource == null)
        {
            _musicSource = gameObject.GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
        }

        _musicSource.clip = BackgroundMusic;
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = Mathf.Clamp01(BackgroundMusicVolume);

        if (!_musicSource.isPlaying)
        {
            _musicSource.Play();
        }
    }

    public void CollectCoin()
    {
        if (_gameOver) return;

        _coinsCollected++;
        UpdateCoinText();

        if (_coinsCollected >= TargetCoins)
        {
            Win();
        }
    }

    public void PlayerDied()
    {
        if (_gameOver) return;
        Lose();
    }

    private void Win()
    {
        EndGame("You Won");
    }

    private void Lose()
    {
        EndGame("You Lost");
    }

    private void EndGame(string message)
    {
        _gameOver = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EndScreenText != null)
        {
            EndScreenText.text = message;
        }

        SetEndScreen(true);
    }

    private void SetEndScreen(bool active)
    {
        if (EndScreenPanel != null)
        {
            EndScreenPanel.SetActive(active);
        }

        if (RestartButton != null)
        {
            RestartButton.gameObject.SetActive(active);
        }
    }

    private void UpdateCoinText()
    {
        if (CoinCountText != null)
        {
            CoinCountText.text = "Coins: " + _coinsCollected + " / " + TargetCoins;
        }
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        Cursor.lockState = _initialCursorLock;
        Cursor.visible = _initialCursorVisible;

        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex < 0)
        {
            Debug.LogWarning("Active scene is not in Build Settings; cannot restart by build index.");
            return;
        }

        SceneManager.LoadScene(scene.buildIndex);
    }
}

