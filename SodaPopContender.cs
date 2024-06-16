using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Microsoft.Win32.SafeHandles;

public class SodaPopContender : MonoBehaviour, EnemyHurtbox.IFighter
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
    int maxUppercuts;
    int uppersMade;
    bool rage;
    [SerializeField] float hookTimer;
    int hookState; // 0 = None, 1 = LH, 2 = RH, 3 = LJ, 4 = RJ, 5 = SP
    [SerializeField] int starsNeededtoKD = 1;

    int getUpAtCount;

    [SerializeField, ReadOnly] float interval;
    [SerializeField] float minInterval;
    [SerializeField] float maxInterval;

    float speed = 0.72f;


    [SerializeField] Animator anim;
    [AnimatorParam("anim")] public int animStunLock;
    [AnimatorParam("anim")] public int animRage;
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
    const string rageRightName = "Rage Right Upper";
    const string rageLeftName = "Rage Left Upper";

    FightManager fightManager;

    [SerializeField] bool pausedFight;

    int timesOpponentBeenHit;

    public bool taunt;
    

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

        

        anim.SetFloat("Attack Speed", speed);
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
        if (minInterval > 1.6f)
        {
            minInterval -= 0.125f;
        }
        maxInterval -= 0.55f;
        if (maxInterval < minInterval) maxInterval = minInterval;

        SpeedIncrease(0.1f);
    }

    private void SpeedIncrease(float increment)
    {
        speed += increment;
        if (speed > 1.25f) speed = 1.25f;
        anim.SetFloat("Attack Speed", speed);
    }

    private void FightManager_PlayerDown(object sender, System.EventArgs e)
    {
        pausedFight = true;
        rage = false;
        anim.SetBool(animRage, false);

        
    }

    private void FightManager_UnpauseFight(object sender, System.EventArgs e)
    {
        pausedFight = false;
        anim.speed = 1;
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

        anim.SetBool(animRage, false);
        rage = false;
        uppersMade = 0;

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

        

        if (hookTimer > 0) hookTimer -= Time.deltaTime;
        else
        {
            hookState = 0;
        }

        if (interval > 0 && !stunned && !comboLock && !knockedDown && !rage) interval -= Time.deltaTime;
        else if (!stunned && !comboLock && !knockedDown && !rage)
        { 

            Attack();
            ResetHit();

            interval = Random.Range(minInterval, maxInterval);
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

    void Attack()
    {
        switch (phase)
        {
            case 1:
                switch (patternNumber)
                {
                    case 0:
                        anim.Play(rightHookName);
                        patternNumber++;
                        break;
                    case 1:
                        anim.Play(rightHookName);
                        patternNumber++;
                        break;
                    case 2:
                        anim.Play(leftUpperName);
                        patternNumber++;
                        break;
                    case 3:
                        anim.Play(rightUpperName);
                        patternNumber++;
                        break;
                    case 4:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 5:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 6:
                        anim.Play(leftUpperName);
                        if (timesOpponentBeenHit < 1)
                        {
                            patternNumber = 0;
                        }
                        else
                        {
                            patternNumber++;
                            timesOpponentBeenHit = 0;
                        }
                        break;
                    case 7:
                        anim.Play("Taunt");
                        patternNumber = 0;
                        break;
                }
                break;
            case 2:
                switch (patternNumber)
                {
                    case 0:
                        anim.Play(rightHookName);
                        patternNumber++;
                        break;
                    case 1:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 2:
                        anim.Play(rightUpperName);
                        patternNumber++;
                        break;
                    case 3:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 4:
                        anim.Play(rightHookName);
                        patternNumber++;
                        break;
                    case 5:
                        anim.Play(leftUpperName);
                        patternNumber++;
                        break;
                    case 6:
                        anim.Play(rightUpperName);
                        if (timesOpponentBeenHit < 2)
                        {
                            patternNumber = 0;
                        }
                        else
                        {
                            patternNumber++;
                            timesOpponentBeenHit = 0;
                        }
                        break;
                    case 7:
                        anim.Play("Taunt");
                        patternNumber = 0;
                        break;
                }
                break;
            case 3:
                switch (patternNumber)
                {
                    case 0:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 1:
                        anim.Play(jabName);
                        patternNumber++;
                        break;
                    case 2:
                        anim.Play(leftUpperName);
                        patternNumber++;
                        break;
                    case 3:
                        anim.Play(rightUpperName);
                        patternNumber++;
                        break;
                    case 4:
                        anim.Play(rightHookName);
                        patternNumber++;
                        break;
                    case 5:
                        anim.Play(rightUpperName);
                        patternNumber++;
                        break;
                    case 6:
                        anim.Play(rightHookName);
                        if (timesOpponentBeenHit < 3)
                        {
                            patternNumber = 0;
                        }
                        else
                        {
                            patternNumber++;
                            timesOpponentBeenHit = 0;
                        }
                        break;
                    case 7:
                        anim.Play("Taunt");
                        patternNumber = 0;
                        break;
                }
                break;
        
        
        }
    }

    void GetUp()
    {
        fightManager.OpponentUp();
        knockedDown = false;
        anim.SetBool(animKnockdown, false);
        health = (int)(maxHealth * Random.Range(0.3f, 0.85f));
        interval = maxInterval;
        if (phase != 3) phase++;
        rage = true;
        anim.SetBool(animRage, true);
        patternNumber = 0;
        maxUppercuts += 3;
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
                        //Instant Knockdowns

                        //Last combo hit is a Star Punch (Stars required to KD increase after each use) 
                        if (comboHits + 1 == maxComboHits && fightManager.lastStarsUsed >= starsNeededtoKD)
                        {
                            damage = 9999;
                            if (starsNeededtoKD < 3) starsNeededtoKD++;
                            fightManager.AddPoints(350);
                        }

                        // Hook into Jab into Star Knockdown -- Part 3: Star Punch (Stars required to KD increase after each use)
                        if ((hookState == 3 || hookState == 4) && fightManager.lastStarsUsed >= starsNeededtoKD)
                        {
                            damage = 9999;
                            if (starsNeededtoKD < 3) starsNeededtoKD++;
                            fightManager.AddPoints(450);
                        }

                        EndCombo();
                        
                    }
                    

                    health -= damage;

                    //Star Opportunity for Hook, only useful as a hook counter
                    if (starOpportunity && side == 1)
                    {
                        fightManager.EarnStar();
                        fightManager.AddPoints(100);
                    }
                    
                    if (health < 0)
                    {
                        Knockdown();
                        fightManager.AddPoints(1000);
                    }

                    if (taunt)
                    {
                        taunt = false;
                        audioSource.Stop();
                        fightManager.EarnStar();
                        fightManager.AddPoints(125);
                    }

                    //Setup for Hook into Star Knockdown -- Part 2: Jab
                    if (side == 1 && hookState == 1) { hookState = 4; hookTimer = 1f; }
                    else if (hookState == 2 && side == -1) { hookState = 3; hookTimer = 1f; }
                    else hookTimer = 0;

                    
                    
                    
                }
                break;
            case EnemyHurtbox.DamageArea.Body:
                anim.SetFloat (animBodyLocation, -1);
                if (bodyState == AreaValues.Open) 
                {
                    anim.SetFloat(animSide, side);
                    // Earn a Star if it's the last hit in the combo
                    if (comboHits + 1 == maxComboHits)
                    {
                        fightManager.EarnStar();
                        fightManager.AddPoints(100);
                    }

                    //Hooks always break combos
                    EndCombo();
                    anim.Play(getHitName);
                    fightManager.AddPoints(30);

                    comboTimer = 1.2f;

                    health -= damage;

                    if (health < 0)
                    {
                        Knockdown();
                        fightManager.AddPoints(1000);
                    }

                    // Hook Stun means a hook right after dodging Soda's hook, which awards a star
                    if (hookStun)
                    {
                        fightManager.EarnStar();
                        fightManager.AddPoints(100);
                    }

                    if (taunt)
                    {
                        taunt = false;
                        audioSource.Stop();
                        fightManager.EarnStar();
                        fightManager.AddPoints(125);
                    }

                    if (side == 1) hookState = 2;
                    else hookState = 1;
                    hookTimer = 1f;
                }
                else if (bodyState == AreaValues.Block)
                {
                    anim.SetFloat(animSide, side);
                    anim.Play("Block");
                    fightManager.DecreaseHearts(1);
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
            maxComboHits = 2;

            timesOpponentBeenHit++;
            
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

    public void RageUpper()
    {
        if (!rage) return;
        uppersMade++;
        if (uppersMade >= maxUppercuts)
        {
            anim.SetBool(animRage, false);
            rage = false;
            uppersMade = 0;
        }

        maxComboHits++;
    }

    public void GotUp()
    {
        fightManager.RestartTimer();
        if (minInterval > 1.6f)
        {
            minInterval -= 0.125f;
        }
        maxInterval -= 0.55f;
        if (maxInterval < minInterval) maxInterval = minInterval;

        SpeedIncrease(0.22f);

        int random = Random.Range(0, 100);
        if (random%2 == 0)
        {
            anim.Play(rageLeftName);
        }
        else
        {
            anim.Play(rageRightName);
        }
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
        maxComboHits++;
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
        anim.speed = 1;
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
                getUpAtCount = Random.Range(1, 11);
                if (getUpAtCount >= 7) getUpAtCount = 25;
                else getUpAtCount = 9;
                break;
            case 5:
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
}
