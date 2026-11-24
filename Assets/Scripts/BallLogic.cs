

using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BallLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    [SerializeField] public float BallSpeed=17f;

    Vector2 surfaceNormal=new Vector2();
    Vector2 direction= new Vector2();
    [SerializeField]GameObject paddle;
    
    void Start()
    {
        BallSpawn();

    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Object Hit");
        if (collision.contactCount > 0)
        {
           surfaceNormal = collision.contacts[0].normal;  
        } 
        direction = (direction - 2 * Vector2.Dot(direction, surfaceNormal) * surfaceNormal).normalized;
        //direction = (direction + new Vector2(UnityEngine.Random.Range(-0.15f , 0.15f),UnityEngine.Random.Range(-0.15f , 0.15f))).normalized; //Adding randomization to bounce
        rb.linearVelocity=direction*BallSpeed;
    }

    public void BallSpawn()
    {
      transform.position= new Vector3(paddle.transform.position.x,-2f,0);
      direction = new Vector2(UnityEngine.Random.Range(-1f, 1f),UnityEngine.Random.Range(0.5f, 1f)).normalized;
      rb.linearVelocity = direction * BallSpeed;

    }
    

    
}
