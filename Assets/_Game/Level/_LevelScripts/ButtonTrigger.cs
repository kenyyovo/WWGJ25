using UnityEngine;

[System.Serializable]
public class ButtonTarget
{
    public GameObject target;
    public bool enableOnPress = true;
}

public class ButtonTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] indicators;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private ButtonTarget[] objectsToToggle;

    private bool isPressed;
    private PlayerController currentPlayer1;
    private bool player2OnButton;

    private void OnEnable()
    {
        ResetButton();
        buttonAnimator.SetInteger("ButtonState", 0);    
        ToggleObjects(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                currentPlayer1 = player;
                player.OnBoxStateChanged += HandleBoxStateChanged;

                buttonAnimator.SetInteger("ButtonState", player.isBox ? 2 : 1);
            }
        }
        
        else if (other.CompareTag("Player2"))
        {
            player2OnButton = true;
            ForcePress();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1") &&
            currentPlayer1 != null &&
            other.TryGetComponent(out PlayerController player) &&
            player == currentPlayer1)
        {
            player.OnBoxStateChanged -= HandleBoxStateChanged;
            currentPlayer1 = null;

            if (!player2OnButton)
            {
                ResetButton();
                buttonAnimator.SetInteger("ButtonState", 0);
            }
        }
        
        else if (other.CompareTag("Player2"))
        {
            player2OnButton = false;
            
            if (currentPlayer1 != null)
            {
                if (currentPlayer1.isBox)
                {
                    PressButton();
                    buttonAnimator.SetInteger("ButtonState", 2);
                }
                else
                {
                    ReleaseButton(1);
                }
            }
            else
            {
                ResetButton();
                buttonAnimator.SetInteger("ButtonState", 0);
            }
        }

    }

    private void HandleBoxStateChanged(bool isBox)
    {
        if (player2OnButton)
            return;
        
        if (!isBox && !isPressed)
        {
            buttonAnimator.SetInteger("ButtonState", 1);
            return;
        }

        if (isBox && !isPressed)
        {
            PressButton();
        }
        else if (!isBox && isPressed)
        {
            ReleaseButton(1);
        }
    }
    
    private void ForcePress()
    {
        isPressed = true;
        ToggleObjects(true);
        buttonAnimator.SetInteger("ButtonState", 2);
        
        foreach (GameObject indicator in indicators) indicator.SetActive(true);
    }

    private void PressButton()
    {
        isPressed = true;
        ToggleObjects(true);
        AudioManager.PlaySound(SoundType.ButtonClick);
        buttonAnimator.SetInteger("ButtonState", 2);
        
        foreach (GameObject indicator in indicators) indicator.SetActive(true);
    }

    private void ReleaseButton(int state)
    {
        isPressed = false;
        ToggleObjects(false);
        buttonAnimator.SetInteger("ButtonState", state);
        
        foreach (GameObject indicator in indicators) indicator.SetActive(false);
    }

    private void ResetButton()
    {
        isPressed = false;
        ToggleObjects(false);
        
        foreach (GameObject indicator in indicators) indicator.SetActive(false);
    }

    private void ToggleObjects(bool pressed)
    {
        foreach (var obj in objectsToToggle)
        {
            obj.target.SetActive(pressed == obj.enableOnPress);
        }
    }
}
