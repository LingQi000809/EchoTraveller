using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleContext : MonoBehaviour
{
    public static BattleContext instance { get; private set; }

    public string enemyName;
    public int winEndingID;
    public int loseEndingID;
    public string resumeWin;
    public string resumeLose;
    public string returnScene;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetupBattle(
        string enemy, int winEnding, int loseEnding,
        string resumeWinKnot, string resumeLoseKnot, string sceneToReturn
    )
    {
        enemyName = enemy;
        winEndingID = winEnding;
        loseEndingID = loseEnding;

        resumeWin = resumeWinKnot;
        resumeLose = resumeLoseKnot;
        returnScene = sceneToReturn;

        Debug.Log($"BattleContext: Setup battle with {enemy}, Win={winEnding}, Lose={loseEnding}, WinResume={resumeWin}, LoseResume={resumeLose}, ReturnScene={sceneToReturn}");
    }

    public void ClearContext()
    {
        if (instance == this)
        {
            Destroy(gameObject);
            instance = null;
        }
    }

    public void ResumeAfterBattle(string resumeScene, string resumeKnot)
    {
        StartCoroutine(ResumeAfterBattleCoroutine(resumeScene, resumeKnot));
    }

    private IEnumerator ResumeAfterBattleCoroutine(string resumeScene, string resumeKnot)
    {
        // 1. Load the scene
        Debug.Log($"Loading scene {resumeScene}");
        var loadOp = SceneManager.LoadSceneAsync(resumeScene);

        while (!loadOp.isDone)
            yield return null;

        // 2. Wait one frame so Awake/Start fire
        yield return null;

        // 3. Wait for DialogManager to exist in the new scene
        while (DialogManager.instance == null)
        {
            Debug.Log("Waiting for DialogManager to spawn...");
            yield return null;
        }

        // 4. Resume dialogue
        Debug.Log($"Resuming dialogue at knot: {resumeKnot}");
        DialogManager.instance.EnterDialogue(resumeKnot);
        GameController.instance.ChangeGameState(GameState.Dialog);

        // 5. clear context
        ClearContext();
    }
}
