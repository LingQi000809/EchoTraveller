using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private GameObject helpPanel;
    [Tooltip("Optional: assign a Close button inside Help panel (if you have one).")]
    [SerializeField] private Button helpCloseButton;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusic;

    private Button[] menuButtons;

    private void Awake()
    {
        ClearSelectionNow();
    }

    private void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            StartCoroutine(ClearSelectionNextFrame());
            OnStartGame();
        });

        helpButton.onClick.AddListener(() =>
        {
            StartCoroutine(ClearSelectionNextFrame());
            OnShowHelp();
        });

        if (helpCloseButton != null)
        {
            helpCloseButton.onClick.AddListener(() =>
            {
                StartCoroutine(ClearSelectionNextFrame());
                CloseHelp();
            });
        }

        helpPanel.SetActive(false);

        menuButtons = new Button[] { startButton, helpButton };

        foreach (var b in menuButtons)
        {
            var cb = b.colors;
            cb.selectedColor = cb.highlightedColor;
            b.colors = cb;
        }

        if (menuMusic != null) menuMusic.Play();
    }

    private void Update()
    {
        if (helpPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseHelp();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.fullScreen = !Screen.fullScreen;
            Debug.Log($"Fullscreen toggled: {Screen.fullScreen}");
        }
    }

    private void OnStartGame()
    {
        Debug.Log("Starting game - Loading Scene1");

        if (ScreenLoader.instance != null)
        {
            ScreenLoader.instance.LoadScene("Scene1");
        }
        else
        {
            Debug.LogWarning("ScreenLoader not found in scene, loading directly...");
            SceneManager.LoadScene("Scene1");
        }
    }

    private void OnShowHelp()
    {
        helpPanel.SetActive(true);
        ClearSelectionNow();
    }

    private void CloseHelp()
    {
        helpPanel.SetActive(false);
        ClearSelectionNow();
    }

    private void ClearSelectionNow()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private IEnumerator ClearSelectionNextFrame()
    {
        yield return null;
        ClearSelectionNow();
    }
}
