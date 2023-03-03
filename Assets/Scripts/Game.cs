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
        Player faceUpPile;
        Player currentTurnPlayer;

        public List<Transform> PlayerPositions = new List<Transform>();

        public enum GameState
        {
            Waiting,
            GameStarted,
            FirstTurn,
            SelectDraw,
            SelectDiscard,
            GameFinished
        };

        public GameState gameState = GameState.Waiting;

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
            gameDataManager = new GameDataManager(localPlayer, remotePlayer, faceUpPile);
            gameDataManager.Shuffle();
            gameDataManager.Deal(localPlayer, remotePlayer, faceUpPile);

            cardAnimator.DealDisplayCards(localPlayer, remotePlayer, faceUpPile);

            gameState = GameState.GameFinished;
        }
        void OnFirstTurn()
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
            if (currentTurnPlayer == null | currentTurnPlayer == remotePlayer)
            {
                currentTurnPlayer = localPlayer;
            }
            else
            {
                currentTurnPlayer = remotePlayer;
            }
        }

        public void CheckForMelds()
        {
            List<byte> playersCards = gameDataManager.PlayerCards(localPlayer);
            localPlayer.SetCardValues(playersCards);
        }

        public void SetFaceUpPile()
        {
            List<byte> cards = gameDataManager.PlayerCards(faceUpPile);
            faceUpPile.SetCardValues(cards);
        }

        public void ShowAndHideCards()
        {
            localPlayer.ShowCards();
            remotePlayer.HideCards();
            faceUpPile.ShowCards();
        }

        public void OnCardSelected(Card card)
        {

        }
    }
}
