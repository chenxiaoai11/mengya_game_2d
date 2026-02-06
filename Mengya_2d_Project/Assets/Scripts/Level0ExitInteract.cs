using UnityEngine;
using UnityEngine.Playables;

public class Level0ExitInteract : MonoBehaviour, IInteractable
{
    public ExitConfirmUI ui;
    public PlayableDirector timelineDirector;
    private int step = 0;
    private readonly string[] messages = new[]
    {
        "真的要走吗",
        "确定要走吗",
        "一定要走吗"
    };

    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        step = 0;
        ShowStep();
    }

    private void ShowStep()
    {
        if (ui == null) return;
        if (step < messages.Length)
        {
            ui.Show(messages[step], OnConfirm, OnCancel);
        }
        else
        {
            StartTimeline();
        }
    }

    private void OnConfirm()
    {
        step++;
        ShowStep();
    }

    private void OnCancel()
    {
        ui?.Hide();
    }

    private void StartTimeline()
    {
        ui?.Hide();
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }
    }
}
