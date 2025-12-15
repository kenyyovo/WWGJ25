using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleRow : MonoBehaviour
{
	[SerializeField] private NoteBubble bubblePrefab;
	[SerializeField] private float spacing = 0.45f;
	[SerializeField] private float failLifetime = 1f;
	[SerializeField] private float successLifetime = 2f;

	private readonly List<NoteBubble> bubbles = new();

	public void AddBubble(Sprite sprite)
	{
		if (bubbles.Count >= 4)
			return;

		NoteBubble bubble = Instantiate(bubblePrefab, transform);
		bubble.transform.localPosition =
			new Vector3(bubbles.Count * spacing, 0f, 0f);

		bubble.Init(sprite, Mathf.Infinity); 
		bubbles.Add(bubble);
	}

	public void Resolve(bool matched, System.Action onComplete)
	{
		float duration = matched ? 2f : 0.5f;

		foreach (var bubble in bubbles)
		{
			bubble.FadeOut(duration);
		}
		
		StartCoroutine(DelayedClear(duration, onComplete));
	}

	private IEnumerator DelayedClear(float duration, System.Action onComplete)
	{
		NoteBubble[] bubblesSnapshot = bubbles.ToArray();

		yield return new WaitForSeconds(duration);

		foreach (var bubble in bubblesSnapshot)
		{
			if (bubble != null) 
				Destroy(bubble.gameObject);
		}

		bubbles.Clear();
		onComplete?.Invoke();
	}


	public void ClearImmediate()
	{
		bubbles.RemoveAll(b => b == null);

		foreach (var b in bubbles)
		{
			if (b != null)
				Destroy(b.gameObject);
		}

		bubbles.Clear();
	}

}