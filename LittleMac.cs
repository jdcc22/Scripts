using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using Unity.Burst.Intrinsics;

public class LittleMac : MonoBehaviour
{
    [SerializeField] Animator anim;
    BufferStates bufferMove;
    bool animationLock;

    const string leftHookName = "Hook Left";
    const string rightHookName = "Hook Right";
    const string leftJabName = "Jab Left";
    const string rightJabName = "Jab Right";
    const string leftDodgeName = "Dodge Left";
    const string rightDodgeName = "Dodge Right";
    const string duckName = "Duck";
    const string blockName = "Block Hit";
    const string starName = "Star Punch";
    const string getHitName = "Get Hit Left";
    const string entranceName = "Stage Entrance";
    const string stopFightingName = "Not Fighting";

    GameInputManager input;

    public int damageAmount;
    public int side;

    [SerializeField] int maxHealth;
    [ReadOnly] public int health;

    float damageTimer;
    bool isBlocking;
    bool isTired;

    [SerializeField] Image whiteHealthbar;
    [SerializeField] Image RedHealthbar;

    [AnimatorParam("anim")] public string blockParam;
    [AnimatorParam("anim")] public string tiredParam;

    FightManager fightManager;

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color tiredColor;

    public bool isDoingStarPunch { get; private set; }

    [SerializeField] bool invulnerable;

    bool pausedFight;

    bool knockedDown;

    bool getUpTime;
    float buttonPresses;
    int maxButtonPresses = 20;

    bool opponentDown;

    [SerializeField] AudioSource audioSource;

    [SerializeField] Transform getUpIndicator;
    Vector3 getUpPosition;

    public enum BufferStates
    {
        Left_Jab,
        Right_Jab,
        Left_Hook,
        Right_Hook,
        Left_Dodge,
        Right_Dodge,
        Duck,
        Star_Punch,
        Single_Star,
        None
    }

    // Start is called before the first frame update
    void Start()
    {
        pausedFight = true;
        animationLock = false;
        bufferMove = BufferStates.None;
        input = FindObjectOfType<GameInputManager>();
        input.Action += Input_Action;
        input.BlockState += Input_BlockState;

        getUpPosition = getUpIndicator.transform.position;

        health = maxHealth;
        fightManager = FindObjectOfType<FightManager>();

        fightManager.PauseFight += FightManager_PauseFight;
        fightManager.UnpauseFight += FightManager_UnpauseFight;
        fightManager.PlayerEntrance += FightManager_PlayerEntrance;
        fightManager.OpponentDown += FightManager_OpponentDown;
        fightManager.OpponentGotUp += FightManager_OpponentGotUp;
        fightManager.PlayerLost += FightManager_PlayerLost;
        fightManager.MacCelebration += FightManager_MacCelebration;
        fightManager.Candy += FightManager_Candy;
    }

    private void FightManager_Candy(object sender, System.EventArgs e)
    {
        health += 77;
        if (health > maxHealth) health = maxHealth;
    }

    private void FightManager_MacCelebration(object sender, System.EventArgs e)
    {
        anim.Play("Celebrate");
    }

    private void FightManager_PlayerLost(object sender, System.EventArgs e)
    {
        getUpTime = false;
        buttonPresses -= 0.1f * buttonPresses / maxButtonPresses;
    }

    private void FightManager_OpponentGotUp(object sender, System.EventArgs e)
    {
        opponentDown = false;
        pausedFight = false;
    }

    private void FightManager_OpponentDown(object sender, System.EventArgs e)
    {
        pausedFight = true;
        opponentDown = true;
    }

    private void FightManager_PlayerEntrance(object sender, System.EventArgs e)
    {
        anim.speed = 1;
        anim.Play(entranceName);
    }

    private void FightManager_UnpauseFight(object sender, System.EventArgs e)
    {
        pausedFight = false;
        UnlockAnimation();
    }

    private void FightManager_PauseFight(object sender, System.EventArgs e)
    {
        if (fightManager.currentRound < 4) Invoke("StopFighting", 2.5f);
        pausedFight = true;
        anim.speed = 0;
    }

    private void Input_BlockState(object sender, GameInputManager.blockContextEventArgs e)
    {
        
        isBlocking = e.isblocking;
    }

