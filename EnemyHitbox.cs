using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    EnemyHurtbox.IFighter fighter;

    private void Start()
    {
        fighter = GetComponentInParent<EnemyHurtbox.IFighter>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hurtbox = collision.collider.gameObject;
        PlayerHurtbox playerHurtbox = hurtbox.GetComponent<PlayerHurtbox>();
        if (playerHurtbox != null )
        {
            playerHurtbox.Damage(fighter.GetDamageDealt(), fighter.GetBlockRestrictions());
            fighter.Hit();
        }
    }
}
