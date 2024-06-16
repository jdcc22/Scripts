using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

public class FightManager : MonoBehaviour
{
    public int stars { get; private set; }
    public int macTotalKDs { get; private set; }
    public int macRoundKDs { get; private set; }
    public int opponentTotalKDs { get; private set; }
    public int opponentRoundKDs { get; private set; }
    float totalTime;
    float maxTime = 180;
    int minutes;
    int seconds;
    bool slowedTimer;
    bool stoppedTimer;

    public int currentRound { get; private set; }


    public int hearts { get; private set; }
    [SerializeField] int maxHearts;
    [SerializeField] float timeMultiplier;
    [SerializeField] TextMeshProUGUI heartsText;
    [SerializeField] TextMeshProUGUI minuteText;
    [SerializeField] TextMeshProUGUI secondText;
    [SerializeField] GameObject star1;
    [SerializeField] GameObject star2;
    [SerializeField] GameObject star3;

    [SerializeField] GameObject playerX1;
    [SerializeField] GameObject playerX2;
    [SerializeField] GameObject playerX3;
    [SerializeField] GameObject opponentX1;
    [SerializeField] GameObject opponentX2;
    [SerializeField] GameObject opponentX3;

    [SerializeField] IntermissionUI intermissionUI;

    [SerializeField] AudioSource ding;
    [SerializeField] AudioSource fightMusic;
    [SerializeField] AudioSource opponentDownMusic;
    [SerializeField] AudioSource playerDownMusic;
    [SerializeField] AudioSource preMatchMusic;
    [SerializeField] AudioSource hudAudio;
    [SerializeField] AudioClip starOneClip;
    [SerializeField] AudioClip starTwoClip;
    [SerializeField] AudioClip starThreeClip;
    [SerializeField] AudioClip heartLossClip;
    [SerializeField] AudioClip refreshClip;


    public bool hasBeenHit;
    public bool hasBlockedLastHit;
    public int lastStarsUsed { get; private set; }

    public event EventHandler PauseFight;
    public event EventHandler UnpauseFight;
    public event EventHandler OpponentEntrance;
    public event EventHandler PlayerEntrance;

    public event EventHandler<CountDownEventArgs> Countdown;


    bool roundOver;

    bool countdown;
    float countdownTimer;

    bool macDown;
    bool opponentDown;

    int pointTotal;
    [SerializeField] TextMeshProUGUI pointText;
    [SerializeField] int pointThreshold;



    int countNumber;

    [SerializeField] Animator UIanim;
    [AnimatorParam("UIanim")] public int animStar;
    [AnimatorParam("UIanim")] public int animTired;

    [SerializeField] Animator marioAnim;
    [AnimatorParam("marioAnim")] public int countAnim;
    [AnimatorParam("marioAnim")] public int tkoTrigger;
    [AnimatorParam("marioAnim")] public int decide;
    [SerializeField] bool soda;

    [Scene] public int mainMenu;
    [Scene] public int sodaFight;

    GameInputManager input;

    bool candyUsed;
    [SerializeField] AudioSource candyAudio;

    [SerializeField] Image UIOverlay;
    [SerializeField] Sprite round1;
    [SerializeField] Sprite round2;
    [SerializeField] Sprite round3;


    public class ResearchDataSave
    {
        public int wins = 0;
        public int losses = 0;
        public int KOs = 0;
        public int TKOs = 0;

        public int round1Wins = 0;
        public int round2Wins = 0;
        public int round3Wins = 0;
        public int decisionWins = 0;
        public int round1Losses = 0;
        public int round2Losses = 0;
        public int round3Losses = 0;

        public int starsEarned = 0;
        public int starsUsed = 0;

        public int sodaFights = 0;
        public int sodaWinRecord = 33;
        public int sodaLossRecord = 2;
        public int sodaKORecord = 25;
        public int sodaWins = 0;
        public int sodaLosses = 0;
        public int sodaKOs = 0;

        public int donWinRecord = 22;
        public int donLossRecord = 3;
        public int donKORecord = 4;
        public int donWins = 0;
        public int donLosses = 0;
        public int donKOs = 0;
    }

    public static ResearchDataSave saveData;

    public class CountDownEventArgs : EventArgs
    {
        public int count;
    }

