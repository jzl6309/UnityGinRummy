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
        public Text DeadWoodText;

        CardAnimator cardAnimator;

        Player player1;
        Player player2;
        Player faceUpPile;
        Player currentTurnPlayer;
        Player playerKnocked;
        int player1Points = 0;
        int player2Points = 0;

        Card selectedCard;
        byte drawnFaceUpCard = 255;
        bool playerCanKnock = false;

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Waiting,
            GameStarted,
            FirstTurn,
            FirstTurnPass,
            SelectDraw,
            SelectDiscard,
            Knock,
            HandFinished,
            GameFinished
        };

        public GameState gameState = GameState.Waiting;

        private void Awake()
        {
            player1 = new Player();
            player1.PlayerId = "Player 1";
            player1.PlayerName = "Player 1";
            player1.Position = PlayerPositions[0].position;
            player1.isBot = false;

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
                if (gameState != GameState.HandFinished && gameState != GameState.GameFinished)
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
                case GameState.Knock:
                    {
                        Debug.Log("Player Knock");
                        OnKnock();
                        break;
                    }
                case GameState.HandFinished:
                    {
                        Debug.Log("Hand Finished");
                        OnHandFinished();
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

        IEnumerator WaitForFunction()
        {
            yield return new WaitForSeconds(2);
            GameFlow();
        }

        IEnumerator WaitForDrawFunction()
        {
            yield return new WaitForSeconds(2);
            gameState = GameState.SelectDiscard;
            GameFlow();
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
            }
            if (currentTurnPlayer.isBot)
            {
                if (currentTurnPlayer.willDrawFaceUpCard(faceUpPile))
                {
                    ReceiveCardFromFaceUpPile(currentTurnPlayer);
                    SwitchTurns();
                    gameState = GameState.SelectDiscard;
                }
                else
                {
                    SwitchTurns();
                    gameState = GameState.SelectDraw;
                }
                StartCoroutine(WaitForFunction());
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
                MessageText.text = "Opponent's Turn";
            }
            if (currentTurnPlayer.isBot)
            {
                if (currentTurnPlayer.willDrawFaceUpCard(faceUpPile))
                {
                    ReceiveCardFromFaceUpPile(currentTurnPlayer);
                    SwitchTurns();
                    gameState = GameState.SelectDiscard;
                    MessageText.text = "Opp Takes Card";
                }
                else
                {
                    SwitchTurns();
                    gameState = GameState.SelectDraw;
                    MessageText.text = "Opp Passes";
                }
                StartCoroutine(WaitForFunction());
            }
        }

        void OnSelectDraw()
        {
            Debug.Log(currentTurnPlayer.PlayerId + " is ready to draw");
            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Draw a card";
                ButtonText.text = "Draw";
            }
            else if (currentTurnPlayer == player2)
            {
                MessageText.text = "Opponent's Turn";
                ButtonText.text = "";
            }
            if (currentTurnPlayer.isBot)
            {
                if (currentTurnPlayer.willDrawFaceUpCard(faceUpPile))
                {
                    ReceiveCardFromFaceUpPile(currentTurnPlayer);
                    gameState = GameState.Waiting;
                }
                else
                {
                    DrawCard();
                    gameState = GameState.Waiting;
                }
                Debug.Log("waiting after draw");

                StartCoroutine(WaitForDrawFunction());
            }
        }

        void OnSelectDiscard()
        {
            playerCanKnock = gameDataManager.CheckKnock(currentTurnPlayer);

            if (currentTurnPlayer == player1)
            {
                MessageText.text = "Discard";
                if (playerCanKnock)
                    ButtonText.text = "Knock";
                else
                {
                    ButtonText.text = "";
                }
            }
            else if (currentTurnPlayer == player2)
            {
                MessageText.text = "Opponent's Turn";
                ButtonText.text = "";
            }
            if (currentTurnPlayer.isBot)
            {
                if (playerCanKnock)
                {
                    playerKnocked = currentTurnPlayer;
                    gameState = GameState.Knock;
                    GameFlow();
                }
                else
                {
                    Discard(currentTurnPlayer);
                    SwitchTurns();
                    gameState = GameState.SelectDraw;
                }
            }
        }

        void OnKnock()
        {
            bool gin = GetFinalDiscard(currentTurnPlayer);

            CheckOppMelds();
            ShowAllCards();

            List<int> finalPoints = gameDataManager.GetFinalPoints(player1, player2);

            int points1 = finalPoints[0];
            int points2 = finalPoints[1];

            Debug.Log("Player 1 points deadwood " + points1);
            Debug.Log("Player 2 points deadwood " + points2);

            if (playerKnocked == player1)
            {
                if (points1 == 0 && gin)
                {
                    player1Points += points2 + 2 * GinRummyUtil.GIN_BONUS;
                }
                else if (points1 == 0)
                {
                    player1Points += points2 + GinRummyUtil.GIN_BONUS;
                }
                else if (points1 < points2)
                {
                    player1Points += points2 - points1;
                }
                else
                {
                    player2Points += GinRummyUtil.UNDERCUT_BONUS + points1;
                }
            }
            else
            {
                if (points2 == 0 && gin)
                {
                    player2Points += points1 + 2 * GinRummyUtil.GIN_BONUS;
                }
                else if (points2 == 0)
                {
                    player2Points += points1 + GinRummyUtil.GIN_BONUS;
                }
                else if (points2 < points1)
                {
                    player2Points += points1 - points2;
                }
                else
                {
                    player1Points += GinRummyUtil.UNDERCUT_BONUS + points2;
                }
            }

            Debug.Log(player1.PlayerId + " has " + player1Points + " points");
            Debug.Log(player2.PlayerId + " has " + player2Points + " points");

            gameState = GameState.HandFinished;
            GameFlow();
        }

        void OnHandFinished()
        {

        }

        public void OnGameFinished()
        {

        }

        public bool GetFinalDiscard(Player player)
        {
            byte cardVal = gameDataManager.GetFinalDiscard(player);

            if (cardVal == Constants.NO_MORE_CARDS)
            {
                return true;
            }
            else
            {
                gameDataManager.RemoveCardFromPlayer(player, cardVal);
                gameDataManager.AddCardToPlayer(faceUpPile, cardVal);

                cardAnimator.DiscardDisplayCardsToFaceUpPile(player, faceUpPile, cardVal);
                player.ResetDisplayCards(cardAnimator);

                selectedCard = null;
                drawnFaceUpCard = 255;
                return false;
            }
        }

        public void AllAnimationsFinished()
        {
            GameFlow();
        }

        public void SwitchTurns()
        {
            if (currentTurnPlayer == null)
            {
                /*
                var rand = new System.Random();
                int n = rand.Next(2);
                if (n == 0) currentTurnPlayer = player1;
                else currentTurnPlayer = player2;
                */
                currentTurnPlayer = player1;
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

        public void SetCurrentMelds(Player player)
        {
            int deadwood = gameDataManager.SetCurrentMelds(player);
            if (currentTurnPlayer == player1)
            {
                DeadWoodText.text = deadwood.ToString();
            }
        }

        public void CheckForMelds()
        {
            List<byte> playersCards = gameDataManager.PlayerCards(player1);
            player1.SetCardValues(playersCards);
            SetCurrentMelds(player1);
        }

        public void CheckOppMelds()
        {
            List<byte> playersCards = gameDataManager.PlayerCards(player2);
            player2.SetCardValues(playersCards);
            SetCurrentMelds(player2);
        }

        public void ShowAllCards()
        {
            player1.ShowCards();
            player2.ShowCards();
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
            drawnFaceUpCard = card;

            //Debug.Log("face up card is " + Card.GetRank(card) + " " + Card.GetSuit(card));

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
            byte card;
            if (currentTurnPlayer.isBot)
            {
                card = gameDataManager.GetDiscard(currentTurnPlayer, drawnFaceUpCard);
                if (card == Constants.NO_MORE_CARDS)
                {
                    gameState = GameState.Knock;
                    GameFlow();
                }
            }
            else
            {
                card = selectedCard.GetCardId((int)selectedCard.Rank, (int)selectedCard.Suit);
            }

            gameDataManager.RemoveCardFromPlayer(player, card);
            gameDataManager.AddCardToPlayer(faceUpPile, card);

            cardAnimator.DiscardDisplayCardsToFaceUpPile(player, faceUpPile, card);
            player.ResetDisplayCards(cardAnimator);

            selectedCard = null;
            drawnFaceUpCard = 255;
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
            else if (gameState == GameState.FirstTurnPass && currentTurnPlayer == player1)
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
                    gameState = GameState.SelectDraw;
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
                    CheckForMelds();
                    SwitchTurns();
                    gameState = GameState.SelectDraw;
                    GameFlow();
                }
                else if(playerCanKnock)
                {
                    playerKnocked = currentTurnPlayer;
                    gameState = GameState.Knock;
                    GameFlow();
                }
            }
        }
    }
}
