using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D;


namespace UnityGinRummy
{
    public class Card : MonoBehaviour
    {
        public SpriteAtlas Atlas;

        public Suits Suit = Suits.NoSuits;
        public Ranks Rank = Ranks.NoRanks;

        public string OwnerId;

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
            //Debug.Log("spriteName is " + spriteName);
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
            Suit = (Suits)(value % 4);
        }

        public void OnSelected(bool selected)
        {
            // TODO
        }

        public void SetDisplayOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
        }

       
    }
}
