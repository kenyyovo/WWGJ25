using UnityEngine;

public class Respawner : MonoBehaviour
{
    [SerializeField] private Transform p1RepsawnPoint;
    [SerializeField] private Transform p2RepsawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
        {
            other.transform.position = p1RepsawnPoint.position;
        }

        if (other.CompareTag("Player2"))
        {
            other.transform.position = p2RepsawnPoint.position;
        }
    }
}
