using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;


public class DonFlamencoContender : MonoBehaviour, EnemyHurtbox.IFighter
{
    public AreaValues bodyState { get; set; } = AreaValues.Block;
    public AreaValues headState { get; set; } = AreaValues.Open;

    [SerializeField] int maxHealth;
    [ReadOnly] public int health;

    bool combo;
    float comboTimer;
    [SerializeField] float healthInterpolation;

    [SerializeField] Image whiteHealthbar;
    [SerializeField] Image RedHealthbar;
    [SerializeField] float intermissionHealthRegenMultiplier;

    int damageAmount;

    bool ignoreBlock;
    public bool stunned;
    [SerializeField] bool comboLock;
    bool hitOpponent;
    float timeSincePause;
    [SerializeField] int maxComboHits = 2;
    int comboHits;
    bool knockedDown;
    
    int patternNumber;
    int phase;
    int maxKDs = 5;
    public bool hookStun;
    public bool starOpportunity;
    
    [SerializeField] float hookTimer;
    
    [SerializeField] int starsNeededtoKD = 1;

    public int counterNumber { get; set; } //1 = Left, 2 = Right


    int getUpAtCount;

    [SerializeField] float interval;
    [SerializeField] Animator anim;
    [AnimatorParam("anim")] public int animStunLock;
    
    [AnimatorParam("anim")] public int animSide;
    [AnimatorParam("anim")] public int animBodyLocation;
    [AnimatorParam("anim")] public int animKnockdown;


    const string jabName = "Jab";
    const string rightHookName = "Right Hook";
    const string rightUpperName = "Right Upper";
    const string getHitName = "Get Hit";
    const string stunBreakName = "Stun Break";
    const string getKDName = "Get KD";
    const string leftUpperName = "Left Upper";
    

    FightManager fightManager;

    [SerializeField] bool pausedFight;

    int timesOpponentBeenHit;

    public bool taunt;

    bool dodgedLastHit;

    int counterattackMove;
    

    public enum AreaValues
    {
        Dodge,
        Block,
        Open, 
        Invincible
    }

    [SerializeField] AudioSource audioSource;

    

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        interval = 2.2f;
        patternNumber = 0;
        pausedFight = true;
        
        phase = 1;
        fightManager = FindObjectOfType<FightManager>();

        fightManager.PauseFight += FightManager_PauseFight;
        fightManager.OpponentEntrance += FightManager_OpponentEntrance;
        fightManager.UnpauseFight += FightManager_UnpauseFight;

        fightManager.PlayerDown += FightManager_PlayerDown;
        fightManager.PlayerGotUp += FightManager_PlayerGotUp;

        fightManager.Countdown += FightManager_Countdown;

