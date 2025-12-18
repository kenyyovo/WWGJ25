using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private GameObject indicator;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject[] objectsToActivate;

    private bool isPressed;

    private void OnEnable()
    {
        indicator.SetActive(false);
        ToggleObjects(false);
        isPressed = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController player))
            return;

        if (player.isBox && !isPressed)
        {
            isPressed = true;
            indicator.SetActive(true);
            ToggleObjects(true);
            buttonAnimator.SetTrigger("OnButton");
        }
        else if (!player.isBox && isPressed)
        {
            isPressed = false;
            indicator.SetActive(false);
            ToggleObjects(false);
            buttonAnimator.SetTrigger("OnButtonInvalid");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController _))
            return;

        isPressed = false;
        indicator.SetActive(false);
        ToggleObjects(false);
        buttonAnimator.SetTrigger("OffButton");
    }

    private void ToggleObjects(bool toggle)
    {
        foreach (var obj in objectsToActivate)
            obj.SetActive(toggle);
    }
}