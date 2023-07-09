using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalState : MonoBehaviour
{
    const int menuSceneIndex = 0;
    const int levelSceneIndex = 1;

    public static Action onGameLoss;
    public static Action<float> addToScore;
    public static Action showEndScreen;

    //NOTE: Needs to be set in both scenes
    [SerializeField] Transform transitionPanel;
    
    //NOTE: Needs to be set in dungeon scene 
    [SerializeField] GameObject endScreen;
    [SerializeField] Text scoreText;

    //NOTE: Needs to be set in menu scene
    [SerializeField] GameObject howToPlayScreen;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject highlightDemo;
    [SerializeField] Text title;

    [SerializeField] float transitionTime;

    float score;

    public void QuitGame()
    {
        Debug.Log("poop");
        Application.Quit();
    }

    void ResetLevel()
    {
        addToScore -= AddToScore;
        onGameLoss -= FadeOutAndLoadLevel;
        showEndScreen -= ShowEndScreen;
        SceneManager.LoadScene(levelSceneIndex);
        
        //Image image = transitionPanel.GetComponent<Image>();
        //Color newColour = image.color;
        //newColour.a = 1f;
        //image.color = newColour;
    }

    void ResetMenu()
    {
        addToScore -= AddToScore;
        onGameLoss -= FadeOutAndLoadLevel;
        showEndScreen -= ShowEndScreen;
        SceneManager.LoadScene(menuSceneIndex);
        
        //Image image = transitionPanel.GetComponent<Image>();
        //Color newColour = image.color;
        //newColour.a = 1f;
        //image.color = newColour;
    }

    public void FadeOutAndLoadMenu()
    {
        StartCoroutine(Transition(ResetMenu, false));
    }

    public void FadeOutAndLoadLevel()
    {
        StartCoroutine(Transition(ResetLevel, false));
    }

    public void ShowHowToPlay()
    {
        title.text = "How to Play";
        mainMenu.SetActive(false);
        howToPlayScreen.SetActive(true);
        highlightDemo.SetActive(true);
    }

    public void ShowMainMenu()
    {
        title.text = "Mimic's Dungeon";
        howToPlayScreen.SetActive(false);
        highlightDemo.SetActive(false);
        mainMenu.SetActive(true);
    }

    void AddToScore(float addedScore)
    {
        score += addedScore;
    }

    void LoadEndScreen()
    {
        scoreText.text = score.ToString();
        endScreen.SetActive(true);
    }

    void ShowEndScreen()
    {
        StartCoroutine(Transition(LoadEndScreen));
    }

    IEnumerator Transition(Action inbetween, bool fadeOut = true)
    {
        transitionPanel.gameObject.SetActive(true);
        Image image = transitionPanel.GetComponent<Image>();
        yield return FadeTransitionPanel(image, 1f);
        yield return new WaitForSeconds(0.5f);
        inbetween();
        if (fadeOut) 
        {
            yield return FadeTransitionPanel(image, 0f);
            transitionPanel.gameObject.SetActive(false);
        }
    }

    IEnumerator StartTransition()
    {
        transitionPanel.gameObject.SetActive(true);
        Image image = transitionPanel.GetComponent<Image>();
        yield return FadeTransitionPanel(image, 0f);
        transitionPanel.gameObject.SetActive(false);
    }

    float elapsedTime = 0f;
    IEnumerator FadeTransitionPanel(Image image, float endAlpha)
    {
        Color newColour = image.color;

        while (elapsedTime < transitionTime)
        {
            newColour.a = Mathf.Lerp(image.color.a, endAlpha, elapsedTime / transitionTime);
            image.color = newColour;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        newColour.a = endAlpha;
        image.color = newColour;
        elapsedTime = 0f;
    }


    void Awake()
    {
        addToScore += AddToScore;
        onGameLoss += FadeOutAndLoadLevel;
        showEndScreen += ShowEndScreen;

        //endScreen.SetActive(false);
        StartCoroutine(StartTransition());
    }
}
