using UnityEngine;

public class DragonController : MonoBehaviour, Interactable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueKnotName = "MeetDragon";

    public void Interact()
    {
        DialogManager.instance.EnterDialogue(dialogueKnotName);
    }
}
