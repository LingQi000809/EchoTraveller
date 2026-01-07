using UnityEngine;

public class MayorController : MonoBehaviour, Interactable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueKnotName = "MeetMayor";

    public void Interact()
    {
        DialogManager.instance.EnterDialogue(dialogueKnotName);
    }
}
