using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGinRummy
{
    [Serializable]
    public class Player : IEquatable<Player>
    {
        public string PlayerId;
        public string PlayerName;
        public bool isBot;
        public Vector2 Position;
        public Vector2 MeldPosition;

        int NumberOfDisplayedCards;

        public List<Card> DisplayingCards = new List<Card>();

        public void ReceiveDisplayCard(Card card)
        {
            DisplayingCards.Add(card);
            card.OwnerId = PlayerId;
            NumberOfDisplayedCards++;
          
        }

        public Vector2 NextCardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * Constants.PLAYER_CARD_POSITION_OFFSET * NumberOfDisplayedCards;
            return nextPos;
        }

        public bool Equals(Player them)
        {
            if (PlayerId.Equals(them.PlayerId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}