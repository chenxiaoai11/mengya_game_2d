using UnityEngine;

public class InteractHint : MonoBehaviour
{
    public GameObject hintUI;
    public KeyCode interactKey = KeyCode.E;
    private bool playerNearby;
    private GameObject cachedPlayer;
    private IInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<IInteractable>();
        if (hintUI != null) hintUI.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            cachedPlayer = other.gameObject;
            if (hintUI != null) hintUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!enabled) return;
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            cachedPlayer = null;
            if (hintUI != null) hintUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerNearby) return;
        if (interactable == null) return;
        if (!interactable.CanInteract(cachedPlayer)) return;
        if (Input.GetKeyDown(interactKey))
        {
            interactable.Interact(cachedPlayer);
        }
    }
}
