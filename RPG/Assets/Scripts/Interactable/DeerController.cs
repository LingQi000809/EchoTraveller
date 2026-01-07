using UnityEngine;

public class DeerController : MonoBehaviour, Interactable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueKnotName = "MeetDeer";

    public void Interact()
    {
        DialogManager.instance.EnterDialogue(dialogueKnotName);
    }
}
