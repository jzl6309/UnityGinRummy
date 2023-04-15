using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SWNetwork;

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
        [SerializeField]
        string currentTurnPlayerId;
        [SerializeField]
        string playerThatKnocked;
        [SerializeField]
        int currentGameState;
        [SerializeField]
        byte drawnCard;
        [SerializeField]
        bool gotGin;


        public ProtectedData(string p1ID, string p2ID, string cardPileID)
        {
            player1ID = p1ID;
            player2ID = p2ID;
            currentTurnPlayerId = "";
            playerThatKnocked = "";
            faceUpID = cardPileID;
            //CalculateKey(roomId);
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
                //player1Cards.Sort();
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

        public void RemoveCardFromPlayer(Player player, byte card)
        {
            if (player.PlayerId.Equals(player1ID))
            {
                player1Cards.Remove(card);
                //player1Cards.Sort();
            }
            else if (player.PlayerId.Equals(player2ID))
            {
                player2Cards.Remove(card);
                player2Cards.Sort();
            }
            else
            {
                faceUpCardPile.Remove(card);
            }
        }

        public void SetCurrentMeldsToPlayer(Player player, List<List<byte>> melds)
        {
            if (player.PlayerId.Equals(player1ID)) 
            {
                player1Cards.Sort();
                if (melds == null)
                {
                    Debug.Log("NO MELDS!");
                    return;
                }
                    
                List<byte> cards = new List<byte>();
                foreach (List<byte> meld in melds)
                    foreach (byte card in meld)
                        cards.Add(card);

                foreach (byte card in player1Cards)
                    if (!cards.Contains(card))
                        cards.Add(card);

                player1Cards = cards;
            }
            else if (player.PlayerId.Equals(player2ID))
            {
                player2Cards.Sort();
                if (melds == null)
                {
                    Debug.Log("NO MELDS!");
                    return;
                }

                List<byte> cards = new List<byte>();
                foreach (List<byte> meld in melds)
                    foreach (byte card in meld)
                        cards.Add(card);

                foreach (byte card in player2Cards)
                    if (!cards.Contains(card))
                        cards.Add(card);

                player2Cards = cards;
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

        public void SetCurrentTurnPlayerId(string playerId)
        {
            currentTurnPlayerId = playerId;
        }

        public string GetCurrentTurnPlayerId()
        {
            return currentTurnPlayerId;
        }

        public void SetDrawnCard(byte card)
        {
            drawnCard = card;
        }

        public byte GetDrawnCard()
        {
            return drawnCard;
        }

        public void SetGotGin(bool gin)
        {
            gotGin = gin;
        }

        public bool GetGotGin()
        {
            return gotGin;
        }

        public void SetCurrentGameState(int gamestate)
        {
            currentGameState = gamestate;
        }

        public int GetCurrentGameState()
        {
            return currentGameState;
        }

        public void SetPlayerThatKnocked(string playerId)
        {
            playerThatKnocked = playerId;
        }

        public string GetPlayerThatKnocked()
        {
            return playerThatKnocked;
        }

        public Byte[] ToArray()
        {
            Debug.Log("ProtectedData - ToArray");
            SWNetworkMessage msg = new SWNetworkMessage();
            msg.Push((byte)poolOfCards.Count);
            msg.PushByteArray(poolOfCards.ToArray());

            msg.Push((byte)player1Cards.Count);
            msg.PushByteArray(player1Cards.ToArray());
            msg.Push((byte)player2Cards.Count);
            msg.PushByteArray(player2Cards.ToArray());
            msg.Push((byte)faceUpCardPile.Count);
            msg.PushByteArray(faceUpCardPile.ToArray());

            msg.PushUTF8ShortString(player1ID);
            msg.PushUTF8ShortString(player2ID);
            msg.PushUTF8ShortString(faceUpID);

            msg.PushUTF8ShortString(currentTurnPlayerId);
            msg.PushUTF8ShortString(playerThatKnocked);

            msg.Push(currentGameState);
            msg.Push(drawnCard);
            msg.Push(gotGin);

            return msg.ToArray();
        }

        public void ApplyByteArray(Byte[] byteArray)
        {
            Debug.Log("ProtectedData - ApplyByteArray");
            SWNetworkMessage msg = new SWNetworkMessage(byteArray);
            byte poolofCardsCount = msg.PopByte();
            poolOfCards = msg.PopByteArray(poolofCardsCount).ToList();

            byte player1CardsCount = msg.PopByte();
            player1Cards = msg.PopByteArray(player1CardsCount).ToList();
            byte player2CardsCount = msg.PopByte();
            player2Cards = msg.PopByteArray(player2CardsCount).ToList();
            byte faceUpPileCount = msg.PopByte();
            faceUpCardPile = msg.PopByteArray(faceUpPileCount).ToList();

            player1ID = msg.PopUTF8ShortString();
            player2ID = msg.PopUTF8ShortString();
            faceUpID = msg.PopUTF8ShortString();

            currentTurnPlayerId = msg.PopUTF8ShortString();
            playerThatKnocked = msg.PopUTF8ShortString();
            currentGameState = msg.PopInt32();
            Debug.Log("dencrypted gamestate " + currentGameState);

            drawnCard = msg.PopByte();
            gotGin = msg.PopBool();
        }
    }
}
