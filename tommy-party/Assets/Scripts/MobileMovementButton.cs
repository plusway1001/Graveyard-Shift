
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMovementButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement playerMovement;

    public string direction;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playerMovement != null)
        {
            playerMovement.SetDirection(direction, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (playerMovement != null)
        {
            playerMovement.SetDirection(direction, false);
        }
    }

    private void OnDisable()
    {
        // Prevent movement from getting stuck if the button is disabled.
        if (playerMovement != null)
        {
            playerMovement.SetDirection(direction, false);
        }
    }
}
