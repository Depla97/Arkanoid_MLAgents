using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DeathZone"))
        { 
            //Debug.Log("Bonus hit deathZone remove bonus.");
            Destroy(this.gameObject);
        }
    }
}
