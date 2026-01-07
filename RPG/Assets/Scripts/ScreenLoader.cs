using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenLoader : MonoBehaviour
{
    public Animator transition;
    public static ScreenLoader instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one GameController in the scene.");
        }
        instance = this;
    }

    public void LoadScene(string sceneName)
    {
        transition.SetTrigger("Start");
        SceneManager.LoadScene(sceneName);
    }
}
