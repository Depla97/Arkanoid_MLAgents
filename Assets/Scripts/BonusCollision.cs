using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusCollision : MonoBehaviour
{
    public Pad playerPad;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("DeathZone"))
        { 
            //Debug.Log("Bonus hit deathZone remove bonus.");
            playerPad.LocalBonus.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
