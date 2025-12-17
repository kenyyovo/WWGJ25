using System.Collections;
using UnityEngine;

public class InstrumentCollectible : MonoBehaviour
{
    [SerializeField] private Animation collectAnimation;
    
    private bool unlockedP1;
    private bool unlockedP2;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") && !unlockedP1)
        {
            Unlocks.UnlockMusicModeP1();
            unlockedP1 = true;
            collectAnimation.Play("CollectibleCollected");
            StartCoroutine(DespawnRoutine());
        }

        if (other.CompareTag("Player2") && !unlockedP2)
        {
            Unlocks.UnlockMusicModeP2();
            unlockedP2 = true;
            collectAnimation.Play("CollectibleCollected");
            StartCoroutine(DespawnRoutine());
        }
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        Destroy(gameObject);
    }
}
