using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;

namespace UnityGinRummy
{
    public class Multiplayer : Game
    {
        NetCode netCode;
      
        protected new void Awake()
        {
            base.Awake();
            GinRummyUtil.initialzeMeldTools();
            remotePlayer.isBot = false;
            Debug.Log("Multiplayer awake");
            netCode = FindObjectOfType<NetCode>();
            
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
            {
                if (successful)
                {
                    int i = 0;
                    foreach(SWPlayer player in reply.players)
                    {
                        Debug.Log("num players " + i++);
                        string playerName = player.GetCustomDataString();
                        string playerId = player.id;

                        if (playerId.Equals(NetworkClient.Instance.PlayerId))
                        {
                            localPlayer.PlayerId = playerId;
                            localPlayer.PlayerName = playerName;
                        }
                        else
                        {
                            remotePlayer.PlayerId = playerId;
                            remotePlayer.PlayerName = playerName;
                        }
                    }

                    gameDataManager = new GameDataManager(localPlayer, remotePlayer, faceUpPile);
                    netCode.EnableRoomPropertyAgent();
                }
                else
                {
                    Debug.Log("Failed to get players");
                }
            });
        }

        void Start()
        {
            Debug.Log("Multiplayer start");
        }

        public override void GameFlow()
        {
            Debug.LogError("GameFlow from multiplayer should not happen!");
        }

