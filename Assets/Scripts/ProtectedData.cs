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
        List<byte> player1Cards = new List<byte>();
        [SerializeField]
        List<byte> player2Cards = new List<byte>();

        [SerializeField]
        string player1ID;
        [SerializeField]
        string player2ID;


        public ProtectedData(string p1ID, string p2ID)
        {
            player1ID = p1ID;
            player2ID = p2ID;
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
            }
            else
            {
                player2Cards.AddRange(cards);
            }
        }
    }
}
