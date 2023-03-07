/**
 * Constants and utilities for Gin Rummy.  Meld checking makes use of bitstring representations
 * where a single long value represents a set of cards.  Each card has an id number i in the range 0-51, and the
 * presence (1) or absense (0) of that card is represented at bit i (the 2^i place in binary).
 * This allows fast set difference/intersection/equivalence/etc. operations with bitwise operators.
 * 
 * Gin Rummy Rules: https://www.pagat.com/rummy/ginrummy.html
 * Adopted variant: North American scoring (25 point bonus for undercut, 25 point bonus for going gin)
 * 
 * @author Todd W. Neller
 * @version 1.0

Copyright (C) 2020 Todd Neller

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Information about the GNU General Public License is available online at:
  http://www.gnu.org/licenses/
To receive a copy of the GNU General Public License, write to the Free
Software Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA
02111-1307, USA.

 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

namespace UnityGinRummy
{
	public class GinRummyUtil
	{
		/**
		* Goal score
		*/
		public const int GOAL_SCORE = 100;
		/**
		 * Bonus for melding all cards before knocking
		 */
		public const int GIN_BONUS = 25;
		/**
		 * Bonus for undercutting (having less than or equal the deadwood of knocking opponent)
		 */
		public const int UNDERCUT_BONUS = 25;
		/**
		 * Maximum deadwood points permitted for knocking
		 */
		public const int MAX_DEADWOOD = 10;
		/**
		 * Deadwood points indexed by card rank
		 */
		private static int[] DEADWOOD_POINTS = new int[Constants.NUM_RANKS];
		/**
		 * Card bitstrings indexed by card id number
		 */
		private static long[] cardBitstrings = new long[Constants.NUM_CARDS];
		/**
		 * List of lists of meld bitstrings.  Melds appearing after melds in lists are supersets, so the 
		 * first meld not made in a list makes further checking in that list unnnecessary.
		 */
		private static List<List<long>> meldBitstrings;
		/**
		 * Map from meld bitstrings to corresponding lists of cards
		 */
		private static Dictionary<long, List<Card>> meldBitstringToCardsMap;
		
		public static void initialzeMeldTools()
        {
			for (int rank = 0; rank < Constants.NUM_RANKS; rank++)
				DEADWOOD_POINTS[rank] = Math.Min(rank + 1, 10);

			long bitstring = 1L;
			for (int i = 0; i < Constants.NUM_CARDS; i++)
            {
				cardBitstrings[i] = bitstring;
				bitstring <<= 1; 
            }

			meldBitstrings = new List<List<long>>();
			meldBitstringToCardsMap = new Dictionary<long, List<Card>>();

			// build run meld lists
			for (int suit = 0; suit < Constants.NUM_SUITS; suit++)
            {
				for (int runRankStart = 1; runRankStart < Constants.NUM_RANKS - 1; runRankStart++)
				{
					List<long> bitstringList = new List<long>();
					List<Card> cards = new List<Card>();
					Card c = Card.GetCard(runRankStart, suit);
					cards.Add(c);
					long meldBitstring = cardBitstrings[c.GetCardId()];
					c = Card.GetCard(runRankStart + 1, suit);
					cards.Add(c);
					meldBitstring |= cardBitstrings[c.GetCardId()];
					for (int rank = runRankStart + 2; rank < Constants.NUM_RANKS + 1; rank++)
					{
						c = Card.GetCard(rank, suit);
						cards.Add(c);
						meldBitstring |= cardBitstrings[c.GetCardId()];
						bitstringList.Add(meldBitstring);
						List<Card> newList = new List<Card>();
						foreach (Card card in cards)
							newList.Add((Card)card.Clone());
						meldBitstringToCardsMap[meldBitstring] = newList;
					}

					meldBitstrings.Add(bitstringList);
				}
			}

			// build set meld lists
			for (int rank = 1; rank < Constants.NUM_RANKS + 1; rank++)
			{
				List<Card> cards = new List<Card>();
				for (int suit = 0; suit < Constants.NUM_SUITS; suit++)
					cards.Add(Card.GetCard(rank, suit));
				for (int suit = 0; suit <= Constants.NUM_SUITS; suit++)
				{
					List<Card> cardSet = new List<Card>();
					
					foreach (Card c in cards) 
						cardSet.Add((Card)c.Clone());
						

					if (suit < Constants.NUM_SUITS) 
					{
						for (int i = 0; i < cardSet.Count; i++)
                        {
							if ((int)cardSet[i].Suit == suit)
								cardSet.RemoveAt(i);
						}
					}

					List<long> bitstringList = new List<long>();
					long meldBitstring = 0L;

					foreach (Card card in cardSet)
						meldBitstring |= cardBitstrings[card.GetCardId()];

					bitstringList.Add(meldBitstring);
					meldBitstringToCardsMap[meldBitstring] = cardSet;
					meldBitstrings.Add(bitstringList);
				}
			}
		}

		public static List<Card> bitstringToCards(long bitstring)
		{
			List<Card> cards = new List<Card>();
			for (byte i = 0; i < Constants.NUM_CARDS; i++)
			{
				if (bitstring % 2 == 1)
					cards.Add(Card.allcards[i]);
				bitstring /= 2;
			}
			return cards;
		}

		public static long cardsToBitstring(List<Card> cards)
		{
			long bitstring = 0L;
			foreach (Card card in cards)
				bitstring |= cardBitstrings[card.GetCardId()];
			return bitstring;
		}

		/**
		* Given a list of cards, return a list of all meld bitstrings that apply to that list of cards
		* @param cards a list of cards
		* @return a list of all meld bitstrings that apply to that list of cards
		*/
		public static List<long> cardsToAllMeldBitstrings(List<Card> cards)
		{
			List<long> bitstringList = new List<long>();
			long cardsBitstring = cardsToBitstring(cards);
			foreach (List<long> meldBitstringList in meldBitstrings)
				foreach (long meldBitstring in meldBitstringList) { 
					if ((meldBitstring & cardsBitstring) == meldBitstring)
						bitstringList.Add(meldBitstring);
					else
						break;
				}
			return bitstringList;
		}

		/**
		* Given a list of cards, return a list of all lists of card melds that apply to that list of cards
		* @param cards a list of cards
		* @return a list of all lists of card melds that apply to that list of cards
		*/
		public static List<List<Card>> cardsToAllMelds(List<Card> cards)
		{
			List<List<Card>> meldList = new List<List<Card>>();
			foreach (long meldBitstring in cardsToAllMeldBitstrings(cards))
				meldList.Add(bitstringToCards(meldBitstring));

			return meldList;
		}

		/**
		* Given a list of cards, return a list of all card melds lists to which another meld cannot be added.
		* This corresponds to all ways one may maximally meld, although this doesn't imply minimum deadwood/cards in the sets of melds.
		* @param cards a list of cards
		* @return a list of all card melds lists to which another meld cannot be added
		*/
		public static List<List<List<Card>>> cardsToAllMaximalMeldSets(List<Card> cards)
		{
			List<List<List<Card>>> maximalMeldSets = new List<List<List<Card>>>();
			List<long> meldBitstrings = cardsToAllMeldBitstrings(cards);
			HashSet<HashSet<int>> closed = new HashSet<HashSet<int>>();
			Queue<HashSet<int>> queue = new Queue<HashSet<int>>();
			HashSet<int> allIndices = new HashSet<int>();
			for (int i = 0; i < meldBitstrings.Count; i++)
			{
				HashSet<int> meldIndexSet = new HashSet<int>();
				meldIndexSet.Add(i);
				allIndices.Add(i);
				queue.Enqueue(meldIndexSet);
			}
			while (queue.Count != 0)
			{
				HashSet<int> meldIndexSet = queue.Dequeue();
				
				if (closed.Contains(meldIndexSet))
					continue;
				long meldSetBitstring = 0L;
				foreach (int meldIndex in meldIndexSet)
					meldSetBitstring |= meldBitstrings[meldIndex];
				closed.Add(meldIndexSet);
				bool isMaximal = true;
				for (int i = 0; i < meldBitstrings.Count; i++)
				{
					if (meldIndexSet.Contains(i))
						continue;
					long meldBitstring = meldBitstrings[i];
					if ((meldSetBitstring & meldBitstring) == 0)
					{ // meld has no overlap with melds in set
						isMaximal = false;
						HashSet<int> newMeldIndexSet = new HashSet<int>();
						foreach (int n in meldIndexSet)
							newMeldIndexSet.Add(n);
						newMeldIndexSet.Add(i);
						queue.Enqueue(newMeldIndexSet);
					}
				}
				if (isMaximal)
				{
					List<List<Card>> cardSets = new List<List<Card>>();
					foreach (int meldIndex in meldIndexSet)
					{
						long meldBitstring = meldBitstrings[meldIndex];
						cardSets.Add(bitstringToCards(meldBitstring));
					}
					/*
					string meldsStr = "";
					foreach (List<Card> meld in cardSets)
						foreach (Card c in meld)
							meldsStr += c.Rank + " of " + c.Suit + ", ";
					Debug.Log(meldsStr);
					*/
					maximalMeldSets.Add(cardSets);
				}
			}
			return maximalMeldSets;
		}

		/**
		* Given a list of card melds and a hand of cards, return the unmelded deadwood points for that hand
		* @param melds a list of card melds
		* @param hand hand of cards
		* @return the unmelded deadwood points for that hand
		*/
		public static int getDeadwoodPoints(List<List<Card>> melds, List<Card> hand)
		{
			HashSet<Card> melded = new HashSet<Card>();
			foreach (List<Card> meld in melds)
				foreach (Card card in meld)
					melded.Add(card);
			int deadwoodPoints = 0;
			foreach (Card card in hand)
				if (!melded.Contains(card))
					deadwoodPoints += DEADWOOD_POINTS[((int)card.Rank)-1];
			return deadwoodPoints;
		}

		/**
		*Return the deadwood points for an individual given card.
		* @param card given card
		* @return the deadwood points for an individual given card
		*/
		public static int getDeadwoodPoints(Card card)
		{
			return DEADWOOD_POINTS[((int)card.Rank)-1];
		}

		/**
		* Return the deadwood points for a list of given cards.
		* @param cards list of given cards
		* @return the deadwood points for a list of given cards
		*/
		public static int getDeadwoodPoints(List<Card> cards)
		{
			int deadwood = 0;
			foreach (Card card in cards)
				deadwood += DEADWOOD_POINTS[((int)card.Rank)-1];
			return deadwood;
		}

		/**
		* Returns a list of list of melds that all leave a minimal deadwood count.
		* @param cards
		* @return a list of list of melds that all leave a minimal deadwood count
		*/
		// Note: This is actually a "weighted maximum coverage problem". See https://en.wikipedia.org/wiki/Maximum_coverage_problem
		public static List<List<List<Card>>> cardsToBestMeldSets(List<Card> cards)
		{
			int minDeadwoodPoints = Int32.MaxValue;
			List<List<List<Card>>> maximalMeldSets = cardsToAllMaximalMeldSets(cards);
			List<List<List<Card>>> bestMeldSets = new List<List<List<Card>>>();
			foreach (List<List<Card>> melds in maximalMeldSets)
			{
				int deadwoodPoints = getDeadwoodPoints(melds, cards);
				if (deadwoodPoints <= minDeadwoodPoints)
				{
					if (deadwoodPoints < minDeadwoodPoints)
					{
						minDeadwoodPoints = deadwoodPoints;
						bestMeldSets.Clear();
					}
					bestMeldSets.Add(melds);
				}
			}

			if (bestMeldSets.Count != 0) 
			{ 
				string meldsStr = "";
				List<List<Card>> bestmelds = bestMeldSets[0];
				foreach (List<Card> meld in bestmelds)
					foreach (Card c in meld)
						meldsStr += c.Rank + " of " + c.Suit + ", ";
				Debug.Log("best melds are " + meldsStr);
			}
			return bestMeldSets;
		}

		/**
	    * Return all meld bitstrings.
	    * @return all meld bitstrings
		*/
		public static List<long> getAllMeldBitstrings()
		{
			return new List<long>(meldBitstringToCardsMap.Keys);
		}
	}
}