        public void OnGameDataReady(EncryptedData encryptedData)
        {
            Debug.Log("OnGameDataReady");
            if (encryptedData == null)
            { 
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.GameStarted;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayerGameStateChanged();
                }
            }
            else
            {
                gameDataManager.ApplyEncryptedData(encryptedData);
                gameState = gameDataManager.GetGameState();
                Debug.Log("OnGameDataReady - gameState " + gameState);
                currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
                currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
            
                if (gameState > GameState.GameStarted)
                {
                    Debug.Log("OnGameDataReady - GameState");
                    cardAnimator.DealDisplayCards(localPlayer, remotePlayer, faceUpPile);

                    base.GameFlow();
                }
            }
        }
        
        public void OnGameDataChanged(EncryptedData encryptedData)
        {
            Debug.Log("OnGameDataChanged");
            gameDataManager.ApplyEncryptedData(encryptedData);
            gameState = gameDataManager.GetGameState();
            currentTurnPlayer = gameDataManager.GetCurrentTurnPlayer();
            currentTurnTargetPlayer = gameDataManager.GetCurrentTurnTargetPlayer();
        }

        public void OnGameStateChanged()
        {
            base.GameFlow();
        }

        protected override void OnGameStart()
        {
            Debug.Log("OnGameStart - Multi");
            if (NetworkClient.Instance.IsHost)
            {
                gameDataManager.Shuffle();
                gameDataManager.Deal(localPlayer, remotePlayer, faceUpPile);
                
                gameState = GameState.FirstTurnStarted;

                gameDataManager.SetGameState(gameState);
                Debug.Log("gamestate is " + gameState);
                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }
            
            cardAnimator.DealDisplayCards(localPlayer, remotePlayer, faceUpPile);
        }

        protected override void OnFirstTurnStarted()
        {
            Debug.Log("OnFirstTurnStarted - Multi");
            if (NetworkClient.Instance.IsHost)
            {
                SwitchTurns();
                gameState = GameState.FirstTurn;

                gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
            }
        }

        protected override void OnFirstTurnPassStarted()
        {
            Debug.Log("OnFirstTurnPassStarted - Multi");
            if (NetworkClient.Instance.IsHost)
            {
                SwitchTurns();
                gameState = GameState.FirstTurnPass;

                gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
            }
        }

        protected override void OnConfirmTakeFaceUpCard()
        {
            Debug.Log("OnConfirmTakeFaceUpCard - Multi");
            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.SelectDiscard;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }
            ReceiveCardFromFaceUpPile(currentTurnPlayer);
        }

        public override void DrawCard()
        {
            if (selectedCard == null)
            {
                byte card = gameDataManager.DrawCard();
                gameDataManager.AddCardToPlayer(currentTurnPlayer, card);
                //gameDataManager.SetDrawnCard(card);

                gameState = GameState.ConfirmDrawCard;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
            }
            else
            {
                selectedCard = null;

                byte card = gameDataManager.DrawFaceUpCard();
                drawnCard = card;

                gameDataManager.AddCardToPlayer(currentTurnPlayer, card);
                gameDataManager.SetDrawnCard(card);

                gameState = GameState.ConfirmTakeFaceUpCard;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
            }
        }

        protected override void OnConfirmDrawCard()
        {
            Debug.Log("OnConfirmDrawCard -  multi");
            cardAnimator.DrawDisplayCard(currentTurnPlayer, selectedCard);
            currentTurnPlayer.ResetDisplayCards(cardAnimator);

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.SelectDiscard;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }

            selectedCard = null;
            drawnCard = 255;
        }

        public override void ReceiveCardFromFaceUpPile(Player player)
        {
            drawnCard = gameDataManager.GetDrawnCard();
            cardAnimator.DrawDisplayingCardsFromFaceUpPile(player, faceUpPile, drawnCard);
            currentTurnPlayer.ResetDisplayCards(cardAnimator);
        }

        public override void Discard(Player player)
        {
            byte card;
           
            card = selectedCard.GetCardId((int)selectedCard.Rank, (int)selectedCard.Suit);

            gameDataManager.RemoveCardFromPlayer(player, card);
            gameDataManager.AddCardToPlayer(faceUpPile, card);
            gameDataManager.SetDrawnCard(card);

            gameState = GameState.ConfirmSelectDiscard;
            gameDataManager.SetGameState(gameState);

            netCode.ModifyGameData(gameDataManager.EncryptedData());
            netCode.NotifyOtherPlayerGameStateChanged();
        }

        protected override void OnConfirmSelectDiscard()
        {
            drawnCard = gameDataManager.GetDrawnCard();
            cardAnimator.DiscardDisplayCardsToFaceUpPile(currentTurnPlayer, faceUpPile, drawnCard);
            currentTurnPlayer.ResetDisplayCards(cardAnimator);

            if (NetworkClient.Instance.IsHost)
            {
                SwitchTurns();
                gameState = GameState.SelectDraw;
                gameDataManager.SetGameState(gameState);
                gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }

            selectedCard = null;
            drawnCard = 255;
        }

        protected override void OnHandFinished()
        {
            Debug.Log("OnHandFinished - Multi");
            if (player1Points < GinRummyUtil.GOAL_SCORE && player2Points < GinRummyUtil.GOAL_SCORE)
            {
                HideAllCards();

                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.GameStarted;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                }
                Debug.Log("I am clearing player 1 cards");
                cardAnimator.ClearAllCards(localPlayer);
                Debug.Log("I am clearing player 2 cards");
                cardAnimator.ClearAllCards(remotePlayer);
                Debug.Log("I am clearing faceUpPile cards");
                cardAnimator.ClearAllCards(faceUpPile);

                Debug.Log("I finished");
            }
        }

        public override void OnOkSelected()
        {
            if (gameState == GameState.FirstTurn && currentTurnPlayer == localPlayer)
            {
                if (selectedCard != null)
                {
                    MessageText.text = "Takes the card";
                    selectedCard = null;

                    byte card = gameDataManager.DrawFaceUpCard();
                    drawnCard = card;

                    gameDataManager.AddCardToPlayer(currentTurnPlayer, card);
                    gameDataManager.SetDrawnCard(card);

                    gameState = GameState.ConfirmTakeFaceUpCard;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayerGameStateChanged();
                }
                else
                {
                    MessageText.text = "Pass";
                    gameState = GameState.FirstTurnPassStarted;

                    gameDataManager.SetGameState(gameState);

                    netCode.NotifyOtherPlayerGameStateChanged();
                }
            }
            else if (gameState == GameState.FirstTurnPass && currentTurnPlayer == localPlayer)
            {
                if (selectedCard != null)
                {
                    MessageText.text = "Takes the card";
                    selectedCard = null;

                    byte card = gameDataManager.DrawFaceUpCard();
                    drawnCard = card;

                    gameDataManager.AddCardToPlayer(currentTurnPlayer, card);
                    gameDataManager.SetDrawnCard(card);

                    gameState = GameState.ConfirmTakeFaceUpCard;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayerGameStateChanged();
                }
                else
                {
                    MessageText.text = "Pass";
                    gameState = GameState.SelectDraw;
                    SwitchTurns();

                    gameDataManager.SetGameState(gameState);
                    gameDataManager.SetCurrentTurnPlayer(currentTurnPlayer);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayerGameStateChanged();
                }
            }
            else if (gameState == GameState.SelectDraw && currentTurnPlayer == localPlayer)
            {
                DrawCard();
            }
            else if (gameState == GameState.SelectDiscard && currentTurnPlayer == localPlayer)
            {
                if (selectedCard != null)
                {
                    Discard(currentTurnPlayer);
                }
                else if (playerCanKnock)
                {
                    playerKnocked = currentTurnPlayer;

                    gameState = GameState.Knock;
                    gameDataManager.SetGameState(gameState);

                    netCode.ModifyGameData(gameDataManager.EncryptedData());
                    netCode.NotifyOtherPlayerGameStateChanged();
                }
            }
        }

        public override IEnumerator WaitForHandFinishedFunction()
        {
            yield return new WaitForSeconds(3);
            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.HandFinished;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
            }
        }

        public override void AllAnimationsFinished()
        {
            Debug.Log("Animations Finished");
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayerGameStateChanged();        
            }
        }

    }
}