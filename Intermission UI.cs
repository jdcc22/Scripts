using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class IntermissionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI docTip;
    [SerializeField] TextMeshProUGUI opponentRemark;
    [SerializeField] TextMeshProUGUI macRecord;
    [SerializeField] TextMeshProUGUI opponentRecord;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource crowdCheer;
    [SerializeField] Animator anim;
    [AnimatorParam("anim")] public int roundIndex;
    [AnimatorParam("anim")] public int animSoda;
    [AnimatorParam("anim")] public int endTrigger;
    [SerializeField] bool soda;

    bool canProgress = true;

    GameInputManager input;
    FightManager fightManager;

    

    // Start is called before the first frame update
    void Start()
    {
        if (soda) anim.SetFloat(animSoda, 1);
        else anim.SetFloat(animSoda, 2);
        input = FindObjectOfType<GameInputManager>();
        fightManager = FindObjectOfType<FightManager>();
        input.PausePerformed += Input_PausePerformed;
        anim.SetFloat(roundIndex, 1);

        

        macRecord.text = FightManager.saveData.wins.ToString() + "-" + FightManager.saveData.losses.ToString() + " " + FightManager.saveData.KOs.ToString() + " KO";

        if (soda)
        {
            opponentRecord.text = FightManager.saveData.sodaWinRecord.ToString() + "-" + FightManager.saveData.sodaLossRecord.ToString() + " " + FightManager.saveData.sodaKORecord.ToString() + " KO";
        }
        else
        {
            opponentRecord.text = FightManager.saveData.donWinRecord.ToString() + "-" + FightManager.saveData.donLossRecord.ToString() + " " + FightManager.saveData.donKORecord.ToString() + " KO";
        }

        docTip.text = "";
        if (soda) opponentRemark.text = "Informacion\r\n\r\nOrigen: Moscu, URSS\r\n\r\ntalla: 198cm\r\n\r\nPeso: 108kg\r\n\r\nRango: #2";
        else opponentRemark.text = "Informacion\r\n\r\nOrigen: Madrid, ESP\r\n\r\ntalla: 185cm\r\n\r\nPeso: 69kg\r\n\r\nRango: Campeon";



    }

    public void Reactivate()
    {
        anim.SetFloat(roundIndex, fightManager.currentRound);
        anim.SetTrigger(endTrigger);
        SetUpNextExchange();
    }

    public void QuickFade()
    {
        anim.Play("Quick Fade");
    }

    private void Input_PausePerformed(object sender, System.EventArgs e)
    {
        if (canProgress)
        {
            canProgress = false;
            anim.SetTrigger(endTrigger);
            
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RestartFight()
    {
        fightManager.RoundRestart();
    }

    public void LoadFinish()
    {
        canProgress = true;

    }

    public void PlayAudio(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    public void CheerStart()
    {
        crowdCheer.Play();
    }

    public void CheerEnd()
    {
        crowdCheer.Stop();
    }


    public void SetUpNextExchange()
    {
        int random = Random.Range(0, 100);


        
         switch (fightManager.currentRound)
         {
             case 2:
                if (soda) 
                {
                     docTip.text = "Golpea su cabeza cuando vaya a hacer un gancho para una estrella!";
                     opponentRemark.text = "Quien es este chiquitin? Donde esta el oponente de verdad?";
                }
                else
                {
                    docTip.text = "Esquiva sus ataques y luego muestrale un verdadero contraataque!";
                    opponentRemark.text = "Me llaman Don, Don Flamenco!";
                }
                 break;
             case 3:
                if (soda) 
                {
                    docTip.text = "Dale al higado y luego su cabeza y acaba con una estrella!";
                    opponentRemark.text = "Despues de acabarte voy a beber por mi salud! ja ja ja!!!";
                }
                else
                {
                    docTip.text = "No seas tan imprudente Mac, despues de un ataque esquiva su contraataque";
                    opponentRemark.text = "No te parece que huelo a victoria?!";
                }
                break;
                
            }
        
        
    }
}
