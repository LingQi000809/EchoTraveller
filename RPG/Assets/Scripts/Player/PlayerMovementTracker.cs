using UnityEngine;

public class PlayerMovementTracker : MonoBehaviour
{
    [SerializeField] private float enterWorldTriggerDistance = 1f;
    [SerializeField] private float translatorTriggerDistance = 3f;

    private Vector3 lastPosition;
    private bool enterWorldTriggered = false;
    private bool translatorTriggered = false;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (enterWorldTriggered && translatorTriggered)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, lastPosition);

        if (!enterWorldTriggered && distance >= enterWorldTriggerDistance)
        {
            enterWorldTriggered = true;
            DialogManager.instance.EnterDialogue("EnterTheWorld");
            lastPosition = transform.position;
        }
        else if (!translatorTriggered && distance >= translatorTriggerDistance)
        {
            translatorTriggered = true;
            DialogManager.instance.EnterDialogue("Device");
            lastPosition = transform.position;
        }
    }
}