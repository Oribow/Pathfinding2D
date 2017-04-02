using UnityEngine;
using System.Collections;

public class JumpArcTest : MonoBehaviour {
    public float xVel;
    public float jumpVel;
    public float gravity;
    public float gravityCap;
    public float maxJumpVel;
    public float tTimeOut;
    public Vector2 startPos;
    public Vector2 targetPos;

    public float deltaTimePerIt;
    public int iterations;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        Vector2 prevPos = startPos;
        Vector2 velocity = new Vector2(xVel, jumpVel);
        Vector2 swapPos;
        for (int it = 1; it < iterations; it++)
        {
            velocity.y -= gravity * deltaTimePerIt;
            velocity.y = Mathf.Min(gravityCap, velocity.y);
            swapPos = prevPos + velocity * deltaTimePerIt;
            //Gizmos.DrawLine(prevPos, swapPos);
            prevPos = swapPos;
        }

        prevPos = new Vector2(-iterations * xVel, -gravity * Mathf.Pow(-iterations * xVel, 2) + jumpVel * -iterations * xVel);
        for (int x = -iterations; x < iterations; x++)
        {
            float y = -gravity * Mathf.Pow(x * xVel, 2) + jumpVel * x * xVel;
            swapPos = new Vector2(x * xVel,  y);
            //Gizmos.DrawLine(startPos + prevPos, startPos+ swapPos);
            prevPos = swapPos;
        }
        Gizmos.color = Color.blue;
        for (int t = 0; t < iterations; t++)
        {
            float x = xVel * t;
            float y = (jumpVel - gravity * t) * t;

            swapPos = new Vector2(x, y);
            Gizmos.DrawLine(startPos + prevPos, startPos + swapPos);
            prevPos = swapPos;
        }

        for (int t = 0; t < iterations; t++)
        {
            float x = -xVel * t;
            float y = (jumpVel - gravity * t) * t;

            swapPos = new Vector2(x, y);
            Gizmos.DrawLine(startPos + prevPos, startPos + swapPos);
            prevPos = swapPos;
        }
        float zero = jumpVel / gravity;
        Gizmos.DrawSphere(startPos + new Vector2(xVel * zero, (jumpVel - gravity * zero) * zero), 2);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPos, 2);
        Gizmos.color = Color.white;
        float targetT = (targetPos.x - startPos.x) / xVel;
        float targetJ = ((targetPos.y - startPos.y) / targetT) + gravity * targetT;

        for (int t = 0; t < iterations; t++)
        {
            float x = xVel * t;
            float y = (targetJ - gravity * t) * t;

            swapPos = new Vector2(x, y);
            Gizmos.DrawLine(startPos + prevPos, startPos + swapPos);
            prevPos = swapPos;
        }

        //Bounds
        float maxY = (maxJumpVel * maxJumpVel) / (4 * gravity);
        Bounds b = new Bounds(startPos, new Vector2(iterations * xVel * 2, maxY * 2));
        b.min = new Vector2(b.min.x, startPos.y + (maxJumpVel - gravity * iterations) * iterations);
        DebugExtension.DrawBounds(b);
    }
}
