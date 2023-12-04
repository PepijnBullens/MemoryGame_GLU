using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStatus
{
    waiting_on_first_card,
    waiting_on_second_card,
    match_found,
    no_match_found
}

public class Game : MonoBehaviour
{
    /* -------------------------- variables -------------------------- */

    /* ------------- Loading in ------------- */

    // rows and columns
    [SerializeField]
    private int rows;
    [SerializeField]
    private int columns;

    [SerializeField]
    private float totalPairs;


    // folder paths
    [SerializeField]
    private string frontSidesFolder = "Sprites/Frontsides/";
    [SerializeField]
    private string backSidesFolder = "Sprites/Backsides/";

    // sprite arrays
    [SerializeField]
    private Sprite[] frontSprites;
    [SerializeField]
    private Sprite[] backSprites;

    // list of selected front sprites
    [SerializeField]
    private List<Sprite> selectedFrontSprites = new List<Sprite>();

    // selected back sprite
    [SerializeField]
    private Sprite selectedBackSprite;


    
    [SerializeField]
    private GameObject cardPrefab;

    private Stack<GameObject> stackOfCards = new Stack<GameObject>();

    private GameObject[,] placedCards;

    [SerializeField]
    private Transform fieldAnchor;

    [SerializeField]
    private float offsetX;
    [SerializeField]
    private float offsetY;

    /* ------------- GamePlay ------------- */

    [SerializeField]
    private GameStatus status;

    private GameObject[] selectedCards;

    private float timeoutTimer;
    [SerializeField]
    private float timeoutTarget;

    // keeps track of when to flip cards back
    public int flipStateOfSelectedCards = 0;


    public bool AllowedToSelectCard(Card card)
    {
        // if no card is selected
        if(selectedCards[0] == null)
        {
            return true;
        }

        // if one card is selected
        if(selectedCards[1] == null)
        {
            if(selectedCards[0] != card.gameObject)
            {
                return true;
            }
        }

        // if two cards are selected
        return false;
    }

    // start
    private void Start()
    {
        MakeCards();
        DistributeCards();

        selectedCards = new GameObject[2];
        status = GameStatus.waiting_on_first_card;
    }

    /* ------------- Loading in ------------- */

    private void MakeCards()
    {
        CalculateAmountOfPairs();
        LoadSprites();
        SelectFrontSprites();
        SelectBackSprite();
        ConstructCards();
    }

    // calculate if number of cards is even
    private void CalculateAmountOfPairs()
    {
        if(rows * columns % 2 == 0)
        {
            totalPairs = (rows * columns) / 2;
        }
        else 
        {
            Debug.LogError("Can't play memory with uneven amount of cards.");
        }
    }

    // load in sprites
    private void LoadSprites()
    {
        frontSprites = Resources.LoadAll<Sprite>(frontSidesFolder);
        backSprites = Resources.LoadAll<Sprite>(backSidesFolder);
    }

    // place front cards in selectedFrontSprites list randomly
    private void SelectFrontSprites()
    {
        if(frontSprites.Length < totalPairs)
        {
            Debug.LogError("There are not enough playing cards to make " + totalPairs + " pairs.");
        }

        selectedFrontSprites = new List<Sprite>();

        while(selectedFrontSprites.Count < totalPairs)
        {
            int rnd = Random.Range(0, frontSprites.Length);

            if(selectedFrontSprites.Contains(frontSprites[rnd]) == false)
            {
                selectedFrontSprites.Add(frontSprites[rnd]);
            }
        }
    }

    // place random back card in selectedBackSprite variable
    private void SelectBackSprite()
    {
        if(backSprites.Length > 0)
        {
            int rnd = Random.Range(0, backSprites.Length);

            selectedBackSprite = backSprites[rnd];
        }
        else 
        {
            Debug.LogError("There are no background sprites.");
        }
    }

    // make pairs and place them in a stack
    private void ConstructCards()
    {
        stackOfCards = new Stack<GameObject>();

        foreach(Sprite selectedFrontSprite in selectedFrontSprites)
        {
            for(int i = 0; i < 2; i++)
            {
                GameObject go = Instantiate(cardPrefab);
                Card cardScript = go.GetComponent<Card>();

                cardScript.SetBack(selectedBackSprite);
                cardScript.SetFront(selectedFrontSprite);

                go.name = selectedFrontSprite.name;

                stackOfCards.Push(go);
            }
        }
    }

    private void DistributeCards()
    {
        placedCards = new GameObject[columns, rows];
        ShuffleCards();
        PlaceCardsOnField();
    }

    // shuffle cards by setting a random position and then removing from the stack
    private void ShuffleCards()
    {
        while(stackOfCards.Count > 0)
        {
            int randX = Random.Range(0, columns);
            int randY = Random.Range(0, rows);

            if(placedCards[randX, randY] == null)
            {
                placedCards[randX, randY] = stackOfCards.Pop();
            }
        }
    }

    private void PlaceCardsOnField()
    {
        // Make a nested for loop, for the x and y position inside the 2d array.
        for(int y = 0; y < rows; y++)
        {
            for(int x = 0; x < columns; x++)
            {
                // We loop through the x and y variables in the for loop. 
                // So as long as we have columns and then rows left to check.
                // We can use the x and y variables to get the properties of every card on the playing field.

                // Get current loopings card and put it in a GameObject variable named "card".
                GameObject card = placedCards[x, y];
                // Get the card script from GameObject variable card.
                Card cardScript = card.GetComponent<Card>();

                // Get card size from the card script attached to card GameObject.
                Vector2 cardSize = cardScript.GetBackSize();

                // Set the current loopings card to the position of the field anchor.
                // Plus the card size and offset combined.
                // Times the iteration of the for loop. In this case the variable "x" or "y".
                float posX = fieldAnchor.position.x + (x * (cardSize.x + offsetX));
                float posY = fieldAnchor.position.y + (y * (cardSize.y + offsetY));

                placedCards[x, y].transform.position = new Vector3(posX, posY, 0f);
            }
        }
    }

    /* ------------- GamePlay ------------- */

    public void SelectCard(GameObject card)
    {
        if(status == GameStatus.waiting_on_first_card)
        {
            selectedCards[0] = card;
            status = GameStatus.waiting_on_second_card;
        }
        else if(status == GameStatus.waiting_on_second_card)
        {
            selectedCards[1] = card;
            CheckForMatchingPair();
        }
    }

    // check if cards match
    private void CheckForMatchingPair()
    {
        timeoutTimer = 0f;
        if(selectedCards[0].name == selectedCards[1].name)
        {
            status = GameStatus.match_found;
        }
        else 
        {
            status = GameStatus.no_match_found;
        }
    }

    // rotate back if we have no match. Or remove if we do have a match
    private void RotateBackOrRemovePair()
    {
        if(flipStateOfSelectedCards == 2)
        {
            timeoutTimer += Time.deltaTime;
        }

        if(timeoutTimer >= timeoutTarget)
        {
            flipStateOfSelectedCards = 0;

            if(status == GameStatus.match_found)
            {
                selectedCards[0].SetActive(false);
                selectedCards[1].SetActive(false);
            }
            else if(status == GameStatus.no_match_found)
            {
                selectedCards[0].GetComponent<Card>().TurnToBack();
                selectedCards[1].GetComponent<Card>().TurnToBack();
            }

            selectedCards[0] = null;
            selectedCards[1] = null;

            status = GameStatus.waiting_on_first_card;
        }
    }

    private void Update()
    {
        if(status == GameStatus.match_found || status == GameStatus.no_match_found)
        {
            RotateBackOrRemovePair();
        }
    }
}