    private void Awake()
    {
        LoadData();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentRound = 1;
        stars = 0;
        totalTime = 0;
        minutes = 3;
        seconds = 0;
        stoppedTimer = true;

        playerX1.SetActive(false);
        playerX2.SetActive(false);
        playerX3.SetActive(false);

        opponentX1.SetActive(false);
        opponentX2.SetActive(false);
        opponentX3.SetActive(false);

        ResetHearts();
        minuteText.text = minutes.ToString();
        secondText.text = "0" + seconds.ToString();

        input = FindObjectOfType<GameInputManager>();

        input.Candybar += Input_Candybar;


    }

    public event EventHandler Candy;
    private void Input_Candybar(object sender, EventArgs e)
    {
        if (roundOver && !candyUsed)
        {
            candyUsed = true;
            candyAudio.Play();
            Candy?.Invoke(this, null);
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (!stoppedTimer) TimerHandling();




        if (Input.GetKeyDown(KeyCode.P)) { SaveData(); }
        if (Input.GetKeyDown(KeyCode.L)) { LoadData(); }






        if (countdown)
        {
            if (countdownTimer > 0) countdownTimer -= Time.deltaTime;
            else
            {
                if (countNumber < 10)
                {

                    countNumber++;
                    marioAnim.SetFloat(countAnim, countNumber);
                    marioAnim.Play("Count");
                    Countdown?.Invoke(this, new CountDownEventArgs() { count = countNumber });
                    countdownTimer = 1.1f;
                }
                else
                {
                    countdown = false;
                    if (opponentDown) WinFight(true);
                    else LoseFight(true);
                }
            }
        }


    }

    void TimerHandling()
    {
        if (!slowedTimer) totalTime += Time.deltaTime * timeMultiplier;
        else totalTime += Time.deltaTime;

        if (totalTime >= maxTime) totalTime = maxTime;

        float remainingTime = maxTime - totalTime;

        if (remainingTime <= 0 && !roundOver)
        {
            RoundEnd();
        }

        minutes = (int)(remainingTime / 60f);
        seconds = (int)(remainingTime - minutes * 60f);

        minuteText.text = minutes.ToString();



        if (seconds / 10 >= 1)
        {
            secondText.text = seconds.ToString();
        }
        else
        {
            secondText.text = "0" + seconds.ToString();
        }
    }

    public void UseAllStars()
    {
        lastStarsUsed = stars;
        saveData.starsUsed += lastStarsUsed;
        stars = 0;
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
    }

    public void LoseAllStars()
    {
        if (stars > 0) UIanim.Play("Lose Stars");
        stars = 0;
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

    }

    public void UseOneStar()
    {
        lastStarsUsed = 1;
        saveData.starsUsed++;
        stars--;
        switch (stars)
        {
            case 0:
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
                break;
            case 1:
                star1.SetActive(true);
                star2.SetActive(false);
                star3.SetActive(false);
                break;
            case 2:
                star1.SetActive(false);
                star2.SetActive(true);
                star3.SetActive(false);
                break;
        }
    }

    public void SlowTimer()
    {
        slowedTimer = true;
    }

    public void RestartTimer()
    {
        stoppedTimer = false;
        slowedTimer = false;
    }

    public void StopTimer()
    {
        stoppedTimer = true;
    }


    public event EventHandler OpponentDown;
    public void OpponentKD()
    {
        fightMusic.Stop();
        opponentDownMusic.Play();
        OpponentDown?.Invoke(this, null);
        marioAnim.Play("Show Up");
        opponentDown = true;
        opponentTotalKDs++;
        opponentRoundKDs++;

        if (opponentRoundKDs > 0) opponentX1.SetActive(true);
        if (opponentRoundKDs > 1) opponentX2.SetActive(true);
        if (opponentRoundKDs > 2) opponentX3.SetActive(true);

        if (opponentRoundKDs < 3)
        {
            countdown = true;
            countdownTimer = 3f;
        }
        else
        {
            WinFight(false);
        }


    }


    public event EventHandler PlayerDown;
    public void PlayerKD()
    {

        fightMusic.Stop();
        playerDownMusic.Play();
        PlayerDown?.Invoke(this, null);
        marioAnim.Play("Show Up");
        macDown = true;
        macTotalKDs++;
        macRoundKDs++;

        if (macRoundKDs > 0) playerX1.SetActive(true);
        if (macRoundKDs > 1) playerX2.SetActive(true);
        if (macRoundKDs > 2) playerX3.SetActive(true);

        if (macRoundKDs < 3)
        {
            countdown = true;
            countdownTimer = 3f;
        }
        else
        {
            LoseFight(false);
        }


    }

    public event EventHandler OpponentGotUp;
    public void OpponentUp()
    {
        opponentDown = false;

        opponentDownMusic.Stop();
        marioAnim.Play("Fight");
        fightMusic.Play();
        OpponentGotUp?.Invoke(this, null);
        countdown = false;
        countNumber = 0;
    }

    public event EventHandler PlayerGotUp;
    public void PlayerUp()
    {
        

        playerDownMusic.Stop();
        marioAnim.Play("Fight");
        fightMusic.Play();
        PlayerGotUp?.Invoke(this, null);
        countdown = false;
        RestartTimer();
        countNumber = 0;
    }

    public void DecreaseHearts(int heartsToDecrease)
    {
        hearts -= heartsToDecrease;
        if (hearts <= 0) 
        {
            hearts = 0;
            UIanim.SetFloat(animTired, 1);
            hudAudio.PlayOneShot(heartLossClip);
        }
        heartsText.text = hearts.ToString();
        
    }

    public void ResetHearts()
    {
        UIanim.SetFloat(animTired, 0);
        hearts = maxHearts;
        heartsText.text = hearts.ToString();
        if (!stoppedTimer) hudAudio.PlayOneShot(refreshClip);
    }

    public void EarnStar()
    {
        saveData.starsEarned++;
        stars++;
        UIanim.SetFloat(animStar, stars);
        UIanim.Play("Earn Star");

        switch (stars)
        {
            case 0:
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(false);
                break;
            case 1:
                hudAudio.PlayOneShot(starOneClip);
                star1.SetActive(true);
                star2.SetActive(false);
                star3.SetActive(false);
                break;
            case 2:
                hudAudio.PlayOneShot(starTwoClip);
                star1.SetActive(false);
                star2.SetActive(true);
                star3.SetActive(false);
                break;
            case 3:
                hudAudio.PlayOneShot(starThreeClip);
                star1.SetActive(false);
                star2.SetActive(false);
                star3.SetActive(true);
                break;
        }

       

    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(saveData);

        File.WriteAllText(Application.dataPath + "/researchData.twr", json);

        Debug.Log("Saved data");
    }

    void LoadData()
    {
        if (File.Exists(Application.dataPath + "/researchData.twr"))
        {
            string json = File.ReadAllText(Application.dataPath + "/researchData.twr");

            saveData = JsonUtility.FromJson<ResearchDataSave>(json);

            Debug.Log(saveData);
        }
        else
        {
            saveData = new ResearchDataSave();
        }
    }

    [ContextMenu("Remove Data")]
    private void DeleteData()
    {
        ResearchDataSave freshData = new ResearchDataSave();
        string json = JsonUtility.ToJson(freshData);

        File.WriteAllText(Application.dataPath + "/researchData.twr", json);

        Debug.Log("Removed Data Successfully");
    }

    void RoundEnd()
    {
        macRoundKDs = 0;
        opponentRoundKDs = 0;
        
        currentRound++;
        PauseFight?.Invoke(this, null);
        Invoke("DingNoise", 0.2f);
        stoppedTimer = true;
        totalTime = 0;
        roundOver = true;
        if (currentRound <= 3)
        {
            Invoke("EnableIntermission", 0.3f);
        }
        else
        {
            intermissionUI.QuickFade();
            Invoke("FinishDecision", 2f);
        }

        

    }

    public void FinishDecision()
    {
        if (pointTotal > pointThreshold)
        {
            marioAnim.SetFloat(decide, 1);
            Invoke("WinByDecision", 3f);
        }
        else
        {
            marioAnim.SetFloat(decide, 2);
            Invoke("LoseByDecision", 3f);
        }

        marioAnim.Play("Show Up");

        
    }


    void DingNoise()
    {
        // Stops all noises before dinging
        fightMusic.Stop();
        playerDownMusic.Stop();
        opponentDownMusic.Stop();


        ding.Play();
    }

    void EnableIntermission()
    {
        switch (currentRound)
        {
            case 1:
                UIOverlay.sprite = round1;
                break;
            case 2:
                UIOverlay.sprite = round2;
                break;
            case 3:
                UIOverlay.sprite = round3;
                break;
        }

        intermissionUI.Reactivate();
        Invoke("MarioSpawn", 2f);
    }

    void MarioSpawn()
    {
        marioAnim.Play("Onscreen");

        playerX1.SetActive(false);
        playerX2.SetActive(false);
        playerX3.SetActive(false);

        opponentX1.SetActive(false);
        opponentX2.SetActive(false);
        opponentX3.SetActive(false);
    }

    public void RoundRestart()
    {
        roundOver = false;
        Invoke("StartEntrances", 1f);

    }

    void StartEntrances()
    {
        preMatchMusic.Play();
        OpponentEntrance?.Invoke(this, null);
        marioAnim.Play("Show Up");
    }
    

    public void OpponentEntranceFinish()
    {
        PlayerEntrance?.Invoke(this, null);
        marioAnim.Play("Fight");
    }

    public void PlayerEntranceFinish()
    {
        UnpauseFight?.Invoke(this, null);
        fightMusic.Play();
        stoppedTimer = false;
        roundOver = false;
    }

    public event EventHandler MacCelebration;

    void MacCelebrate()
    {
        MacCelebration?.Invoke(this, null);
    }

    public event EventHandler OpponentCelebration;

    void OpponentCelebrate()
    {
        OpponentCelebration?.Invoke(this, null);
    }

    public void WinFight(bool KO)
    {
        
        if (KO)
        {
            marioAnim.Play("KO");
        }
        else
        { 
            marioAnim.SetBool(tkoTrigger, true);
        }

        Invoke("MacCelebrate", 2f);

        saveData.wins++;
        if (KO) saveData.KOs++;
        else saveData.TKOs++;

        if (soda)
        {
            saveData.sodaLosses++;
            saveData.sodaLossRecord++;
        }
        else
        {
            saveData.donLosses++;
            saveData.donLossRecord++;
        }

        SaveData();

        if (soda) Invoke("ReturnToMenu", 7f);
        else Invoke("ProceedToSoda", 7f);
    }

    public event EventHandler PlayerLost;

    public void LoseFight(bool KO)
    {
        PlayerLost?.Invoke(this, null);

        Invoke("OpponentCelebrate", 2f);

        if (KO)
        {
            marioAnim.Play("KO");
        }
        else
        {
            marioAnim.SetBool(tkoTrigger, true);
        }

        saveData.losses++;

        if (currentRound == 1) saveData.round1Losses++;
        if (currentRound == 2) saveData.round2Losses++;
        if (currentRound == 3) saveData.round3Losses++;

        if (soda)
        {
            saveData.sodaWins++;
            saveData.sodaKOs++;
            saveData.sodaWinRecord++;
            saveData.sodaKORecord++;
        }
        else
        {
            saveData.donKOs++;
            saveData.donWins++;
            saveData.donWinRecord++;
            saveData.donKORecord++;
        }

        SaveData();


        Invoke("ReturnToMenu", 7f);

    }

    void WinByDecision()
    {
        if (soda) Invoke("ReturnToMenu", 7f);
        else Invoke("ProceedToSoda", 7f);
        saveData.wins++;
        Invoke("MacCelebrate", 1f);

        saveData.decisionWins++;

        if (soda)
        {
            saveData.sodaLosses++;
            saveData.sodaLossRecord++;
        }
        else
        {
            saveData.donLosses++;
            saveData.donLossRecord++;
        }

        SaveData();
    }

    void LoseByDecision()
    {
        Invoke("ReturnToMenu", 7f);

        saveData.losses++;

        Invoke("OpponentCelebrate", 1f);
        

        if (soda)
        {
            saveData.sodaWins++;
            saveData.sodaWinRecord++;
        }
        else
        {
            saveData.donWins++;
            saveData.donWinRecord++;
        }

        SaveData();
    }

    public void AddPoints(int score)
    {
        pointTotal += score;
        int pointsTenth = Mathf.RoundToInt(pointTotal / 10);

        pointText.text = pointsTenth.ToString();
    }

    void ReturnToMenu()
    {
        input.Destroy();
        SceneManager.LoadScene(mainMenu);
    }

    void ProceedToSoda()
    {
        input.Destroy();
        SceneManager.LoadScene(sodaFight);
    }

}
