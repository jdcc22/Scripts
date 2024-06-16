using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NaughtyAttributes;


public class MainMenu : MonoBehaviour
{
    PlayerInputActions input;
    [Scene] public int fightScene;
    [SerializeField] AudioSource audioSource;

    bool controlMenuOn;
    [SerializeField] AudioClip controlAudio;
    [SerializeField] GameObject controlMenu;

    // Start is called before the first frame update
    void Start()
    {
        controlMenu.SetActive(false);
        input = new PlayerInputActions();
        input.Punchtime.Enable();
        input.Punchtime.BeginGame.performed += BeginGame_performed;
        Screen.SetResolution(1024, 896, true);
    }

    private void BeginGame_performed(InputAction.CallbackContext obj)
    {
        if (controlMenuOn)
        {
            Invoke("StartGame", 2f);
            audioSource.Play();
        }
        else
        {
            controlMenuOn = true;
            controlMenu.SetActive(true);
            audioSource.PlayOneShot(controlAudio);
        }
    }

    void StartGame()
    {
        input.Disable();
        SceneManager.LoadScene(fightScene);
    }

    
}