        fightManager.OpponentCelebration += FightManager_OpponentCelebration;
    }

    private void FightManager_OpponentCelebration(object sender, System.EventArgs e)
    {
        anim.Play("Celebrate");
    }

    private void FightManager_Countdown(object sender, FightManager.CountDownEventArgs e)
    {
        if (knockedDown)
        {
            if (fightManager.opponentTotalKDs < maxKDs)
            {
                if (e.count == getUpAtCount) GetUp();
            }
        }
    }

    private void FightManager_PlayerGotUp(object sender, System.EventArgs e)
    {
        pausedFight = false;
    }

    private void FightManager_PlayerDown(object sender, System.EventArgs e)
    {
        pausedFight = true;
        
    }

    private void FightManager_UnpauseFight(object sender, System.EventArgs e)
    {
        pausedFight = false;
    }

    private void FightManager_OpponentEntrance(object sender, System.EventArgs e)
    {
        health += Mathf.RoundToInt(timeSincePause * intermissionHealthRegenMultiplier);
        if (health > maxHealth) health = maxHealth;
        Debug.Log(timeSincePause);
        anim.speed = 1;
        anim.Play("Stage Entrance");
    }

    private void FightManager_PauseFight(object sender, System.EventArgs e)
    {
        pausedFight = true;
        timeSincePause = 0;
        anim.speed = 0;
        if (fightManager.currentRound < 4) Invoke("ReturnToCorner", 2.5f);

        

    }

    // Update is called once per frame
    void Update()
    {
        

        RedHealthbar.fillAmount = (float)health / maxHealth;
        if (!combo)
        {
            if (whiteHealthbar.fillAmount > RedHealthbar.fillAmount) whiteHealthbar.fillAmount -= healthInterpolation * Time.deltaTime;
        }


        if (pausedFight) timeSincePause += Time.deltaTime;

        if (comboTimer > 0)
        {
            combo = true;
            comboTimer -= Time.deltaTime;
        }
        else
        {
            combo = false;
            comboLock = false;

        }



        if (pausedFight) return;

        

        if (health < 0) health = 0;

        

        

        if (interval > 0 && !stunned && !comboLock && !knockedDown) interval -= Time.deltaTime;
        else if (!stunned && !comboLock && !knockedDown)
        { 

            Attack();
            ResetHit();

            if (!dodgedLastHit) interval = Random.Range(2f, 5f);
        }

        if (comboLock)
        {
            anim.SetFloat(animStunLock, 1);
        }
        else
        {
            anim.SetFloat(animStunLock, 0);
        }

        
    }

    void RegularWait()
    {
        if (dodgedLastHit)
        {
            switch (counterattackMove)
            {
                case 1:
                    anim.Play(jabName); break;
                case 2:
                    anim.Play(rightHookName); break;
                case 3:
                    anim.Play(rightUpperName); break;
                case 4:
                    anim.Play(leftUpperName); break;
            }
            dodgedLastHit = false;
        }
        else
        {
            anim.Play("Taunt");

        }
    }

    void InstantAttack()
    {
        if (dodgedLastHit)
        {
            RegularWait();
            patternNumber++;
            if (patternNumber > 6) patternNumber = 0;
        }
        else
        {
            int random = Random.Range(1, 101);
            if (random < 50)
            {
                Dodge(1);
            }
            else
            {
                Dodge(-1);
            }
        }
    }

    void Attack()
    {
        if (fightManager.hearts >= 1)
        {
            switch (phase)
            {
                case 1:
                    RegularWait();
                    break;
                case 2:

                    switch (patternNumber)
                    {
                        case 0:
                            InstantAttack();
                            break;
                        case 1:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 2:
                            InstantAttack();
                            break;
                        case 3:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 4:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 5:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 6:
                            InstantAttack();
                            break;
                    }
                    break;
                case 3:
                    switch (patternNumber)
                    {
                        case 0:
                            InstantAttack();
                            break;
                        case 1:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 2:
                            InstantAttack();
                            break;
                        case 3:
                            InstantAttack();
                            patternNumber++;
                            break;
                        case 4:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 5:
                            RegularWait();
                            patternNumber++;
                            break;
                        case 6:
                            InstantAttack();
                            break;
                    }
                    break;


            }
        }
        else
        {
            InstantAttack();
        }
    }

    void GetUp()
    {
        fightManager.OpponentUp();
        knockedDown = false;
        anim.SetBool(animKnockdown, false);
        health = (int)(maxHealth * Random.Range(0.5f, 0.85f));
        interval = 7f;
        if (phase != 3) phase++;
        patternNumber = 0;
        whiteHealthbar.fillAmount = (float)health / maxHealth;
    }

    public void Damage(EnemyHurtbox.DamageArea damageArea, int damage, int side, bool star)
    {
        
        
        switch (damageArea)
        {
            case EnemyHurtbox.DamageArea.Head:
                anim.SetFloat(animBodyLocation, 1);
                if (headState == AreaValues.Open)
                {
                    anim.SetFloat(animSide, side);
                    comboTimer = 1.2f;
                    if (!star)
                    {
                        // Handle Combos
                        if (stunned)
                        {
                            StartCombo();
                            fightManager.AddPoints(50);
                        }
                        else
                        {
                            if (comboLock && comboHits + 1 < maxComboHits)
                            {
                                ComboHit();
                                fightManager.AddPoints(20);
                            }
                            else if (comboLock)
                            {
                                EndCombo();
                                fightManager.AddPoints(50);
                            }
                            else
                            {
                                EndCombo();
                                fightManager.AddPoints(10);
                            }
                        }
                    }
                    else
                    {
                        fightManager.AddPoints(500);
                        EndCombo();
                        
                    }
                    

                    health -= damage;

                    
                    if (starOpportunity)
                    {
                        fightManager.EarnStar();
                        fightManager.AddPoints(100);
                    }
                    
                    if (health < 0)
                    {
                        Knockdown();
                        fightManager.AddPoints(1000);
                    }

                   

                    

                    
                    
                    
                }
                else if (headState == AreaValues.Dodge)
                {
                    Dodge(side);
                }
                else if (headState == AreaValues.Block)
                {
                    anim.SetFloat(animSide, side);
                    anim.Play("Block");
                    fightManager.DecreaseHearts(1);
                    Invoke("InstantAttack", 0.35f);
                }
                break;
            case EnemyHurtbox.DamageArea.Body:
                anim.SetFloat (animBodyLocation, -1);
                if (bodyState == AreaValues.Open) 
                {
                    anim.SetFloat(animSide, side);

                    // Handle Combos
                    if (stunned)
                    {
                        StartCombo();
                        fightManager.AddPoints(50);
                    }
                    else
                    {
                        if (comboLock && comboHits + 1 < maxComboHits)
                        {
                            ComboHit();
                            fightManager.AddPoints(20);
                        }
                        else if (comboLock)
                        {
                            EndCombo();
                            fightManager.AddPoints(50);
                        }
                        else
                        {
                            EndCombo();
                            fightManager.AddPoints(10);
                        }
                    }
                    

                    comboTimer = 1.2f;

                    health -= damage;

                    if (health < 0)
                    {
                        Knockdown();
                        fightManager.AddPoints(1000);
                    }

                    if (starOpportunity)
                    {
                        fightManager.EarnStar();
                        fightManager.AddPoints(100);
                        starOpportunity = false;
                    }

                    

                    
                }
                
                else if (bodyState == AreaValues.Dodge)
                {
                    Dodge(side);
                }
                break;
               
        }
    }

    public int GetDamageDealt()
    {
        return damageAmount;
    }

    public bool GetBlockRestrictions()
    {
        return ignoreBlock;
    }

    public void SetBlockState(int state)
    {
        if (state == 0) ignoreBlock = true;
        else ignoreBlock = false;
    }

    public void SetDamage(int damage)
    {
        damageAmount = damage;
    }

    public void CheckIfStun()
    {
        if (fightManager.hasBeenHit)
        {
            Debug.Log("You Got Hit!");
            fightManager.hasBeenHit = false;
            stunned = false;
            

            
            
        }
        else
        {
            stunned = true;
            Debug.Log("You Dodged!");
            if (!fightManager.hasBlockedLastHit && fightManager.hearts <= 0)
            {
                fightManager.ResetHearts();
                
            }
            
        }
        fightManager.hasBlockedLastHit = false;
    }

    public void UnStun()
    {
        stunned = false;
    }

    public void Hit()
    {
        hitOpponent = true;
    }

    void ResetHit()
    {
        hitOpponent = false;
    }

    

    public void GotUp()
    {
        fightManager.RestartTimer();
    }

    void EndCombo()
    {
        comboHits = 0;
        comboLock = false;
        comboTimer = 0f;
        anim.Play(stunBreakName);
        fightManager.RestartTimer();
    }

    void StartCombo()
    {
        comboLock = true;
        stunned = false;
        comboTimer = 3f;
        anim.Play(getHitName);
        fightManager.SlowTimer();
    }

    void ComboHit()
    {
        comboHits++;
        comboTimer = 3f;
        anim.Play(getHitName);
        fightManager.SlowTimer();
    }

    void Knockdown()
    {
        health = 0;
        fightManager.OpponentKD();
        switch (fightManager.opponentTotalKDs)
        {
            case 1:
                getUpAtCount = Random.Range(1, 4);
                break;
            case 2:
                getUpAtCount = Random.Range(3, 9);
                break;
            case 3:
                getUpAtCount = Random.Range(5, 10);
                break;
            case 4:
                getUpAtCount = 25;
                break;
        }
            
        anim.Play(getKDName);
        knockedDown = true;
        anim.SetBool(animKnockdown, true);
        fightManager.StopTimer();
    }

    void ReturnToCorner()
    {
        anim.Play("Not Fighting");
    }

    public void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }


    public void FinishEntrance()
    {
        fightManager.OpponentEntranceFinish();
    }


    void Dodge(int side)
    {
        anim.SetFloat(animSide, side);
        dodgedLastHit = true;
        anim.Play("Dodge");
        interval = 0.5f;
        
         
        if (side == 1)
        {
            int random = Random.Range(1, 101);
            if (random > 40) counterattackMove = 3;
            else counterattackMove = 2;
        }
        else
        {
            int random = Random.Range(1, 101);
            if (random > 40) counterattackMove = 4;
            else counterattackMove = 1;
        }
    }
}
