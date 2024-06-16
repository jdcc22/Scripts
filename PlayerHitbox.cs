using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] LittleMac mac;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        GameObject hurtbox = collision.collider.gameObject;
        EnemyHurtbox enemyHurtbox = hurtbox.GetComponent<EnemyHurtbox>();
        if (enemyHurtbox != null )
        {
            enemyHurtbox.Damage(mac.damageAmount, mac.side, mac.isDoingStarPunch);
        }
    }
}
