using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGinRummy
{
    [Serializable]
    public class GameDataManager
    {
        Player localPlayer;
        Player remotePlayer;

        [SerializeField]
        ProtectedData protectedData;


        public GameDataManager(Player local, Player remote)
        {
            localPlayer = local;
            remotePlayer = remote;
            protectedData = new ProtectedData(localPlayer.PlayerId, remotePlayer.PlayerId);
        }

        public void Shuffle()
        {
            List<byte> cardValues = new List<byte>();

            for (byte code = 0; code < 52; code++)
            {
                cardValues.Add(code);
            }

            List<byte> poolOfCards = new List<byte>();

            for (int i = 0; i < 52; i++)
            {
                int valueIndexToAdd = UnityEngine.Random.Range(0, cardValues.Count);

                byte valueToAdd = cardValues[valueIndexToAdd];
                poolOfCards.Add(valueToAdd);
                cardValues.Remove(valueToAdd);
            }

            protectedData.SetPoolOfCards(poolOfCards);
        }

        public void Deal(Player player1, Player player2)
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numCardInPool = poolOfCards.Count;
            int range = numCardInPool - 1 - (Constants.INITIAL_CARDS);

            List<byte> cards = poolOfCards.GetRange(range, (Constants.INITIAL_CARDS));
            poolOfCards.RemoveRange(range, Constants.INITIAL_CARDS);

            List<byte> player1Cards = new List<byte>();
            List<byte> player2Cards = new List<byte>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (i % 2 == 0)
                {
                    player1Cards.Add(cards[i]);
                }
                else
                {
                    player2Cards.Add(cards[i]);
                }
            }
            protectedData.AddCardValuesToPlayer(player1, player1Cards);
            protectedData.AddCardValuesToPlayer(player2, player2Cards);
        }

        public List<byte> PlayerCards(Player player)
        {
            return protectedData.PlayerCards(player);
        }
    }
}
