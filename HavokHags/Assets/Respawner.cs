using UnityEngine;

public class Respawner : MonoBehaviour
{
    [SerializeField] private float respawnHeight = 5f;

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.position = Vector2.up * respawnHeight;
        }
    }
}