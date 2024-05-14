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

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision");

        if (collision.gameObject.tag == "Player" && collision.gameObject != caster)
        {
            Debug.Log("Hit player");
            // Apply force or effect to the hit character
            ApplyPushbackForce(collision.gameObject);
            GetComponent<Collider2D>().enabled = false;
            // Destroy(gameObject);
        }
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
