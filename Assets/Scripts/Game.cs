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

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Waiting,
            GameStarted,
            FirstTurn,
            FirstTurnPass,
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
                case GameState.FirstTurnPass:
                    {
                        Debug.Log("First Turn Pass");
                        OnFirstTurnPass();
                        break;
                    }
                case GameState.SelectDraw:
                    {
                        Debug.Log("Select Draw");
                        OnSelectDraw();
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
            SwitchTurns();
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Take Face Up Card?";
                ButtonText.text = "Pass";
            }
            else if (currentTurnPlayer == player2)
            {
                MessageText.text = "Oppenent's Turn";
                GameFlow();
            }
        }

        void OnFirstTurnPass()
        {
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Take Face Up Card?";
                ButtonText.text = "Pass";
            }
            else if (currentTurnPlayer == player2)
            {
                MessageText.text = "Oppenent's Turn";
                gameState = GameState.SelectDraw;
                SwitchTurns();
                GameFlow();
            }
        }

        void OnSelectDraw()
        {
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Draw a card";
                ButtonText.text = "Draw";
            }
            else if (currentTurnPlayer == player2)
            {
                SwitchTurns();
            }
        }

        void OnSelectDiscard()
        {
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Discard";
                ButtonText.text = "";
            }
            else if (currentTurnPlayer == player2)
            {
                SwitchTurns();
            }
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

            cardAnimator.DrawDisplayingCardsFromFaceUpPile(player, faceUpPile, card);

            gameDataManager.AddCardToPlayer(player, card);
        }

        public void DrawCard()
        {
            if (selectedCard == null) { 
                byte card =  gameDataManager.DrawCard();
                gameDataManager.AddCardToPlayer(currentTurnPlayer, card);

                cardAnimator.DrawDisplayCard(currentTurnPlayer, selectedCard);

                currentTurnPlayer.ResetDisplayCards(cardAnimator);
                selectedCard = null;
            }
            else
            {
                ReceiveCardFromFaceUpPile(currentTurnPlayer);
                selectedCard = null;
            }
        }

        public void Discard(Player player)
        {
            byte card = selectedCard.GetCardId((int)selectedCard.Rank,(int)selectedCard.Suit);

            gameDataManager.RemoveCardFromPlayer(player, card);
            gameDataManager.AddCardToPlayer(faceUpPile, card);

            cardAnimator.DiscardDisplayCardsToFaceUpPile(player, faceUpPile, card);
            player.ResetDisplayCards(cardAnimator);

            selectedCard = null;
        }

        public void OnCardSelected(Card card)
        {
            if (gameState == GameState.FirstTurn)
            {
                if (card.OwnerId == faceUpPile.PlayerId && currentTurnPlayer == player1)
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
            else if (gameState == GameState.SelectDraw)
            {
                if (card.OwnerId == faceUpPile.PlayerId)
                {
                    if (selectedCard != null && selectedCard.isSelected)
                    {
                        selectedCard.OnSelected(false);
                        selectedCard = null;
                        ButtonText.text = "Draw";
                    }
                    else
                    {
                        selectedCard = card;
                        selectedCard.OnSelected(true);
                        ButtonText.text = "Draw";
                    }
                }
            }
            else if (gameState == GameState.SelectDiscard)
            {
                if (card.OwnerId == currentTurnPlayer.PlayerId)
                {
                    if (selectedCard != null && selectedCard.isSelected)
                    {
                        selectedCard.OnSelected(false);
                        selectedCard = null;
                        ButtonText.text = "";
                    }
                    else
                    {
                        selectedCard = card;
                        selectedCard.OnSelected(true);
                        ButtonText.text = "Discard";
                    }
                }
            }
        }

        public void OnOkSelected()
        {
            if (gameState == GameState.FirstTurn && currentTurnPlayer == player1)
            {
                if (selectedCard != null)
                {
                    MessageText.text = "Takes the card";
                    selectedCard = null;
                    ReceiveCardFromFaceUpPile(currentTurnPlayer);
                    gameState = GameState.SelectDiscard;
                    GameFlow();
                }
                else
                {
                    MessageText.text = "Pass";
                    gameState = GameState.FirstTurnPass;
                    SwitchTurns();
                    GameFlow();
                }
            }
            else if (gameState == GameState.SelectDraw && currentTurnPlayer == player1)
            {
                DrawCard();
                gameState = GameState.SelectDiscard;
                GameFlow();
            }
            else if (gameState == GameState.SelectDiscard && currentTurnPlayer == player1)
            {
                if (selectedCard != null)
                {
                    Discard(currentTurnPlayer);
                    gameState = GameState.SelectDraw;
                    GameFlow();
                }
            }
        }
    }
}
