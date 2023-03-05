using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGinRummy
{
    [Serializable]
    public class GameDataManager
    {
        Player player1;
        Player player2;
        Player faceUpPile;

        [SerializeField]
        ProtectedData protectedData;


        public GameDataManager(Player one, Player two, Player cardPile)    
        {
            player1 = one;
            player2 = two;
            faceUpPile = cardPile;
            protectedData = new ProtectedData(player1.PlayerId, player2.PlayerId, faceUpPile.PlayerId);
        }

        public void Shuffle()
        {
            List<byte> cardValues = new List<byte>();

            for (byte code = 0; code < 52; code++)
            {
                cardValues.Add(code);
                Card card = new Card();
                card.SetCardValue(code);
                card.SetCardId(code);
                Card.AddCard(card);
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

        public void Deal(Player player1, Player player2, Player pile)
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numCardInPool = poolOfCards.Count;
            int range = numCardInPool - 1 - (Constants.INITIAL_CARDS);

            List<byte> cards = poolOfCards.GetRange(range, (Constants.INITIAL_CARDS));
            poolOfCards.RemoveRange(range, Constants.INITIAL_CARDS);

            List<byte> player1Cards = new List<byte>();
            List<byte> player2Cards = new List<byte>();
            List<byte> faceUpCards = new List<byte>();

            for (int i = 0; i < cards.Count - 1; i++)
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
            
            faceUpCards.Add(cards[cards.Count - 1]);
            
            protectedData.AddCardValuesToPlayer(player1, player1Cards);
            protectedData.AddCardValuesToPlayer(player2, player2Cards);
            protectedData.AddCardValuesToPlayer(pile, faceUpCards);
        }

        public List<List<Card>> GetMelds(Player player)
        {
            List<byte> cardBytes = protectedData.PlayerCards(player);
            List<Card> cards = new List<Card>();

            foreach (byte b in cardBytes)
                cards.Add((Card)Card.allcards[b].Clone());

            List<List<List<Card>>> melds = GinRummyUtil.cardsToBestMeldSets(cards);

            if (melds.Count == 0)
            {
                Debug.Log("Player " + player + " has " + cards + " with " + GinRummyUtil.getDeadwoodPoints(cards) + " deadwood.\n");
                return null;
            }
            else
            {
                List<List<Card>> bestMelds = melds[0];
                foreach (List<Card> meld in bestMelds)
                    foreach (Card card in meld)
                        cards.Remove(card);
                bestMelds.Add(cards);
                Debug.Log("Player " + player + " has " + bestMelds + " with " + GinRummyUtil.getDeadwoodPoints(cards) + " deadwood.\n");
                return bestMelds;
            }
        }

        public byte DrawCard()
        {
            List<byte> poolOfCards = protectedData.GetPoolOfCards();

            int numCardsInPool = poolOfCards.Count;

            if (numCardsInPool > 0)
            {
                byte val = poolOfCards[numCardsInPool - 1];
                poolOfCards.Remove(val);

                return val;
            }

            return Constants.NO_MORE_CARDS; 
        }

        public void AddCardToPlayer(Player player, byte card)
        {
            protectedData.AddCardToPlayer(player, card);
        }

        public byte DrawFaceUpCard()
        {
            List<byte> faceUpCards = protectedData.PlayerCards(faceUpPile);
            byte card = faceUpCards[faceUpCards.Count - 1];

            faceUpCards.Remove(card);
            return card; 
        }

        public List<byte> PlayerCards(Player player)
        {
            return protectedData.PlayerCards(player);
        }
    }
}
