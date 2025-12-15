using System.Collections;
using UnityEngine;

public class NoteBubble : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	private float lifetime;

	public void Init(Sprite sprite, float duration)
	{
		spriteRenderer.sprite = sprite;
		lifetime = duration;
	}

	public void SetLifetime(float duration)
	{
		lifetime = duration;
	}

	private void Update()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0f)
			Destroy(gameObject);
	}
	
	public void FadeOut(float duration)
	{
		StartCoroutine(FadeRoutine(duration));
	}

	private IEnumerator FadeRoutine(float duration)
	{
		float t = 0f;
		Color startColor = spriteRenderer.color;
		while (t < duration)
		{
			t += Time.deltaTime;
			spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0, t / duration));
			yield return null;
		}
		Destroy(gameObject);
	}
}