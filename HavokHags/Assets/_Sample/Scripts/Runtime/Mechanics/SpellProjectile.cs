using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float pushbackForce = 2f;
    public GameObject caster;

    public bool isLaunched = false;
    public Vector2 CastDirection;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Collider2D>().enabled = false;

        // enable collider in 2 seconds
        Invoke("EnableCollider", 0.2f);
            
        // Destroy this game object after a random delay
        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        if (caster == null || !isLaunched)
        {
            return;
        }

        GetComponent<Rigidbody2D>().velocity = speed * CastDirection;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == caster)
        {
            // Destroy the projectile when it hits the ground
            return;
        }

        if (collision.gameObject.tag == "Player" && collision.gameObject != caster)
        {
            // Apply force or effect to the hit character
            ApplyPushbackForce(collision.gameObject);
            GetComponent<Collider2D>().enabled = false;
        }

        Destroy(gameObject);
    }


    private void EnableCollider()
    {
        GetComponent<Collider2D>().enabled = true;
    }

    private void ApplyPushbackForce(GameObject hitCharacter)
    {
        Rigidbody2D hitRigidbody = hitCharacter.GetComponent<Rigidbody2D>();

        if (hitRigidbody != null)
        {
            Debug.Log("Applying pushback force");
            Vector2 forceDirection = (hitCharacter.transform.position - transform.position).normalized;
            hitRigidbody.AddForce(forceDirection * pushbackForce, ForceMode2D.Impulse);
        }
    }
}
