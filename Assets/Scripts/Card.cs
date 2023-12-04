using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardStatus
{
    show_back,
    show_front,
    rotating_to_back,
    rotating_to_front
}

public class Card : MonoBehaviour
{
    /* ------------ variables ------------ */

    // reference to game script
    private Game game;

    // sprite renderers
    private SpriteRenderer frontRenderer;
    private SpriteRenderer backRenderer;

    // card status
    [SerializeField]
    private CardStatus status;
    private CardStatus previousGameState;

    // turning variables
    [SerializeField]
    private float turnTargetTime;
    private float turnTimer;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private void Awake()
    {
        game = FindObjectOfType<Game>();

        status = CardStatus.show_back;
        previousGameState = status;

        GetFrontAndBackSpriteRenderers();
    }

    private void Update()
    {
        // check if the previous game state was rotating and now is front.
        // then add to a variable in the game script. That keeps track of when to start timer that turns the cards back.
        // In the Powerpoint it said the easy way to fix the problem of the second card turning back instantly is to make the TurnTargetTime longer.
        // I did not want the the card to turn that long. So i made my own solution.
        if(previousGameState == CardStatus.rotating_to_front && status == CardStatus.show_front)
        {
            game.flipStateOfSelectedCards++;
        }

        previousGameState = status;

        /* ------------ rotating ------------ */
        if(status == CardStatus.rotating_to_front || status == CardStatus.rotating_to_back)
        {
            float percentage;

            turnTimer += Time.deltaTime;
            percentage = turnTimer / turnTargetTime;

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, percentage);

            if(percentage >= 1)
            {
                if(status == CardStatus.rotating_to_back)
                {
                    status = CardStatus.show_back;
                    turnTimer = 0;
                }
                else if(status == CardStatus.rotating_to_front)
                {
                    status = CardStatus.show_front;
                    turnTimer = 0;
                }
            }
        }
    }

    /* ------------- flipping ------------- */
    void OnMouseUp()
    {
        if(game.AllowedToSelectCard(this) == true)
        {
            if(status == CardStatus.show_back)
            {
                game.SelectCard(gameObject);
                TurnToFront();
            }
            else if(status == CardStatus.show_front) 
            {
                TurnToBack();
            }
        }
    }

    public void TurnToFront()
    {
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0,180,0);

        status = CardStatus.rotating_to_front;
    }

    public void TurnToBack()
    {
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0,0,0);

        status = CardStatus.rotating_to_back;
    }

    /* ------------- sprite renderer ------------- */

    private void GetFrontAndBackSpriteRenderers()
    {
        foreach(Transform t in transform)
        {
            if(t.name == "Front")
            {
                frontRenderer = t.GetComponent<SpriteRenderer>();
            }
            else 
            {
                backRenderer = t.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void SetFront(Sprite sprite)
    {
        if(frontRenderer != null)
        {
            frontRenderer.sprite = sprite;
        }
    }

    public void SetBack(Sprite sprite)
    {
        if(backRenderer != null)
        {
            backRenderer.sprite = sprite;
        }
    }

    public Vector2 GetFrontSize()
    {
        if(frontRenderer == null)
        {
            Debug.LogError("No frontRenderer found.");
        }
        return frontRenderer.bounds.size;
    }

    public Vector2 GetBackSize()
    {
        if(backRenderer == null)
        {
            Debug.LogError("No backRenderer found.");
        }
        return backRenderer.bounds.size;
    }
}
