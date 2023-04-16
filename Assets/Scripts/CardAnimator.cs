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
        public List<Card> FaceUpDisplay;
        public Queue<CardAnimation> cardAnimations;

        CardAnimation currentCardAnimation;
        Vector2 startPosition = new Vector2(-7f, 0.5f);

        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;

        // Start is called before the first frame update
        void Start()
        {
            cardAnimations = new Queue<CardAnimation>();
            InitializeDeck();
        }

        public void InitializeDeck()
        {
            DisplayingCards = new List<Card>();
            for (byte i = 0; i < 52; i++)
            {
                Vector2 newPosition = startPosition + Vector2.right * Constants.DECK_CARD_POSITION_OFFSET * i;
                GameObject newGameObject = Instantiate(CardPrefab, newPosition, Quaternion.identity);
                newGameObject.transform.parent = transform;
                Card card = newGameObject.GetComponent<Card>();
                card.SetDisplayOrder(-1);
                card.transform.position = newPosition;
                DisplayingCards.Add(card);
             }
        }

        public void DealDisplayCards(Player player1, Player player2, Player pile)
        {
            int start = DisplayingCards.Count - 1;
            int stop = DisplayingCards.Count - Constants.INITIAL_CARDS - 1;

            List<Card> cardsToRemoveFromDeck = new List<Card>();

            Card card = null;
            int p = 0;
            for (int i = start; i > stop + 1; i--)
            {
                card = DisplayingCards[i];
                card.SetDisplayOrder(-1);

                if (p % 2 == 0)
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
            }

            card = DisplayingCards[stop + 1];
            pile.ReceiveDisplayCard(card);
            cardsToRemoveFromDeck.Add(card);
            FaceUpDisplay.Add(card);
            AddCardAnimation(card, pile.NextDiscardPosition());

            foreach (Card c in cardsToRemoveFromDeck)
                DisplayingCards.Remove(c);
        }

        public void DrawDisplayCard(Player player, Card card)
        {
            if (card == null)
            {
                card = DisplayingCards[DisplayingCards.Count - 1];
                player.ReceiveDisplayCard(card);
                AddCardAnimation(card, player.NextCardPosition());
                DisplayingCards.Remove(card);
            }
            else
            {
                Debug.LogError("There's an issue in DrawDisplayCard");
            }
        }
        public void DrawDisplayingCardsFromFaceUpPile(Player player, Player faceUpPile, byte ID)
        {
            int numDisplayCards = FaceUpDisplay.Count;

            if (numDisplayCards > 0)
            {
                Card card = FaceUpDisplay[numDisplayCards - 1];
                card.SetCardValue(ID);
                card.SetFaceUp(true);
                player.ReceiveDisplayCard(card);
                AddCardAnimation(card, player.NextCardPosition());

                faceUpPile.Remove(card);
                FaceUpDisplay.Remove(card);
            }
        }

        public void DiscardDisplayCardsToFaceUpPile(Player player, Player faceUpPile, byte ID)
        {
            foreach (Card card in player.DisplayingCards)
            {
                if (card.Rank == Card.GetRank(ID) && card.Suit == Card.GetSuit(ID) || card.Rank == Ranks.NoRanks)
                {
                    Card c = card;
                    c.SetCardValue(ID);
                    c.SetFaceUp(true);
                    faceUpPile.ReceiveDisplayCard(c);
                    FaceUpDisplay.Add(c);
                    AddCardAnimation(c, faceUpPile.NextDiscardPosition());

                    player.Remove(card);
                    break;
                }
            }
        }

        public void AddCardAnimation(Card card, Vector2 pos)
        {
            CardAnimation cardAnimation = new CardAnimation(card, pos);
            cardAnimations.Enqueue(cardAnimation);
            working = true;
        }

        public void ClearAllCards(Player player)
        {
            for (int i = 0; i < player.DisplayingCards.Count; i++)
            {
                Card c = player.DisplayingCards[i];
                Vector2 newPosition = startPosition + Vector2.right * Constants.DECK_CARD_POSITION_OFFSET * DisplayingCards.Count;
                AddCardAnimation(c, newPosition);
                DisplayingCards.Add(c);
            }

            player.ClearAllCards();

            if (player.PlayerId == "Face Up Pile")
            {
                FaceUpDisplay.Clear();
            }
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
