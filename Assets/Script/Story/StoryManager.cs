using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    public static StoryManager System;

    [SerializeField] private Image actorOne, actorTwo, continueButton;
    [SerializeField] private TMP_Text bubbleText, actorNameText;
    [SerializeField] private List<Button> multipleChoiceButtons;
    [SerializeField] private GameObject choicesPanel, timer;
    [SerializeField] private Color active, nonActive;
    private Button currentSelectButton;
    
    private List<Actor> actors;
    
    private List<Sentence> currentStoryPart;
    private Sentence currentSentence;
    private bool multipleChoiceActive;

    /// <summary>
    /// This class is meant to be loaded when entering a story node.
    /// it will take the story part that would be next for the given actor and returns a JSON file that can
    /// be read
    /// </summary>
    /// <param name="actor"></param>
    public void GetNextStoryPart(Actor actor)
    {
        if (actor.Scenes.Count == 0)
        {
            Debug.Log("No more story parts for this actor");
            return;
        }
        currentStoryPart = actor.Scenes[actor.currentProgress].sentences;
        currentSentence = currentStoryPart[0];
        var sprite  = currentSentence.actorSprite ? currentSentence.actorSprite : actor.defaultImage.sprite;
        if (currentSentence.leftImage) {
            actorOne.GetComponent<Image>().sprite = sprite;
            actorOne.GetComponent<Image>().color = active;
            actorTwo.GetComponent<Image>().color = nonActive;
        } else {
            actorTwo.GetComponent<Image>().sprite = sprite;
            actorTwo.GetComponent<Image>().color = active;
            actorOne.GetComponent<Image>().color = nonActive;
        }
        bubbleText.text = currentSentence.text;
        actorNameText.text = currentSentence.actor.actorName;
    }


    private void GoToNextLine()
    {
        switch (currentSentence.choiceBranchIDs.Count)
        {
            case 0:
                Debug.Log("No more lines");
                return;
            case 1:
                currentSentence = currentStoryPart[currentSentence.choiceBranchIDs[0]];
                var sprite = currentSentence.actorSprite ? currentSentence.actorSprite : currentSentence.actor.defaultImage.sprite;
                if (currentSentence.leftImage) {
                    actorOne.GetComponent<Image>().sprite = sprite;
                    actorOne.GetComponent<Image>().color = active;
                    actorTwo.GetComponent<Image>().color = nonActive;
                } else {
                    actorTwo.GetComponent<Image>().sprite = sprite;
                    actorTwo.GetComponent<Image>().color = active;
                    actorOne.GetComponent<Image>().color = nonActive;
                }
                bubbleText.text = currentSentence.text;
                actorNameText.text = currentSentence.actor.actorName;
                break;
            default:
                multipleChoiceActive = true;
                choicesPanel.SetActive(true);
                //timer.SetActive(true);
                for (var i = 0; i < multipleChoiceButtons.Count; i++)
                {
                    if (i >= currentSentence.choiceBranchIDs.Count) continue;
                    multipleChoiceButtons[i].gameObject.SetActive(true);
                    multipleChoiceButtons[i].GetComponentInChildren<TMP_Text>().text =
                        currentStoryPart[currentSentence.choiceBranchIDs[i]].text;
                    multipleChoiceButtons[i].onClick.RemoveAllListeners();
                    var index = i;
                    multipleChoiceButtons[i].onClick.AddListener(() =>
                    {
                        currentSentence = currentStoryPart[currentSentence.choiceBranchIDs[index]];
                        multipleChoiceActive = false;
                        choicesPanel.SetActive(false);
                        GoToNextLine();
                        //timer.SetActive(false);
                    });
                }
                SetCurrentSelectButton(multipleChoiceButtons[0]);

                break;
        }
    }


    #region Input

    public void Submit()
    {
        if (currentSelectButton && multipleChoiceActive)
            currentSelectButton?.onClick?.Invoke();
        else
        {
            Debug.Log("here with the story");
            GoToNextLine();
        }
            
    }
    
    public void Navigate(Vector2 normalizedInput)
    {
        
        if (!currentSelectButton || normalizedInput == Vector2.zero) return;
        var isVertical = Mathf.Abs(normalizedInput.y) > Mathf.Abs(normalizedInput.x);
        Selectable selectable;
        if (isVertical)
        {
            selectable = normalizedInput.y > 0
                ? currentSelectButton.navigation.selectOnUp
                : currentSelectButton.navigation.selectOnDown;
        }
        else
        {
            selectable = normalizedInput.x > 0
                ? currentSelectButton.navigation.selectOnRight
                : currentSelectButton.navigation.selectOnLeft;
        }

        if (!selectable) return;
        SetCurrentSelectButton((Button)selectable);
    }

    private void SetCurrentSelectButton(Button button)
    {
        if (currentSelectButton && currentSelectButton.TryGetComponent(typeof(GameButton), out var component))
        {
            ( component as GameButton)?.OnDeselectEvent?.Invoke();
        }
        currentSelectButton = button;
        currentSelectButton?.Select();
        if (!currentSelectButton || !currentSelectButton.TryGetComponent(typeof(GameButton), out component)) return;
        {
            (component as GameButton)?.OnSelectEvent?.Invoke();
        }
    }

    #endregion
}


[System.Serializable]
public struct Sentence
{
    public int id; // Unique ID for this sentence
    public Actor actor;
    public AudioClip audio;
    [TextArea(3, 5)] public string text;
    public Sprite actorSprite;
    public bool leftImage;
    
    // Instead of storing the actual Sentence objects, 
    // store the IDs of the sentences this choice leads to.
    public List<int> choiceBranchIDs; 
}
