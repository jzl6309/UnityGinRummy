using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGinRummy
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }

    [Serializable]
    public class GameDataManager
    {
        Player localPlayer;
        Player remotePlayer;
        Player faceUpPile;

        [SerializeField]
        ProtectedData protectedData;


        public GameDataManager(Player one, Player two, Player cardPile)    
        {
            localPlayer = one;
            remotePlayer = two;
            faceUpPile = cardPile;
            protectedData = new ProtectedData(localPlayer.PlayerId, remotePlayer.PlayerId, faceUpPile.PlayerId);
        }

        public void Shuffle()
        {
            List<byte> cardValues = new List<byte>();

            for (byte code = 0; code < 52; code++)
            {
                cardValues.Add(code);
                /*
                Card card = new Card();
                card.SetCardValue(code);
                card.SetCardId(code);
                Card.AddCard(card);
                */
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

        public int SetCurrentMelds(Player player)
        {
            int deadwood = 0;

            List<byte> cardBytes = protectedData.PlayerCards(player);
            List<Card> unmeldedCards = new List<Card>();

            foreach (byte b in cardBytes)
                unmeldedCards.Add((Card)Card.allcards[b].Clone());
           
            List<List<List<Card>>> bestMelds = GinRummyUtil.cardsToBestMeldSets(unmeldedCards);

            if (bestMelds.Count == 0)
            {
                deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);
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

                deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);

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
            return deadwood;
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

        public bool CheckKnock(Player player)
        {
            int deadwood = Int32.MaxValue;

            List<byte> cardBytes = protectedData.PlayerCards(player);
            List<Card> unmeldedCards = new List<Card>();

            foreach (byte b in cardBytes)
                unmeldedCards.Add((Card)Card.allcards[b].Clone());

            // Check if deadwood of maximal meld is low enough to go out. 
            List<List<List<Card>>> bestMeldSets = GinRummyUtil.cardsToBestMeldSets(unmeldedCards);

            if (bestMeldSets.Count == 0)
            {
                deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);
                return false;
            }
            else
            {
                List<List<Card>> melds = bestMeldSets[0];
                foreach (List<Card> meld in melds)
                    foreach (Card card in meld)
                        for (int i = 0; i < unmeldedCards.Count; i++)
                        {
                            if (card.GetCardId() == unmeldedCards[i].GetCardId())
                            {
                                unmeldedCards.RemoveAt(i);
                            }
                        }

                byte max = Byte.MinValue;
                foreach (Card card in unmeldedCards)
                    if (card.GetCardId() > max)
                        max = card.GetCardId();

                for (int i = 0; i < unmeldedCards.Count; i++)
                {
                    if (unmeldedCards[i].GetCardId() == max) { 
                        unmeldedCards.RemoveAt(i);
                        break;
                    }
                }

                deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);
                Debug.Log("Deadwood in checkDeadwood is " + deadwood);
                Debug.Log("The unmelded card count in checkDeadwood is " + unmeldedCards.Count);
                return deadwood <= 50;// GinRummyUtil.MAX_DEADWOOD;
            }
        }

        public byte GetFinalDiscard(Player player)
        {
            List<byte> cardBytes = protectedData.PlayerCards(player);
            List<Card> unmeldedCards = new List<Card>();

            foreach (byte b in cardBytes)
                unmeldedCards.Add((Card)Card.allcards[b].Clone());

            List<List<List<Card>>> bestMeldSets = GinRummyUtil.cardsToBestMeldSets(unmeldedCards);

            List<List<Card>> melds = bestMeldSets[0];
            foreach (List<Card> meld in melds)
                foreach (Card card in meld)
                    for (int i = 0; i < unmeldedCards.Count; i++)
                    {
                        if (card.GetCardId() == unmeldedCards[i].GetCardId())
                        {
                            unmeldedCards.RemoveAt(i);
                        }
                    }

            byte max = Byte.MinValue;
            foreach (Card card in unmeldedCards)
                if (card.GetCardId() > max)
                    max = card.GetCardId();

            foreach (Card card in unmeldedCards)
                if (card.GetCardId() == max)
                {
                    return card.GetCardId();
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

        public byte GetDiscard(Player player, byte drawnFaceUpCard)
        {
            int minDeadwood = Int32.MaxValue;
            List<Card> candidateCards = new List<Card>();

            List<byte> cardVals = protectedData.PlayerCards(player);
            List<Card> cards = new List<Card>();

            foreach (byte card in cardVals)
                cards.Add((Card)Card.allcards[card].Clone());

            foreach (Card card in cards)
            {
                if (card.GetCardId() == drawnFaceUpCard)
                    continue;

                List<Card> remainingCards = new List<Card>();
                foreach (Card c in cards)
                {
                    if (card.GetCardId() != c.GetCardId())
                        remainingCards.Add((Card)c.Clone());
                }

                int deadwood = 0;
                List<List<List<Card>>> bestMeldSets = GinRummyUtil.cardsToBestMeldSets(remainingCards);
                if (bestMeldSets.Count == 0)
                {
                    deadwood = GinRummyUtil.getDeadwoodPoints(remainingCards);
                }
                else
                {
                    deadwood = GinRummyUtil.getDeadwoodPoints(bestMeldSets[0], remainingCards);
                }

                if (deadwood <= minDeadwood)
                {
                    if (deadwood < minDeadwood)
                    {
                        minDeadwood = deadwood;
                        candidateCards.Clear();
                    }
                    candidateCards.Add(card);
                }
            }

            if (candidateCards.Count == 0)
                return Constants.NO_MORE_CARDS;

            Card discard = candidateCards[UnityEngine.Random.Range(0, candidateCards.Count)];

            return discard.GetCardId();
        }

        public List<int> GetFinalPoints(Player player1, Player player2)
        {
            List<int> finalPoints = new List<int>();
            List<Player> players = new List<Player>();
            players.Add(player1);
            players.Add(player2);

            foreach (Player player in players)
            {
                int deadwood = 0;

                List<byte> cardBytes = protectedData.PlayerCards(player);
                List<Card> unmeldedCards = new List<Card>();

                foreach (byte b in cardBytes)
                    unmeldedCards.Add((Card)Card.allcards[b].Clone());

                List<List<List<Card>>> bestMelds = GinRummyUtil.cardsToBestMeldSets(unmeldedCards);

                if (bestMelds.Count == 0)
                {
                    deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);
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

                    deadwood = GinRummyUtil.getDeadwoodPoints(unmeldedCards);
                }
                finalPoints.Add(deadwood);
            }
            return finalPoints;
        }

        public EncryptedData EncryptedData()
        {
            Debug.Log("EncryptedData");
            Byte[] data = protectedData.ToArray();

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.data = data;

            return encryptedData;
        }

        public void ApplyEncryptedData(EncryptedData encryptedData)
        {
            Debug.Log("ApplyEncryptedData");
            if(encryptedData == null)
            {
                return;
            }

            protectedData.ApplyByteArray(encryptedData.data);
        }

        public void SetGameState(Game.GameState gamestate)
        {
            protectedData.SetCurrentGameState((int)gamestate);
        }

        public Game.GameState GetGameState()
        {
            return (Game.GameState)protectedData.GetCurrentGameState();
        }

        public void SetDrawnCard(byte card)
        {
            protectedData.SetDrawnCard(card);
        }

        public byte GetDrawnCard()
        {
            return protectedData.GetDrawnCard();
        }

        public void SetGotGin(bool gin)
        {
            protectedData.SetGotGin(gin);
        }

        public bool GetGotGin()
        {
            return protectedData.GetGotGin();
        }

        public void SetCurrentTurnPlayer(Player player)
        {
            protectedData.SetCurrentTurnPlayerId(player.PlayerId);
        }

        public Player GetCurrentTurnPlayer()
        {
            if (localPlayer == null) Debug.Log("WTF!!!");
            string playerId = protectedData.GetCurrentTurnPlayerId();
            Debug.Log("playerid string is " + playerId);
            if (playerId.Equals(localPlayer.PlayerId)) 
            {
                return localPlayer;
            }
            else
            {
                return remotePlayer;
            }
        }

        public void SetPlayerThatKnocked(Player player)
        {
            protectedData.SetCurrentTurnPlayerId(player.PlayerId);
        }

        public Player GetPlayerThatKnocked()
        {
            string playerId = protectedData.GetPlayerThatKnocked();
            Debug.Log("playerid string is " + playerId);
            if (playerId.Equals(localPlayer.PlayerId))
            {
                return localPlayer;
            }
            else
            {
                return remotePlayer;
            }
        }

        public Player GetCurrentTurnTargetPlayer()
        {
            string playerId = protectedData.GetCurrentTurnPlayerId();
            if (playerId.Equals(localPlayer.PlayerId))
            {
                return remotePlayer;
            }
            else
            {
                return localPlayer;
            }
        }
    }
}
