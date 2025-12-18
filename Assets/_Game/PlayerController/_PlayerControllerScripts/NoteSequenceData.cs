using UnityEngine;

[CreateAssetMenu(fileName = "NoteSequenceData", menuName = "CoupleSlop/NoteSequenceData")]
public class NoteSequenceData : ScriptableObject
{
	[Header("All Sequences for This Player")]
	public Sequence[] sequences; 

	[System.Serializable]
	public struct Sequence
	{
		public string sequenceName; 
		public int[] notes;
		
		public string unlockKey;
	}
}