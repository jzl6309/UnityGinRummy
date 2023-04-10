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
            Vector2 nextPos = Position + Vector2.right * Constants.DISCARD_PILE_POSITION_OFFSET * NumberOfDisplayedCards;
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
                Debug.LogError("Somethings wrong with " + PlayerId + "s cards in Player.SetCardValues\n" + "DisplayingCards count " + DisplayingCards.Count + " vals count " + vals.Count);
                return;
            }

            for (int i = 0; i < vals.Count; i++)
            {
                Card card = DisplayingCards[i];
                card.SetCardValue(vals[i]);
                card.SetDisplayOrder(i);
            }
        }

        public void Remove(Card card)
        {
            DisplayingCards.Remove(card);
            NumberOfDisplayedCards--;
        }

        public void ClearAllCards()
        {
            DisplayingCards.Clear();
            NumberOfDisplayedCards = 0;
        }

        public List<Card> GetDisplayCards()
        {
            List<Card> cards = new List<Card>();

            foreach (Card card in DisplayingCards)
                cards.Add((Card)card.Clone());

            return cards;
        }

        public void ResetDisplayCards(CardAnimator ca)
        {
            Debug.Log("ResetDisplayCards");
            NumberOfDisplayedCards = 0;
            foreach (Card card in DisplayingCards)
            {
                NumberOfDisplayedCards++;
                ca.AddCardAnimation(card, NextCardPosition());
            }
        }

        public Boolean willDrawFaceUpCard(Player faceUpPile)
        {
            List<Card> cards = new List<Card>();
            foreach (Card c in DisplayingCards)
                cards.Add((Card) c.Clone());

            Card card = faceUpPile.DisplayingCards[faceUpPile.DisplayingCards.Count - 1];

            cards.Add((Card) card.Clone());

            foreach (List<Card> meld in GinRummyUtil.cardsToAllMelds(cards))
                foreach (Card c in meld)
                    if (card.GetCardId() == c.GetCardId())
                        return true;

            return false;
        }
    }
}