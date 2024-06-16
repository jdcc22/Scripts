using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    [SerializeField] LittleMac mac;

    public void Damage(int damage, bool ignoresBlock)
    {
        mac.Damage(damage, ignoresBlock);
        
    }
}
