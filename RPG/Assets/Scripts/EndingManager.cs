using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text endingTitle;
    [SerializeField] private Text endingText;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private Button returnButton;

    [Header("Ending Data")]
    [SerializeField] private EndingData[] endings;

    [Header("Typing Settings")]
    [SerializeField] private float lettersPerSecond = 40f;
    private Coroutine typingCoroutine;

    void Start()
    {
        int endingId = PlayerPrefs.GetInt("EndingID", 0);
        Debug.Log($"EndingSceneManager: Showing ending {endingId}");
        returnButton.onClick.AddListener(OnReturnToMenu);

        if (GameController.instance != null)
        {
            GameController.instance.ChangeGameState(GameState.End);
        }

        LoadEnding(endingId);
    }

    private void LoadEnding(int id)
    {
        EndingData data = Array.Find(endings, e => e.id == id);
        if (data == null)
        {
            Debug.LogWarning($"No ending data found for ID {id}");
            endingText.text = "The story fades into mystery...";
            return;
        }

        // Apply audio and visuals
        backgroundImage.sprite = data.background;
        endingTitle.text = data.title;
        musicSource.clip = data.music;
        musicSource.Play();

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeEndingText(data.text));
    }

    private IEnumerator TypeEndingText(string text)
        {
            endingText.text = "";
            foreach (char letter in text.ToCharArray())
            {
                endingText.text += letter;
                yield return new WaitForSeconds(1f / lettersPerSecond);
            }
        }

    public void OnReturnToMenu()
    {
        Debug.Log("Returning to Main Menu");
        PlayerPrefs.DeleteKey("EndingID");

        if (ScreenLoader.instance != null)
        {
            ScreenLoader.instance.LoadScene("Scene0");
        }
        else
        {
            SceneManager.LoadScene("Scene0");
        }
    }
}

[Serializable]
public class EndingData
{
    public int id;
    public Sprite background;
    public string title;
    public string text;
    public AudioClip music;
}
