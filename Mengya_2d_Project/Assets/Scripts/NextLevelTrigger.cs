using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour, IInteractable
{
    public string nextSceneName;
    public int nextSceneBuildIndex = -1;

    void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }

    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        LoadNextLevel();
    }
}
