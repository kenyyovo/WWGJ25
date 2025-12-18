using System;
using System.Collections;
using UnityEngine;

public class Effect1Collectible : MonoBehaviour
{
    [SerializeField] private Animation collectAnimation;
    [SerializeField] private GameObject[] tunePreviews;
    
    private bool collected;

    private void OnEnable()
    {
        foreach (GameObject tune in tunePreviews)
        {
            tune.SetActive(false);
        }   
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1") && !collected || other.CompareTag("Player2") && !collected)
        {
            Unlocks.UnlockEffect1();
            collected = true;
            collectAnimation.Play("CollectibleCollected");
            StartCoroutine(DespawnRoutine());
        }
    }
    
    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        foreach (GameObject tune in tunePreviews)
        {
            tune.SetActive(true);
        }   
        
        Destroy(gameObject);
    }
}