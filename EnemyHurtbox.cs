using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{

    [SerializeField] DamageArea area;

    IFighter fighter;
    public enum DamageArea
    {
        Head,
        Body
    }

    

    private void Start()
    {
        fighter = GetComponentInParent<IFighter>();
        
    }

    public interface IFighter
    {
        void Damage(DamageArea area, int damage, int side, bool star);
        int GetDamageDealt();
        bool GetBlockRestrictions();

        void Hit();
    }

    public void Damage(int damage, int side, bool star)
    {
        fighter.Damage(area, damage, side, star);
    }
}
