using System;
using Unity.VisualScripting;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField]
    private float speed = 8f;

    private void Start()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.up*speed;
    }
    

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collision detected!");
        Debug.Log(other.gameObject.name);
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("DeathZone"))
        {
            Debug.Log("wall detected, self destruct");
            Destroy(this.gameObject);
        }
        
    }

}
