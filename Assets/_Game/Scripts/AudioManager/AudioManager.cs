using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SoundType
{
	P1Note0,
	P1Note1,
	P1Note2,
	P1Note3,
	P2Note0,
	P2Note1,
	P2Note2,
	P2Note3,
	P1BadEffect,
	P2BadEffect,
	ToggleMode,
	ButtonClick,
	DoubleJump,
	Collectible,
	Boxed,
	Gravity,
	Flatten
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class AudioManager : MonoBehaviour
{
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private SoundList[] soundList;
    
	private static AudioManager instance;

	private void Awake()
	{
		instance = this;
	}

	public static void PlaySound(SoundType sound, float volume = 0.5f)
	{
		AudioClip[] clips = instance.soundList[(int)sound].Sounds;
		AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        
		if (clips.Length == 0)
			return;

		instance.audioSource.PlayOneShot(randomClip, volume);
	}
    
#if UNITY_EDITOR
	private void OnEnable()
	{
		string[] names = Enum.GetNames(typeof(SoundType));
		Array.Resize(ref soundList, names.Length);

		for (int i = 0; i < soundList.Length; i++)
		{
			soundList[i].name = names[i];
		}
	}
#endif
    
}

[Serializable]
public struct SoundList
{
	public AudioClip[] Sounds { get => sounds; } 
	[HideInInspector] public string name;
	[SerializeField] private AudioClip[] sounds;
}