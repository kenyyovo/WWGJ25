using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button startButton;
    [SerializeField] private PlayerController p1Controller;
    [SerializeField] private PlayerController p2Controller;
 
    private Camera mainCamera;
    private void OnEnable()
    {
        mainCamera = Camera.main;
        mainCamera.transform.position = new Vector3(0, 32, -10);
        
        p1Controller.ToggleControls(false);
        p2Controller.ToggleControls(false);
        
        startButton.onClick.AddListener(OnStart);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
    }

    private void OnStart()
    {
        startButton.onClick.RemoveAllListeners();
        
        Unlocks.ResetAllUnlocks();
        p1Controller.ToggleControls(true);
        p2Controller.ToggleControls(true);
        
        StartCoroutine(MoveCameraRoutine());
    }

    private IEnumerator MoveCameraRoutine()
    {
        float elapsed = 0f;
        float duration = 60f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(0, 0, -10), t);

            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
