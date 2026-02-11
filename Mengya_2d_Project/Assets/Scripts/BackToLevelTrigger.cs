using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLevelTrigger : MonoBehaviour, IInteractable
{
    public string targetSceneName;

    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            TimelineSkipManager.SetSkipOnce(targetSceneName);
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
