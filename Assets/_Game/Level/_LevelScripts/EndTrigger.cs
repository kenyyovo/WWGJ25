using System;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer p1Indicator;
    [SerializeField] private SpriteRenderer p2Indicator;
    [SerializeField] private SceneLoader sceneLoader;
    
    private bool player1Entered;
    private bool player2Entered;

    private void OnEnable()
    {
        player1Entered = false;
        player2Entered = false;
        p1Indicator.enabled = false;
        p2Indicator.enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            player1Entered = true;
            p1Indicator.enabled = true;
        }

        if (other.CompareTag("Player2"))
        {
            player2Entered = true;
            p2Indicator.enabled = true;
        }

        if (player1Entered && player2Entered)
        {
            sceneLoader.TransitionScene();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            player1Entered = false;
            p1Indicator.enabled = false;
        }

        if (other.CompareTag("Player2"))
        {
            player2Entered = false;
            p2Indicator.enabled = false;
        }
    }
}
