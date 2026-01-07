using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpeakerProfile
{
    public string speakerName;
    public Sprite sprite;
}

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance { get; private set; }

    [SerializeField] GameObject dialogContentParent;
    [SerializeField] Text dialogText;
    [SerializeField] Button[] choiceButtons;

    // profiles
    [SerializeField] Image profileImg;
    [SerializeField] Sprite unknownSpeaker;
    [SerializeField] private List<SpeakerProfile> speakerProfiles;
    private Dictionary<string, Sprite> speakerDict;

    private int highlightedChoiceIdx = -1;
    private int selectedChoiceIdx = -1;
    [SerializeField] int lettersPerSecond;
    private bool isTyping = false;
    private string fullLine = "";
    private Coroutine typingCoroutine = null;

    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;

    private Story story;

    private bool dialoguePlaying = false;
    private bool waitingForChoicePause = false;


    private void Awake()
    {
        instance = this;
        story = new Story(inkJson.text);

        speakerDict = new Dictionary<string, Sprite>();
        foreach (var profile in speakerProfiles)
        {
            if (!speakerDict.ContainsKey(profile.speakerName))
                speakerDict.Add(profile.speakerName, profile.sprite);
        }
    }

    private void ResetPanel()
    {
        dialogText.text = "";
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }
        highlightedChoiceIdx = -1;
        selectedChoiceIdx = -1;
    }

    public void EnterDialogue(string knotName)
    {
        if (dialoguePlaying) return;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayDialogueTriggerSound();
            AudioManager.instance.FadeMusicForDialogue();
        }

        Debug.Log("Entering Dialogue");
        dialoguePlaying = true;
        ResetPanel();
        dialogContentParent.SetActive(true);
        GameController.instance.ChangeGameState(GameState.Dialog);
        GameController.instance.playerController.DisablePlayerMovement();

        // jump to the knot
        if (!string.IsNullOrEmpty(knotName))
        {
            story.ChoosePathString(knotName);
        }
        else
        {
            Debug.LogWarning("Knot name was the empty string when entering dialogue.");
        }

        // kick off the story
        ContinueOrExitStory();
    }

    public void HandleUpdate()
    {
        // Skip typing if Return is pressed 
        if (isTyping && Input.GetKeyDown(KeyCode.Return))
        {
            // Immediately finish the current line
            isTyping = false;
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            dialogText.text = fullLine; 
            return; 
        }
        if (dialoguePlaying)
        {
            if (story.currentChoices.Count > 0 && !waitingForChoicePause)
            {
                HandleChoiceNavigation();
            }
            // Ink consumes the choice that players select. 
            // We have to keep the story moving by forcing players to hit Return. 
            // Otherwise, if the player restarts the dialog, some choices will disappear.
            else if (!isTyping && Input.GetKeyDown(KeyCode.Return))
            {
                ContinueOrExitStory();
            }
        }
    }


    private void HandleChoiceNavigation()
    {
        int choiceCount = story.currentChoices.Count;

        // Check for voice input first
        if (VoiceController.instance != null && VoiceController.instance.HasDetectedTrend())
        {
            VoiceController.PitchTrend trend = VoiceController.instance.GetLastDetectedTrend();

            if (trend == VoiceController.PitchTrend.Ascending)
            {
                highlightedChoiceIdx = 0;
                Debug.Log("Voice: ASCENDING - Highlighted TOP choice");
                UpdateChoiceHighlight();
            }
            else if (trend == VoiceController.PitchTrend.Descending)
            {
                highlightedChoiceIdx = choiceCount > 1 ? 1 : 0;
                Debug.Log("Voice: DESCENDING - Highlighted BOTTOM choice");
                UpdateChoiceHighlight();
            }
            else if (trend == VoiceController.PitchTrend.Stable)
            {
                highlightedChoiceIdx = -1;
                Debug.Log("Voice: STABLE - Cleared selection");
                UpdateChoiceHighlight();
            }
        }
        // Fallback to keyboard input
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            highlightedChoiceIdx = 0;
            UpdateChoiceHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            highlightedChoiceIdx = choiceCount > 1 ? 1 : 0;
            UpdateChoiceHighlight();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            highlightedChoiceIdx = -1;
            UpdateChoiceHighlight();
        }

        // Handle ENTER key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // If a choice is highlighted, confirm it
            if (highlightedChoiceIdx >= 0 && highlightedChoiceIdx < choiceCount)
            {
                // Stop voice listening
                if (VoiceController.instance != null)
                {
                    VoiceController.instance.StopListening();
                }

                selectedChoiceIdx = highlightedChoiceIdx;
                story.ChooseChoiceIndex(selectedChoiceIdx);
                ContinueOrExitStory();
            }
            // // If no choice is highlighted, do nothing and keep waiting
            // else
            // {
            //     ContinueOrExitStory();
            // }
        }
    }


    private void UpdateChoiceHighlight()
    {
        // Reset all to normal
        foreach (Button button in choiceButtons)
        {
            button.image.color = button.colors.normalColor;
        }
        if (highlightedChoiceIdx >= 0)
        {
            Button highlightedButton = choiceButtons[highlightedChoiceIdx];
            highlightedButton.image.color = highlightedButton.colors.highlightedColor;
        }
    }

    // this function is called when player enters the dialog or presses return key to proceed
    private void ContinueOrExitStory()
    {
        // If we were waiting for a return press (which just happend) before showing choices
        if (waitingForChoicePause)
        {
            Debug.Log("Showing choices");
            waitingForChoicePause = false;
            SetChoices(story.currentChoices);
            // Start voice listening only when actual choices appear
            if (story.currentChoices.Count > 0 && VoiceController.instance != null)
                VoiceController.instance.StartListening();
            return;
        }

        if (story.canContinue)
        {
            string dialogueLine = story.Continue();
            string speaker = dialogueLine.Trim().Split(": ")[0];
            if (speakerDict.ContainsKey(speaker))
                profileImg.sprite = speakerDict[speaker];
            else
                profileImg.sprite = unknownSpeaker; // fallback

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialog(dialogueLine));

            if (story.currentChoices.Count > 0)
            {
                Debug.Log("There are choices; waiting for return before showing them");
                waitingForChoicePause = true;
            } else // hide choices 
            {
                SetChoices(story.currentChoices);
            }
        }
        else
        {
            ExitDialogue();
        }
    }

    private IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        fullLine = line;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            if (!isTyping)
            {
                // typing was skipped
                dialogText.text = fullLine;
                yield break;
            }

            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        // finished typing
        isTyping = false;
    } 

    private void SetChoices(List<Choice> dialogChoices)
    {
        // Hide all buttons first
        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }
        // If there are no choices, return
        if (dialogChoices.Count == 0)
            return;

        // Display available choices
        for (int i = 0; i < dialogChoices.Count && i < choiceButtons.Length; i++)
        {
            Choice dialogChoice = dialogChoices[i];
            Button choiceButton = choiceButtons[i];

            choiceButton.gameObject.SetActive(true);
            choiceButton.GetComponentInChildren<Text>().text = dialogChoice.text;
        }
        highlightedChoiceIdx = -1;
        UpdateChoiceHighlight();
    }

    private void ExitDialogue()
    {
        Debug.Log("Exiting Dialogue");

        // Stop voice listening
        if (VoiceController.instance != null)
        {
            VoiceController.instance.StopListening();
        }

        // Restore music volume
        if (AudioManager.instance != null)
        {
            AudioManager.instance.RestoreMusicAfterDialogue();
        }

        dialogContentParent.SetActive(false);
        ResetPanel();
        dialoguePlaying = false;

        // ---- READ INK VARIABLES ----
        string sceneVar = story.variablesState["scene"]?.ToString() ?? "";
        string battleWith = story.variablesState["battle_with"]?.ToString() ?? "";
        string endingVar = story.variablesState["ending"]?.ToString() ?? "0";
        int ending = int.TryParse(endingVar, out int e) ? e : 0;
        string battleWinEndingStr = story.variablesState["battle_win_ending"]?.ToString() ?? "0";
        string battleLoseEndingStr = story.variablesState["battle_lose_ending"]?.ToString() ?? "0";
        int battleWinEnding = int.TryParse(battleWinEndingStr, out int bw) ? bw : 0;
        int battleLoseEnding = int.TryParse(battleLoseEndingStr, out int bl) ? bl : 0;
        string resumeWin = story.variablesState["battle_win_resume"]?.ToString() ?? "";
        string resumeLose = story.variablesState["battle_lose_resume"]?.ToString() ?? "";

        // reset story variables
        story.ResetState();
        // ---- DETERMINE WHAT TO DO NEXT ----
        // 1️⃣ Battle transition
        if (!string.IsNullOrEmpty(battleWith))
        {
            Debug.Log($"Starting battle with: {battleWith}");
            StartBattle(battleWith, battleWinEnding, battleLoseEnding, resumeWin, resumeLose);
            return;
        }

        // 2️⃣ Ending scene
        if (ending != 0)
        {
            Debug.Log($"Loading ending scene: {ending}");
            PlayerPrefs.SetInt("EndingID", ending);
            // GameController.instance.endingId = ending;
            GameController.instance.ChangeGameState(GameState.End);
            ScreenLoader.instance.LoadScene("EndingScene");
            return;
        }

        // 3️⃣ Scene transition (story scene variable)
        if (!string.IsNullOrEmpty(sceneVar))
        {
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneVar != currentSceneName)
            {
                Debug.Log($"Switching scene to: {sceneVar}");
                ScreenLoader.instance.LoadScene(sceneVar);
                return;
            }
        }

        // 4️⃣ Otherwise, go back to free roam
        GameController.instance.ChangeGameState(GameState.FreeRoam);
        GameController.instance.playerController.EnablePlayerMovement();
    }

    //private void StartBattle(string enemyName, int winEnding, int loseEnding)
    //{
    // BattleManager.instance.SetupBattle(enemyName, winEnding, loseEnding);
    // ScreenLoader.instance.LoadScene("BattleScene");
    // Placeholder (randomly decide player wins or loses); to be replaced by actual battle
    //     bool playerWon = UnityEngine.Random.value > 0.5f;
    //   Debug.Log($"Randomly deciding if player wins: {playerWon}");
    // int endingToLoad = playerWon ? winEnding : loseEnding;
    // Debug.Log($"Loading ending {endingToLoad}");
    // PlayerPrefs.SetInt("EndingID", endingToLoad);
    // GameController.instance.endingId = endingToLoad;
    // GameController.instance.ChangeGameState(GameState.End);
    //ScreenLoader.instance.LoadScene("EndingScene");
    //}

    private void StartBattle(string enemyName, int winEnding, int loseEnding, string resumeWin, string resumeLose)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        GameObject battleContextObj = new GameObject("BattleContext");
        BattleContext context = battleContextObj.AddComponent<BattleContext>();
        context.SetupBattle(enemyName, winEnding, loseEnding, resumeWin, resumeLose, currentScene);

        GameController.instance.ChangeGameState(GameState.Battle);
        ScreenLoader.instance.LoadScene("BattleScene");
    }
}
