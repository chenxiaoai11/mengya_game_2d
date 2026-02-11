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
            Debug.Log($"[出口交互] 玩家进入触发区 | 对象: {gameObject.name}");
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
            Debug.Log($"[出口交互] 玩家离开触发区 | 对象: {gameObject.name}");
        }
    }

    void Update()
    {
        if (!playerNearby) return;
        if (interactable == null)
        {
            Debug.LogWarning($"[出口交互] IInteractable 未找到 | 对象: {gameObject.name}");
            return;
        }
        if (!interactable.CanInteract(cachedPlayer))
        {
            Debug.Log($"[出口交互] CanInteract=false | 对象: {gameObject.name}");
            return;
        }
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[出口交互] 按键触发: {interactKey} | 对象: {gameObject.name}");
            interactable.Interact(cachedPlayer);
        }
    }
}
