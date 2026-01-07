using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Dialog, Battle, End }

public class GameController : MonoBehaviour
{
    public static GameController instance { get; private set; }

    [SerializeField] public PlayerController playerController;
    public GameState state;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found duplicate GameController, destroying this one.");
            Destroy(gameObject);
            return;
        }
        instance = this;
        // DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        FindAndAssignPlayer();
    }

    private void FindAndAssignPlayer()
    {
        PlayerController foundPlayer = FindFirstObjectByType<PlayerController>();

        if (foundPlayer != null)
        {
            playerController = foundPlayer;
            Debug.Log($"GameController: Found and assigned PlayerController: {foundPlayer.name}");

            if (state != GameState.Dialog && state != GameState.Battle)
            {
                ChangeGameState(GameState.FreeRoam);
            }
        }
        else
        {
            Debug.LogWarning("GameController: No PlayerController found in scene!");
        }
    }

    public void ChangeGameState(GameState newState)
    {
        this.state = newState;
        Debug.Log($"GameState changed to: {newState}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Screen.fullScreen = !Screen.fullScreen;
            Debug.Log($"Fullscreen toggled: {Screen.fullScreen}");
        }

        if (state == GameState.FreeRoam)
        {
            if (playerController != null)
            {
                playerController.HandleUpdate();
            }
        }
        else if (state == GameState.Dialog)
        {
            if (DialogManager.instance != null)
            {
                DialogManager.instance.HandleUpdate();
            }
        }
        else if (state == GameState.Battle)
        {
        }
    }
}