    private void Input_Action(object sender, GameInputManager.actionContextEventArgs e)
    {
        bufferMove = e.bufferState;
        bool punch = false;
        if (bufferMove == BufferStates.Left_Jab || bufferMove == BufferStates.Right_Jab || bufferMove == BufferStates.Left_Hook || bufferMove == BufferStates.Right_Hook) punch = true;

            if (getUpTime && punch)
            {
            buttonPresses++;
            if (buttonPresses >= maxButtonPresses)
            {
                GetUpFully();
            }
            }

            if (opponentDown && punch)
        {
            health += 1;
            if (health > maxHealth) health = maxHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {


        if (health <= 0) 
        { 
            health = 0;
            if (!knockedDown) GetKD();
        }
        RedHealthbar.fillAmount = (float)health / maxHealth;
        if (damageTimer <= 0)
        {
            if (whiteHealthbar.fillAmount > RedHealthbar.fillAmount) whiteHealthbar.fillAmount -= 0.5f * Time.deltaTime;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }

        if (pausedFight) return;

        if (!knockedDown)
        {

            if (isBlocking)
            {
                anim.SetFloat(blockParam, 1);
            }
            else
            {
                anim.SetFloat(blockParam, 0);
            }

            if (isTired)
            {
                anim.SetFloat(tiredParam, 1);
            }
            else
            {
                anim.SetFloat(tiredParam, 0);
            }

            if (fightManager.hearts == 0) isTired = true;
            else isTired = false;

            if (isTired)
            {
                spriteRenderer.color = tiredColor;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }


            MoveBuffer();
        }

        else
        {
            

            spriteRenderer.color = tiredColor;

            if (getUpTime)
            {
                buttonPresses -= maxButtonPresses * 0.2f * Time.deltaTime;
                if (buttonPresses < 0) buttonPresses = 0;
                transform.position = new Vector3(0, -0.35f + 1.2f * buttonPresses / maxButtonPresses, 0);
                getUpIndicator.transform.position = getUpPosition;
            }
        }
       
    }

    void MoveBuffer()
    {
        if (!animationLock)
        {
            switch (bufferMove)
            {
                case BufferStates.Left_Jab:
                    isDoingStarPunch = false;
                    if (isTired) break;
                    damageAmount = 4;
                    anim.Play(leftJabName);
                    side = -1;
                    break;
                case BufferStates.Right_Jab:
                    isDoingStarPunch = false;
                    if (isTired) break;
                    damageAmount = 4;
                    anim.Play(rightJabName);
                    side = 1;
                    break;
                case BufferStates.Left_Hook:
                    isDoingStarPunch = false;
                    if (isTired) break;
                    damageAmount = 7;
                    anim.Play(leftHookName);
                    side = -1;
                    break;
                case BufferStates.Right_Hook:
                    isDoingStarPunch = false;
                    if (isTired) break;
                    damageAmount = 7;
                    anim.Play(rightHookName);
                    side = 1;
                    break;
                case BufferStates.Left_Dodge:
                    anim.Play(leftDodgeName);

                    break;
                case BufferStates.Right_Dodge:
                    anim.Play(rightDodgeName);

                    break;
                case BufferStates.Duck:
                    anim.Play(duckName);

                    break;
                case BufferStates.Star_Punch:
                    side = 1;
                    isDoingStarPunch = true;
                    if (isTired) break;
                    if (fightManager.stars > 0)
                    {
                        switch (fightManager.stars)
                        {
                            case 1:
                                damageAmount = 25;
                                break;
                            case 2:
                                damageAmount = 55;
                                break;
                            case 3:
                                damageAmount = 90;
                                break;
                        }
                        fightManager.UseAllStars();
                        anim.Play(starName);
                    }
                    break;
                case BufferStates.Single_Star:
                    isDoingStarPunch = true;
                    side = 1;
                    if (fightManager.stars > 0)
                    {
                        damageAmount = 30;
                        fightManager.UseOneStar();
                        anim.Play(starName);
                    }
                    break;
            }

            bufferMove = BufferStates.None;
        }
    }

    public void LockAnimation()
    {
        animationLock = true;
    }

    public void UnlockAnimation()
    {
        animationLock = false;
    }

    public void Damage(int damage, bool ignoresBlock)
    {
        if (invulnerable) return;
        if (!ignoresBlock && isBlocking && !animationLock) 
        { 
            fightManager.hasBlockedLastHit = true;
            isBlocking = false;
            anim.Play(blockName);
            fightManager.DecreaseHearts(1);
        }
        else
        {
            health -= damage;
            damageTimer = 1f;
            fightManager.DecreaseHearts(3);
            fightManager.LoseAllStars();
            fightManager.hasBeenHit = true;
            anim.Play(getHitName);
            fightManager.AddPoints(-50);
        }
        
    }

    void StopFighting()
    {
        anim.Play(stopFightingName);
    }

    public void FinishedEntrance()
    {
        fightManager.PlayerEntranceFinish();
    }

    void GetKD()
    {
        fightManager.StopTimer();
        
        anim.Play("Get KD Left");
        knockedDown = true;

        fightManager.PlayerKD();
        
    }

    public void KDAnimationFinish()
    {
        getUpTime = true;
        buttonPresses = 0;
        switch(fightManager.macTotalKDs)
        {
            case 1:
                maxButtonPresses = 20;
                break;
            case 2:
                maxButtonPresses = 30;
                break;
            case 3:
                maxButtonPresses = 45;
                break;
            case 4:
                maxButtonPresses = 10000000;
                break;
        }
    }

    void GetUpFully()
    {
        fightManager.ResetHearts();
        fightManager.PlayerUp();
        health = Mathf.RoundToInt(maxHealth * 0.72f);
        getUpTime = false;
        Invoke("Unfreeze", 0.7f);
        transform.position = new Vector3(0, - 0.35f, 0);
        anim.Play("Idle");
    }

    void Unfreeze()
    {
        knockedDown = false;
        bufferMove = BufferStates.None;
        UnlockAnimation();
    }

    public void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    
}
