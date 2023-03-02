using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity;
using UnityEngine.UI;

namespace UnityGinRummy
{
    public class Game : MonoBehaviour
    {
        public GameDataManager gameDataManager;

        CardAnimator cardAnimator;

        Player localPlayer;
        Player remotePlayer;
        Player currentTurnPlayer;

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Idel,
            GameStarted,
            GameFinished
        };

        public GameState gameState = GameState.Idel;

        private void Awake()
        {
            localPlayer = new Player();
            localPlayer.PlayerId = "Player 1";
            localPlayer.PlayerName = "Player 1";
            localPlayer.Position = PlayerPositions[0].position;

            remotePlayer = new Player();
            remotePlayer.PlayerId = "Player 2";
            remotePlayer.PlayerName = "Gin Rummy Bot";
            remotePlayer.Position = PlayerPositions[1].position;
            remotePlayer.isBot = true;

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
                
            }

            switch (gameState)
            {
                case GameState.Idel:
                    {
                        Debug.Log("Idelling");
                        break;
                    }
                case GameState.GameStarted:
                    {
                        Debug.Log("The Game Started");
                        OnGameStart();
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
            gameDataManager = new GameDataManager(localPlayer, remotePlayer);
            gameDataManager.Shuffle();
            gameDataManager.Deal(localPlayer, remotePlayer);

            cardAnimator.DealDisplayCards(localPlayer, remotePlayer);

            gameState = GameState.GameFinished;
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
            if (currentTurnPlayer == null | currentTurnPlayer == remotePlayer)
            {
                currentTurnPlayer = localPlayer;
            }
            else
            {
                currentTurnPlayer = remotePlayer;
            }
        }
    }
}
