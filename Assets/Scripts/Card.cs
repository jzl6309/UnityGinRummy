using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;

namespace UnityGinRummy
{
    public class Card : MonoBehaviour, ICloneable
    {
        public static List<Card> allcards = new List<Card>();

        public SpriteAtlas Atlas;
        public Suits Suit = Suits.NoSuits;
        public Ranks Rank = Ranks.NoRanks;

        public string OwnerId;
        public bool isSelected;
        public byte cardId;

        /**
	    * map from String representations to Card objects
	    */
        public static Dictionary<string, Card> strCardMap = new Dictionary<string, Card>();

        /**
         * map from string representations to Card id numbers
         */
        public static Dictionary<string, int> strIdMap = new Dictionary<string, int>();

        /**
         * map from Card id numbers to string representations
         */
        public static Dictionary<int, string> idStrMap = new Dictionary<int, string>();




        SpriteRenderer spriteRenderer;

        bool faceUp = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            UpdateSprite();
        }

        public static void AddCard(Card c)
        {
            allcards.Add(c);
            strCardMap[c.SpriteName()] = c;
            strIdMap[c.SpriteName()] = c.GetCardId();
            idStrMap[c.GetCardId()] = c.SpriteName();
        }

        void UpdateSprite()
        {
            if (faceUp)
            {
                spriteRenderer.sprite = Atlas.GetSprite(SpriteName());
            }
            else
            {
                spriteRenderer.sprite = Atlas.GetSprite(Constants.CARD_BACK);
            }
        }

        string SpriteName()
        {
            string spriteName = $"{Suit}{GetRankDescription()}";
            return spriteName; 
        }

        string GetRankDescription()
        {
            FieldInfo fieldInfo = Rank.GetType().GetField(Rank.ToString());
            DescriptionAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes[0].Description;
        }

        public void SetFaceUp(bool value)
        {
            faceUp = value;
            UpdateSprite();

            if (value == false)
            {
                Rank = Ranks.NoRanks;
                Suit = Suits.NoSuits;
            }
        }

        public void SetCardValue(byte value)
        {
            // 0-3 are 1's
            // 4-7 are 2's
            // ...
            // 48-51 are kings's
            Rank = (Ranks)(value / 4 + 1);

            // 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 are Spades(0)
            // 1, 5, 9, 13, 17, 21, 25, 29, 33, 37, 41, 45, 49 are Clubs(1)
            // 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50 are Diamonds(2)
            // 3, 7, 11, 15, 19, 23, 27, 31, 35, 39, 43, 47, 51 are Hearts(3)
            Suit = (Suits)(value % 4);
        }

        public byte GetCardId()
        {
            return cardId;
        }

        public void SetCardId(byte id)
        {
            cardId = id;
        }

        public static Card GetCard(int rank, int suit)
        {
            return allcards[(rank-1) * Constants.NUM_SUITS + suit];
        }

        public void OnSelected(bool selected)
        {
            isSelected = selected;
            if (isSelected)
            {
                transform.position = (Vector2)transform.position + Vector2.up * Constants.SELECTED_CARD_OFFSET;
            }
            else
            {
                transform.position = (Vector2)transform.position - Vector2.up * Constants.SELECTED_CARD_OFFSET;
            }
        }

        public void SetDisplayOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }

        public object Clone()
        {
            Card card = new Card
            {
                Suit = Suit,
                Rank = Rank,
                cardId = cardId
            };
            return card; 
        }
    }
}
