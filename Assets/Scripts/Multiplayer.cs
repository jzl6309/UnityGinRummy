using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;

namespace UnityGinRummy
{
    public class Multiplayer : Game
    {
        protected new void Awake()
        {
            base.Awake();

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
                            player1.PlayerId = playerId;
                            player1.PlayerName = playerName;
                        }
                        else
                        {
                            player2.PlayerId = playerId;
                            player2.PlayerName = playerName;

                        }
                    }

                    gameDataManager = new GameDataManager(player1, player2, faceUpPile);
                }
                else
                {
                    Debug.Log("Failed to get players");
                }
            });
        }
    
    }
}