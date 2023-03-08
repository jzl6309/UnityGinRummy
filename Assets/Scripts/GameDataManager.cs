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

        public void SetCurrentMelds(Player player)
        {
            List<byte> cardBytes = protectedData.PlayerCards(player);
            List<Card> unmeldedCards = new List<Card>();

            foreach (byte b in cardBytes)
                unmeldedCards.Add((Card)Card.allcards[b].Clone());
           
            List<List<List<Card>>> bestMelds = GinRummyUtil.cardsToBestMeldSets(unmeldedCards);

            if (bestMelds.Count == 0)
            {
                Debug.Log("Player " + player.PlayerId + " has " + GinRummyUtil.getDeadwoodPoints(unmeldedCards) + " deadwood.\n");
                protectedData.SetCurrentMeldsToPlayer(player, null);
            }
            else
            {
                List<List<Card>> melds = bestMelds[0];
                foreach (List<Card> meld in melds)
                    foreach (Card card in meld)
                        for (int i = 0; i < unmeldedCards.Count; i++)
                        {
                            if (card.GetCardId() == unmeldedCards[i].GetCardId())
                            {
                                unmeldedCards.RemoveAt(i);
                            }
                        }

                Debug.Log("Melds: Player " + player.PlayerId + " has " + GinRummyUtil.getDeadwoodPoints(unmeldedCards) + " deadwood.\n");

                List<List<byte>> meldValsList = new List<List<byte>>();
                foreach (List<Card> meld in melds) { 
                    List<byte> meldVals = new List<byte>();
                    foreach (Card card in meld)
                    {
                        meldVals.Add(card.GetCardId());
                    }
                    meldValsList.Add(meldVals);
                }
                protectedData.SetCurrentMeldsToPlayer(player, meldValsList);
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

        public void RemoveCardFromPlayer(Player player, byte card)
        {
            protectedData.RemoveCardFromPlayer(player, card);
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
