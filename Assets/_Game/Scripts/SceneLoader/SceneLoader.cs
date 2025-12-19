using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Animation fadeAnimation;
    
    private void OnEnable()
    {
        StartCoroutine(FadeOutRoutine());
    }

    public void TransitionScene()
    {
        StartCoroutine(TransitionSceneRoutine());
    }

    public void TransitionSceneName(string name)
    {
        StartCoroutine(TransitionSceneNameRoutine(name));
    }

    private IEnumerator FadeOutRoutine()
    {
        canvasGroup.alpha = 1;
        
        fadeAnimation.Play("FadeOut");
        
        yield return new WaitForSeconds(1.25f);
        
        canvasGroup.alpha = 0;
    }

    private IEnumerator TransitionSceneRoutine()
    {
        yield return new WaitForSeconds(1.25f);
        
        canvasGroup.alpha = 1;
        
        fadeAnimation.Play("FadeIn");
        
        yield return new WaitForSeconds(1.25f);
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    
    private IEnumerator TransitionSceneNameRoutine(string name)
    {
        yield return new WaitForSeconds(1.25f);
        
        canvasGroup.alpha = 1;
        
        fadeAnimation.Play("FadeIn");
        
        yield return new WaitForSeconds(1.25f);
        
        SceneManager.LoadScene(name);
    }
}
