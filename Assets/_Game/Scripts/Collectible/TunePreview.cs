using System;
using System.Collections;
using UnityEngine;

public class TunePreview : MonoBehaviour
{
    [SerializeField] private SoundType tune0;
    [SerializeField] private SoundType tune1;
    [SerializeField] private SoundType tune2;
    [SerializeField] private SoundType tune3;
    
    [SerializeField] private Animation tuneAnimation;

    private bool isPlaying;
    
    private void OnDisable()
    {
        StopAllCoroutines();    
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlaying) return;
        
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            StartCoroutine(TunePreviewRoutine());
            isPlaying = true;
        }
    }

    private IEnumerator TunePreviewRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        tuneAnimation.Play();
        AudioManager.PlaySound(tune0);

        yield return new WaitForSeconds(1f);
        
        tuneAnimation.Play();
        AudioManager.PlaySound(tune1);
        
        yield return new WaitForSeconds(1f);
        
        tuneAnimation.Play();
        AudioManager.PlaySound(tune2);
        
        yield return new WaitForSeconds(1f);
        
        tuneAnimation.Play();
        AudioManager.PlaySound(tune3);
        
        yield return new WaitForSeconds(0.5f);
        
        isPlaying = false;
    }
}
