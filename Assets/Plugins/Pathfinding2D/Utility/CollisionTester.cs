using UnityEngine;
using System.Collections;

public class CollisionTester : MonoBehaviour
{


    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(name+" entered a collision with "+ collision.collider.name);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log(name + " stayed in a collision with "+ collision.collider.name);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log(name + " exit a collision with "+ collision.collider.name);
    }

    void Update()
    {
        Rigidbody2D r2 = GetComponent<Rigidbody2D>();
        if (r2 != null)
        {
            Debug.DrawRay(transform.position, r2.velocity, Color.green);
        }
    }
}
