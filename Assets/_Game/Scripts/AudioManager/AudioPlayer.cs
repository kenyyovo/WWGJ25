using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] SoundType soundType;
    [SerializeField] float volume = 0.5f;
    
    public void PlayAudio()
    {
        AudioManager.PlaySound(soundType, volume);
    }
}
