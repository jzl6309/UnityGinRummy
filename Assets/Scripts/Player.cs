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
            card.OwnerId = PlayerId;
            DisplayingCards.Add(card);
            NumberOfDisplayedCards++;
        }

        public Vector2 NextCardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * Constants.PLAYER_CARD_POSITION_OFFSET * NumberOfDisplayedCards;
            return nextPos;
        }

        public Vector2 NextDiscardPosition()
        {
            Vector2 nextPos = Position + Vector2.right * Constants.DECK_CARD_POSITION_OFFSET * NumberOfDisplayedCards;
            return nextPos;
        }

        public bool Equals(Player that)
        {
            if (PlayerId.Equals(that.PlayerId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ShowCards()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(true);
            }
        }

        public void HideCards()
        {
            foreach (Card card in DisplayingCards)
            {
                card.SetFaceUp(false);
            }
        }

        public void SetCardValues(List<byte> vals)
        {
            if (DisplayingCards.Count != vals.Count)
            {
                Debug.LogError("Somethings wrong in Player.SetCardValues\n" + "DisplayingCards count " + DisplayingCards.Count + " vals count " + vals.Count);
                return;
            }

            for (int i = 0; i < vals.Count; i++)
            {
                Card card = DisplayingCards[i];
                card.SetCardValue(vals[i]);
                card.SetDisplayOrder(i);
            }
        }

        public List<Card> GetDisplayCards()
        {
            List<Card> cards = new List<Card>();

            foreach (Card card in DisplayingCards)
                cards.Add((Card)card.Clone());

            return cards;
        }

        public Boolean willDrawFaceUpCard(Card card)
        {
            return false;
        }
    }
}