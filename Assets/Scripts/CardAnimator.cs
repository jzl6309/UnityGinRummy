using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityGinRummy
{
    public class CardAnimation
    {
        public Card card;
        public Vector2 dest;
        public Quaternion rot;
    
        public CardAnimation(Card c, Vector2 pos)
        {
            card = c;
            dest = pos;
            rot = Quaternion.identity;
        }

        public CardAnimation(Card c, Vector2 pos, Quaternion r)
        {
            card = c;
            dest = pos;
            rot = r;
        }

        public bool Play()
        {
            bool finished = false;

            if (Vector2.Distance(card.transform.position, dest) < Constants.CARD_SNAP_DISTANCE)
            {
                card.transform.position = dest;
                finished = true;
            }
            else
            {
                card.transform.position = Vector2.MoveTowards(card.transform.position, dest, Constants.CARD_MOVEMENT_SPEED * Time.deltaTime);
                card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rot, Constants.CARD_ROTATION_SPEED * Time.deltaTime);
            }

            return finished;
        }
        
    }


    public class CardAnimator : MonoBehaviour 
    {
        public GameObject CardPrefab;
        public List<Card> DisplayingCards;
        public Queue<CardAnimation> cardAnimations;

        CardAnimation currentCardAnimation;
        Vector2 startPosition = new Vector2(-3f, 0.5f);

        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;

        // Start is called before the first frame update
        void Start()
        {
            cardAnimations = new Queue<CardAnimation>();
            InitializeDeck();
        }

        void InitializeDeck()
        {
            DisplayingCards = new List<Card>();
            for (byte value = 0; value < 52; value++)
            {
                Vector2 newPosition = startPosition + Vector2.right * Constants.DECK_CARD_POSITION_OFFSET * value;
                GameObject newGameObject = Instantiate(CardPrefab, newPosition, Quaternion.identity);
                newGameObject.transform.parent = transform;
                Card card = newGameObject.GetComponent<Card>();
                card.SetDisplayOrder(-1);
                card.transform.position = newPosition;
                DisplayingCards.Add(card);
             }
        }

        public void DealDisplayCards(Player player1, Player player2)
        {
            int start = DisplayingCards.Count - 1;
            int stop = DisplayingCards.Count - Constants.INITIAL_CARDS - 1;

            List<Card> cardsToRemoveFromDeck = new List<Card>();

            int p = 0;
            for (int i = start; i > stop; i--)
            {
                Card card = DisplayingCards[i];
                
                if (p%2 == 0)
                {
                    p++;
                    player1.ReceiveDisplayCard(card);
                    cardsToRemoveFromDeck.Add(card);
                    AddCardAnimation(card, player1.NextCardPosition());
                }
                else
                {
                    p++;
                    player2.ReceiveDisplayCard(card);
                    cardsToRemoveFromDeck.Add(card);
                    AddCardAnimation(card, player2.NextCardPosition());
                }

                foreach (Card c in cardsToRemoveFromDeck)
                    DisplayingCards.Remove(c);
            }
        }

        public void AddCardAnimation(Card card, Vector2 pos)
        {

            CardAnimation cardAnimation = new CardAnimation(card, pos);
            cardAnimations.Enqueue(cardAnimation);
            working = true;
        }

        private void Update()
        {
            if (currentCardAnimation == null ) 
            {
                NextAnimation();
            }
            else
            {
                if (currentCardAnimation.Play())
                {
                    NextAnimation();
                }
            }
        }

        void NextAnimation()
        {
            currentCardAnimation = null;

            if (cardAnimations.Count > 0)
            {
                CardAnimation cardAnimation = cardAnimations.Dequeue();
                currentCardAnimation = cardAnimation;
            }
            else if (working)
            {
                working = false;
                OnAllAnimationsFinished.Invoke();
            }

        }
    }

}
