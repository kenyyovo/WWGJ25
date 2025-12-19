using System.Collections;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer p1Indicator;
    [SerializeField] private SpriteRenderer p2Indicator;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private Animation endingAnimation;
    
    private bool player1Entered;
    private bool player2Entered;
    
    private Camera mainCamera;

    private bool hasTriggeredEnding;

    private void OnEnable()
    {
        player1Entered = false;
        player2Entered = false;
        p1Indicator.enabled = false;
        p2Indicator.enabled = false;
        
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggeredEnding) return;
        
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
            hasTriggeredEnding = true;
            StartCoroutine(EndingRoutine());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasTriggeredEnding) return;
        
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

    private IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(1f);
        
        StartCoroutine(MoveCameraRoutine());
        
        yield return new WaitForSeconds(3f);
        
        endingAnimation.Play("ShowEndingScreen");
        
        yield return new WaitForSeconds(3f);

        StartCoroutine(InputRoutine());
    }
    
    private IEnumerator MoveCameraRoutine()
    {
        float elapsed = 0f;
        float duration = 60f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(0, 82, -10), t);

            yield return null;
        }
    }

    private IEnumerator InputRoutine()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                sceneLoader.TransitionSceneName("IntroScene");
            }
            
            yield return null;
        }
    }
}
