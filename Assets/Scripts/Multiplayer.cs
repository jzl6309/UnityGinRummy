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
            
            netCode = FindObjectOfType<NetCode>();
            
            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
            {
                if (successful)
                {
                    foreach(SWPlayer player in reply.players)
                    {
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
            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.GameStarted;
                gameDataManager.SetGameState(gameState);

                netCode.ModifyGameData(gameDataManager.EncryptedData());
                netCode.NotifyOtherPlayerGameStateChanged();
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
                
                gameState = GameState.FirstTurn;

                gameDataManager.SetGameState(gameState);
                Debug.Log("gamestate is " + gameState);
                netCode.ModifyGameData(gameDataManager.EncryptedData());
            }
            
            GinRummyUtil.initialzeMeldTools();
            
            cardAnimator.DealDisplayCards(localPlayer, remotePlayer, faceUpPile);

        }

        public override void AllAnimationsFinished()
        {
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayerGameStateChanged();
            }
        }

    }
}