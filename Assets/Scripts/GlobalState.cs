using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalState : MonoBehaviour
{
    public static Action onGameLoss;
    public static Action<float> addToScore;
    public static Action showEndScreen;

    [SerializeField] Transform transitionPanel;
    [SerializeField] GameObject endScreen;
    [SerializeField] Text scoreText;

    [SerializeField] float transitionTime;

    float score;

    public void QuitGame()
    {
        Application.Quit();
    }

    void ResetLevel()
    {
        addToScore -= AddToScore;
        onGameLoss -= OnGameLoss;
        showEndScreen -= ShowEndScreen;
        SceneManager.LoadScene(0);
        
        Image image = transitionPanel.GetComponent<Image>();
        Color newColour = image.color;
        newColour.a = 1f;
        image.color = newColour;
    }

    void OnGameLoss()
    {
        StartCoroutine(Transition(ResetLevel, false));
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
        Image image = transitionPanel.GetComponent<Image>();
        yield return FadeTransitionPanel(image, 1f);
        yield return new WaitForSeconds(0.5f);
        inbetween();
        if (fadeOut) yield return FadeTransitionPanel(image, 0f);
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
        onGameLoss += OnGameLoss;
        showEndScreen += ShowEndScreen;

        endScreen.SetActive(false);
        Image image = transitionPanel.GetComponent<Image>();
        StartCoroutine(FadeTransitionPanel(image, 0f));
    }
}
