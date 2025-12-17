using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleRow : MonoBehaviour
{
	[SerializeField] private NoteBubble bubblePrefab;
	[SerializeField] private float spacing = 0.45f;
	[SerializeField] private Animation animation;

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
		float duration = matched ? 1.2f : .5f;

		if (matched)
		{
			animation.Play("RightSequenceAnim");
		}
		else
		{
			animation.Play("WrongSequenceAnim");
		}

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
		
		transform.localScale = Vector3.one;
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