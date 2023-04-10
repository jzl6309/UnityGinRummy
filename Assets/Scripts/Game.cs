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
        [SerializeField]
        public GameDataManager gameDataManager;
        
        public Text MessageText;
        public Text ButtonText;
        public Text DeadWoodText;
        public Text Player1ScoreText;
        public Text Player2ScoreText;
        public Text HandScoreText;

        protected CardAnimator cardAnimator;

        [SerializeField]
        protected Player localPlayer;
        [SerializeField]
        protected Player remotePlayer;
        [SerializeField]
        protected Player faceUpPile;

        [SerializeField]
        protected Player currentTurnPlayer;
        [SerializeField]
        protected Player currentTurnTargetPlayer;

        protected Player playerKnocked;
        protected int player1Points = 0;
        protected int player2Points = 0;

        protected Card selectedCard;
        protected byte drawnCard = 255;
        protected bool playerCanKnock = false;

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Waiting,
            GameStarted,
            FirstTurnStarted,
            FirstTurn,
            FirstTurnPassStarted,
            FirstTurnPass,
            ConfirmTakeFaceUpCard,
            SelectDraw,
            ConfirmDrawCard,
            SelectDiscard,
            ConfirmSelectDiscard,
            Knock,
            HandFinished,
            GameFinished
        };

        [SerializeField]
        public GameState gameState = GameState.Waiting;

        protected void Awake()
        {
            Debug.Log("base awake");
            localPlayer = new Player();
            localPlayer.PlayerId = "Player 1";
            localPlayer.PlayerName = "Player 1";
            localPlayer.Position = PlayerPositions[0].position;
            localPlayer.isBot = false;

            remotePlayer = new Player();
            remotePlayer.PlayerId = "Player 2";
            remotePlayer.PlayerName = "Gin Rummy Bot";
            remotePlayer.Position = PlayerPositions[1].position;
            remotePlayer.isBot = true;

            faceUpPile = new Player();
            faceUpPile.PlayerId = "Face Up Pile";
            faceUpPile.PlayerName = "Face Up Pile";
            faceUpPile.Position = PlayerPositions[2].position;

            cardAnimator = FindObjectOfType<CardAnimator>();

        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("base start");
            gameState = GameState.GameStarted;
            GameFlow();
        }

        public virtual void GameFlow()
        {
            if (gameState > GameState.GameStarted && gameState < GameState.GameFinished)
            {
                if (gameState != GameState.ConfirmTakeFaceUpCard && gameState != GameState.ConfirmSelectDiscard
                    && gameState != GameState.ConfirmDrawCard)
                {
                    SetFaceUpPile();
                    CheckForMelds();
                }
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
                case GameState.FirstTurnStarted:
                    {
                        Debug.Log("First Turn Started");
                        OnFirstTurnStarted();
                        break;
                    }
                case GameState.FirstTurn:
                    {
                        Debug.Log("First Turn");
                        OnFirstTurn();
                        break;
                    }
                case GameState.FirstTurnPassStarted:
                    {
                        Debug.Log("First Turn Pass Started");
                        OnFirstTurnPassStarted();
                        break;
                    }
                case GameState.FirstTurnPass:
                    {
                        Debug.Log("First Turn Pass");
                        OnFirstTurnPass();
                        break;
                    }
                case GameState.ConfirmTakeFaceUpCard:
                    {
                        Debug.Log("ConfirmTakeFaceUpCard");
                        OnConfirmTakeFaceUpCard();
                        break;
                    }
                case GameState.SelectDraw:
                    {
                        Debug.Log("Select Draw");
                        OnSelectDraw();
                        break;
                    }
                case GameState.ConfirmDrawCard:
                    {
                        Debug.Log("ConfirmDrawCard");
                        OnConfirmDrawCard();
                        break;
                    }
                case GameState.SelectDiscard:
                    {
                        Debug.Log("Select Discard");
                        OnSelectDiscard();
                        break;
                    }
                case GameState.ConfirmSelectDiscard:
                    {
                        Debug.Log("ConfirmSelectDiscard");
                        OnConfirmSelectDiscard();
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
        
        protected virtual void OnGameStart()
        {
            GinRummyUtil.initialzeMeldTools();
            gameDataManager = new GameDataManager(localPlayer, remotePlayer, faceUpPile);
            gameDataManager.Shuffle();
            gameDataManager.Deal(localPlayer, remotePlayer, faceUpPile);

            cardAnimator.DealDisplayCards(localPlayer, remotePlayer, faceUpPile);

            gameState = GameState.FirstTurnStarted;
        }

        protected virtual void OnFirstTurnStarted()
        {
            Debug.Log("OnFirstTurnStarted");
            SwitchTurns();
            gameState = GameState.FirstTurn;
            GameFlow();
        }

        protected virtual void OnFirstTurn()
        {
            Debug.Log("OnFirstTurn");
            if (currentTurnPlayer == localPlayer)
            {
                MessageText.text = "Take Face Up Card?";
                ButtonText.text = "Pass";
            }
            else if (currentTurnPlayer == remotePlayer)
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
                }
                else
                {
                    SwitchTurns();
                    gameState = GameState.FirstTurnPass;
                }
                StartCoroutine(WaitForFunction());
            }
        }

        protected virtual void OnFirstTurnPassStarted()
        {
            Debug.Log("OnFirstPassTurnStarted");
            SwitchTurns();
            gameState = GameState.FirstTurnPass;
            GameFlow();
        }

        void OnFirstTurnPass()
        {
            if (currentTurnPlayer == localPlayer)
            {
                MessageText.text = "Take Face Up Card?";
                ButtonText.text = "Pass";
            }
            else if (currentTurnPlayer == remotePlayer)
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

        protected virtual void OnConfirmTakeFaceUpCard()
        {
            Debug.Log("OnConfirmTakeFaceUpCard");
        }

        void OnSelectDraw()
        {
            Debug.Log(currentTurnPlayer.PlayerId + " is ready to draw");
            if (currentTurnPlayer == localPlayer)
            {
                MessageText.text = "Draw a card";
                ButtonText.text = "Draw";
            }
            else if (currentTurnPlayer == remotePlayer)
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

        protected virtual void OnConfirmDrawCard()
        {
            Debug.Log("OnConfirmDrawCard");
        }

        void OnSelectDiscard()
        {
            playerCanKnock = gameDataManager.CheckKnock(currentTurnPlayer);

            if (currentTurnPlayer == localPlayer)
            {
                MessageText.text = "Discard";
                if (playerCanKnock)
                    ButtonText.text = "Knock";
                else
                {
                    ButtonText.text = "";
                }
            }
            else if (currentTurnPlayer == remotePlayer)
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

        protected virtual void OnConfirmSelectDiscard()
        {

        }

        protected virtual void OnKnock()
        {
            gameState = GameState.Waiting;
            bool gin = GetFinalDiscard(currentTurnPlayer);

            CheckOppMelds();
            ShowAllCards();

            List<int> finalPoints = gameDataManager.GetFinalPoints(localPlayer, remotePlayer);

            int points1 = finalPoints[0];
            int points2 = finalPoints[1];
            int bonus = 0;

            Debug.Log("Player 1 deadwood points " + points1);
            Debug.Log("Player 2 deadwood points " + points2);

            if (playerKnocked == localPlayer)
            {
                if (points1 == 0 && gin)
                {
                    player1Points += points2 + GinRummyUtil.BIG_GIN_BONUS;
                    bonus = GinRummyUtil.BIG_GIN_BONUS;
                }
                else if (points1 == 0)
                {
                    player1Points += points2 + GinRummyUtil.GIN_BONUS;
                    bonus = GinRummyUtil.GIN_BONUS;
                }
                else if (points1 < points2)
                {
                    player1Points += points2 - points1;
                }
                else
                {
                    player2Points += GinRummyUtil.UNDERCUT_BONUS + points1;
                    bonus = GinRummyUtil.UNDERCUT_BONUS;
                }
            }
            else
            {
                if (points2 == 0 && gin)
                {
                    player2Points += points1 + GinRummyUtil.BIG_GIN_BONUS;
                    bonus = GinRummyUtil.BIG_GIN_BONUS;
                }
                else if (points2 == 0)
                {
                    player2Points += points1 + GinRummyUtil.GIN_BONUS;
                    bonus = GinRummyUtil.GIN_BONUS;
                }
                else if (points2 < points1)
                {
                    player2Points += points1 - points2;
                }
                else
                {
                    player1Points += GinRummyUtil.UNDERCUT_BONUS + points2;
                    bonus = GinRummyUtil.UNDERCUT_BONUS;
                }
            }

            Debug.Log(localPlayer.PlayerId + " has " + player1Points + " points");
            Debug.Log(remotePlayer.PlayerId + " has " + player2Points + " points");

            SetScoresText(points1, points2, playerKnocked, bonus);

            StartCoroutine(WaitForHandFinishedFunction());
        }

        protected virtual void OnHandFinished()
        {
            Debug.Log("OnHandFinished");
            if (player1Points < GinRummyUtil.GOAL_SCORE && player2Points < GinRummyUtil.GOAL_SCORE)
            {
                HideAllCards();

                Debug.Log("I am clearing player 1 cards");
                cardAnimator.ClearAllCards(localPlayer);
                Debug.Log("I am clearing player 2 cards");
                cardAnimator.ClearAllCards(remotePlayer);
                Debug.Log("I am clearing faceUpPile cards");
                cardAnimator.ClearAllCards(faceUpPile);

                Debug.Log("I finished");
                gameState = GameState.GameStarted;
            }
        }

        public void OnGameFinished()
        {

        }

        public IEnumerator WaitForFunction()
        {
            yield return new WaitForSeconds(2);
            GameFlow();
        }

        public IEnumerator WaitForDrawFunction()
        {
            yield return new WaitForSeconds(2);
            gameState = GameState.SelectDiscard;
            GameFlow();
        }

        public virtual IEnumerator WaitForHandFinishedFunction()
        {
            yield return new WaitForSeconds(3);
            gameState = GameState.HandFinished;
            GameFlow();
        }

        public void SetScoresText(int player1Deadwood, int player2Deadwood, Player playerKnocked, int bonus)
        {
            if (playerKnocked == localPlayer)
            {
                if (player1Deadwood < player2Deadwood) 
                {
                    if (bonus == 0)
                    {
                        HandScoreText.text = "Player 1 score: " + player2Deadwood + " - " + player1Deadwood + " = " + (player2Deadwood - player1Deadwood);
                    }
                    else if (bonus == GinRummyUtil.BIG_GIN_BONUS)
                    {
                        HandScoreText.text = "Player 1 score: " + player2Deadwood + " - " + player1Deadwood + " = " + (player2Deadwood - player1Deadwood) +
                                             "\n+ Big Gin Bonus 50";
                    }
                    else if (bonus == GinRummyUtil.GIN_BONUS)
                    {
                        HandScoreText.text = "Player 1 score: " + player2Deadwood + " - " + player1Deadwood + " = " + (player2Deadwood - player1Deadwood) +
                                             "\n+ Gin Bonus 25";
                    }
                }
                else
                {
                    HandScoreText.text = "Player 2 score: " + player1Deadwood + " - " + player2Deadwood + " = " + (player1Deadwood - player2Deadwood) +
                                         "\n+ Gin Bonus 25";
                }

            }
            else
            {
                if (player2Deadwood < player1Deadwood)
                {
                    if (bonus == 0)
                    {
                        HandScoreText.text = "Player 2 score: " + player1Deadwood + " - " + player2Deadwood + " = " + (player1Deadwood - player2Deadwood);
                    }
                    else if (bonus == GinRummyUtil.BIG_GIN_BONUS)
                    {
                        HandScoreText.text = "Player 2 score: " + player1Deadwood + " - " + player2Deadwood + " = " + (player1Deadwood - player2Deadwood) +
                                             "\n+ Big Gin Bonus 50";
                    }
                    else if (bonus == GinRummyUtil.GIN_BONUS)
                    {
                        HandScoreText.text = "Player 2 score: " + player1Deadwood + " - " + player2Deadwood + " = " + (player1Deadwood - player2Deadwood) +
                                             "\n+ Gin Bonus 25";
                    }
                }
                else
                {
                    HandScoreText.text = "Player 1 score: " + player2Deadwood + " - " + player1Deadwood + " = " + (player2Deadwood - player1Deadwood) +
                                         "\n+ Gin Bonus 25";
                }
            }


            Player1ScoreText.text = "Player 1: " + player1Points;
            Player2ScoreText.text = "Player 2: " + player2Points;
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
                drawnCard = 255;
                return false;
            }
        }

        public virtual void AllAnimationsFinished()
        {
            GameFlow();
        }

        public void SwitchTurns()
        {
            Debug.Log("SwitchTurns - " + currentTurnPlayer.PlayerName);
            /*
            if (currentTurnPlayer == null)
            {
                /*
                var rand = new System.Random();
                int n = rand.Next(2);
                if (n == 0) currentTurnPlayer = player1;
                else currentTurnPlayer = player2;
                
                Debug.Log("SwitchTurns - set current player");
                currentTurnPlayer = localPlayer;
            }
            */
            if (currentTurnPlayer == localPlayer)
            {
                currentTurnPlayer = remotePlayer;
            }
            else if (currentTurnPlayer == remotePlayer)
            {
                currentTurnPlayer = localPlayer;
            }
            else
            {
                currentTurnPlayer = localPlayer;
            }
        }

        public void SetCurrentMelds(Player player)
        {
            int deadwood = gameDataManager.SetCurrentMelds(player);
            /*
            if (currentTurnPlayer == localPlayer)
            {
                DeadWoodText.text = deadwood.ToString();
            }
            else
            {
                DeadWoodText.text = deadwood.ToString();
            }*/
            DeadWoodText.text = deadwood.ToString();
        }

        public void CheckForMelds()
        {
            SetCurrentMelds(localPlayer);
            List<byte> playersCards = gameDataManager.PlayerCards(localPlayer);
            localPlayer.SetCardValues(playersCards);
        }

        public void CheckOppMelds()
        {
            List<byte> playersCards = gameDataManager.PlayerCards(remotePlayer);
            remotePlayer.SetCardValues(playersCards);
            SetCurrentMelds(remotePlayer);
        }

        public void ShowAllCards()
        {
            localPlayer.ShowCards();
            remotePlayer.ShowCards();
        }

        public void HideAllCards()
        {
            localPlayer.HideCards();
            remotePlayer.HideCards();
            faceUpPile.HideCards();
        }

        public void SetFaceUpPile()
        {
            List<byte> faceUpCards = gameDataManager.PlayerCards(faceUpPile);
            faceUpPile.SetCardValues(faceUpCards);
        }

        public void ShowAndHideCards()
        {
            localPlayer.ShowCards();
            remotePlayer.HideCards();
            faceUpPile.ShowCards();
        }

        public virtual void ReceiveCardFromFaceUpPile(Player player)
        {
            byte card = gameDataManager.DrawFaceUpCard();
            drawnCard = card;

            //Debug.Log("face up card is " + Card.GetRank(card) + " " + Card.GetSuit(card));
            gameDataManager.AddCardToPlayer(player, card);

            cardAnimator.DrawDisplayingCardsFromFaceUpPile(player, faceUpPile, card);
        }

        public virtual void DrawCard()
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

        public virtual void Discard(Player player)
        {
            byte card;
            if (currentTurnPlayer.isBot)
            {
                card = gameDataManager.GetDiscard(currentTurnPlayer, drawnCard);
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
            drawnCard = 255;
        }

        public virtual void OnCardSelected(Card card)
        {
            if (gameState == GameState.FirstTurn)
            {
                if (card.OwnerId == faceUpPile.PlayerId && currentTurnPlayer == localPlayer)
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
            if (gameState == GameState.FirstTurnPass)
            {
                if (card.OwnerId == faceUpPile.PlayerId && currentTurnPlayer == localPlayer)
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
                if (card.OwnerId == faceUpPile.PlayerId && currentTurnPlayer == localPlayer)
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
                if (card.OwnerId == currentTurnPlayer.PlayerId && currentTurnPlayer == localPlayer)
                {
                    if (selectedCard != null && selectedCard.isSelected)
                    {
                        selectedCard.OnSelected(false);
                        selectedCard = null;
                        ButtonText.text = "";
                        if (playerCanKnock)
                            ButtonText.text = "Knock";
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

        public virtual void OnOkSelected()
        {
            if (gameState == GameState.FirstTurn && currentTurnPlayer == localPlayer)
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
                    gameState = GameState.FirstTurnPassStarted;
                    GameFlow();
                }
            }
            else if (gameState == GameState.FirstTurnPass && currentTurnPlayer == localPlayer)
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
            else if (gameState == GameState.SelectDraw && currentTurnPlayer == localPlayer)
            {
                DrawCard();
                gameState = GameState.SelectDiscard;
                GameFlow();
            }
            else if (gameState == GameState.SelectDiscard && currentTurnPlayer == localPlayer)
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
