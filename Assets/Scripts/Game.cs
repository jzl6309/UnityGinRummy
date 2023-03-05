using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity;
using UnityEngine.UI;
using System;

namespace UnityGinRummy
{
    public class Game : MonoBehaviour
    {
        public GameDataManager gameDataManager;
        public Text MessageText;
        public Text ButtonText;

        CardAnimator cardAnimator;

        Player player1;
        Player player2;
        Player faceUpPile;
        Player currentTurnPlayer;

        Card selectedCard;

        int numTurns = 0;

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Waiting,
            GameStarted,
            FirstTurn,
            OppFirstTurn,
            SelectDraw,
            SelectDiscard,
            OppSelectDraw,
            OppSelectDiscard,
            GameFinished
        };

        public GameState gameState = GameState.Waiting;

        private void Awake()
        {
            player1 = new Player();
            player1.PlayerId = "Player 1";
            player1.PlayerName = "Player 1";
            player1.Position = PlayerPositions[0].position;

            player2 = new Player();
            player2.PlayerId = "Player 2";
            player2.PlayerName = "Gin Rummy Bot";
            player2.Position = PlayerPositions[1].position;
            player2.isBot = true;

            faceUpPile = new Player();
            faceUpPile.PlayerId = "Face Up Pile";
            faceUpPile.PlayerName = "Face Up Pile";
            faceUpPile.Position = PlayerPositions[2].position;

            cardAnimator = FindObjectOfType<CardAnimator>();

        }

        // Start is called before the first frame update
        void Start()
        {
            gameState = GameState.GameStarted;
            GameFlow();
        }

        public void GameFlow()
        {
            if (gameState > GameState.GameStarted)
            {
                SetFaceUpPile();
                CheckForMelds();
                ShowAndHideCards();
            }

            switch (gameState)
            {
                case GameState.Waiting:
                    {
                        Debug.Log("Waiting");
                        break;
                    }
                case GameState.GameStarted:
                    {
                        Debug.Log("The Game Started");
                        OnGameStart();
                        break;
                    }
                case GameState.FirstTurn:
                    {
                        Debug.Log("First Turn");
                        OnFirstTurn();
                        break;
                    }
                case GameState.OppFirstTurn:
                    {
                        Debug.Log("Opponent's First Turn");
                        //OnOppFirstTurn();
                        break;
                    }
                case GameState.SelectDiscard:
                    {
                        Debug.Log("Select Discard");
                        OnSelectDiscard();
                        break;
                    }
                case GameState.GameFinished:
                    {
                        Debug.Log("The Game is finished");
                        OnGameFinished();
                        break;
                    }
            }
        }
        
        void OnGameStart()
        {
            gameDataManager = new GameDataManager(player1, player2, faceUpPile);
            gameDataManager.Shuffle();
            gameDataManager.Deal(player1, player2, faceUpPile);
            GinRummyUtil.initialzeMeldTools();

            cardAnimator.DealDisplayCards(player1, player2, faceUpPile);

            gameState = GameState.FirstTurn;
        }
        void OnFirstTurn()
        {
            numTurns++;
            SwitchTurns();
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Take Face Up Card?";
                ButtonText.text = "Pass";
            }
            else if (currentTurnPlayer == player2)
            {
                MessageText.text = "Oppenent's Turn";
                
            }
            if (currentTurnPlayer.isBot)
            {

            }
        }

        public void OnSelectDiscard()
        {

        }

        public void OnGameFinished()
        {

        }

        public void AllAnimationsFinished()
        {
            GameFlow();
        }

        public void SwitchTurns()
        {
            if (currentTurnPlayer == null)
            {
                var rand = new System.Random();
                int n = rand.Next(2);
                if (n == 0) currentTurnPlayer = player1;
                else currentTurnPlayer = player2;
            }
            else if (currentTurnPlayer == player1)
            {
                currentTurnPlayer = player2;
            }
            else if (currentTurnPlayer == player2)
            {
                currentTurnPlayer = player1;
            }
        }

        public void ShowCurrentMelds(Player player)
        {
            gameDataManager.GetMelds(player);

        }

        public void CheckForMelds()
        {
            List<byte> playersCards = gameDataManager.PlayerCards(player1);
            player1.SetCardValues(playersCards);
            ShowCurrentMelds(player1);
        }

        public void SetFaceUpPile()
        {
            List<byte> faceUpCards = gameDataManager.PlayerCards(faceUpPile);
            faceUpPile.SetCardValues(faceUpCards);
        }

        public void ShowAndHideCards()
        {
            player1.ShowCards();
            player2.HideCards();
            faceUpPile.ShowCards();
        }

        public void ReceiveCardFromFaceUpPile(Player player)
        {
            byte card = gameDataManager.DrawFaceUpCard();

            Debug.Log("face up card is " + Card.GetRank(card) + " " + Card.GetSuit(card));

            cardAnimator.DrawDisplayingCardsFromFaceUpPile(currentTurnPlayer, faceUpPile, card);
            gameState = GameState.SelectDiscard;

            gameDataManager.AddCardToPlayer(currentTurnPlayer, card);
        }

        public void OnCardSelected(Card card)
        {
            if (gameState == GameState.FirstTurn)
            {
                if (card.OwnerId == faceUpPile.PlayerId)
                {
                    if (selectedCard != null && selectedCard.isSelected)
                    {
                        selectedCard.OnSelected(false);
                        selectedCard = null;
                        ButtonText.text = "Pass";
                    }
                    else
                    {
                        selectedCard = card;
                        selectedCard.OnSelected(true);
                        ButtonText.text = "Take Card";
                    }
                }
            }
        }

        public void OnOkSelected()
        {
            if (gameState == GameState.FirstTurn)
            {
                if (selectedCard != null)
                {
                    MessageText.text = "Takes the card";
                    ReceiveCardFromFaceUpPile(currentTurnPlayer);
                }
                else
                {
                    MessageText.text = "Pass";
                    GameFlow();
                }
            }
        }
    }
}
