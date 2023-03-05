using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGinRummy
{
    [Serializable]
    public class ProtectedData
    {
        [SerializeField]
        List<byte> poolOfCards = new List<byte>();
        [SerializeField]
        List<byte> faceUpCardPile = new List<byte>();
        [SerializeField]
        List<byte> player1Cards = new List<byte>();
        [SerializeField]
        List<byte> player2Cards = new List<byte>();

        [SerializeField]
        string player1ID;
        [SerializeField]
        string player2ID;
        [SerializeField]
        string faceUpID;


        public ProtectedData(string p1ID, string p2ID, string cardPileID)
        {
            player1ID = p1ID;
            player2ID = p2ID;
            faceUpID = cardPileID;
        }

        public void SetPoolOfCards(List<byte> cards)
        {
            poolOfCards = cards;
        }

        public List<byte> GetPoolOfCards()
        {
            return poolOfCards;
        }

        public void AddCardValuesToPlayer(Player player, List<byte> cards)
        {
            if (player.PlayerId.Equals(player1ID))
            {
                player1Cards.AddRange(cards);
                player1Cards.Sort();
            }
            else if (player.PlayerId.Equals(player2ID))
            {
                player2Cards.AddRange(cards);
                player2Cards.Sort();
            }
            else
            {
                faceUpCardPile.AddRange(cards);
            }
        }

        public void AddCardToPlayer(Player player, byte card)
        {
            if (player.PlayerId.Equals(player1ID))
            {
                player1Cards.Add(card);
                player1Cards.Sort();
            }
            else if (player.PlayerId.Equals(player2ID))
            {
                player2Cards.Add(card);
                player2Cards.Sort();
            }
            else
            {
                faceUpCardPile.Add(card);
            }
        }

        public List<byte> PlayerCards(Player player)
        {
            if (player.PlayerId.Equals(player1ID))
            {
                return player1Cards;
            }
            else if (player.PlayerId.Equals(player2ID))
            {
                return player2Cards;
            }
            else
            {
                return faceUpCardPile;
            }

        }
    }
}
