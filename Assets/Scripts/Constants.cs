using System.ComponentModel;
using UnityEngine;

namespace UnityGinRummy
{
    public static class Constants
    {
        public const string CARD_BACK = "Card_Back";
        public const int INITIAL_CARDS = 21;
        public const float CARD_MOVEMENT_SPEED = 30.0f;
        public const float CARD_ROTATION_SPEED = 8f;
        public const float CARD_SNAP_DISTANCE = 0.01f;
        public const float DECK_CARD_POSITION_OFFSET = 0.005f;
        public const float PLAYER_CARD_POSITION_OFFSET = 1.0f;
        public const float SELECTED_CARD_OFFSET = 0.5F;
        public const byte NO_MORE_CARDS = 255;
        public const int NUM_RANKS = 13;
        public const int NUM_SUITS = 4;
        public const int NUM_CARDS = NUM_RANKS * NUM_SUITS;
    }

    public enum Suits
    {
        NoSuits = -1,
        spades = 0,
        clubs = 1,
        diamonds = 2,
        hearts = 3,
    }

    public enum Ranks
    {
        [Description("No Ranks")]
        NoRanks = -1,
        [Description("A")]
        Ace = 1,
        [Description("2")]
        Two = 2,
        [Description("3")]
        Three = 3,
        [Description("4")]
        Four = 4,
        [Description("5")]
        Five = 5,
        [Description("6")]
        Six = 6,
        [Description("7")]
        Seven = 7,
        [Description("8")]
        Eight = 8,
        [Description("9")]
        Nine = 9,
        [Description("10")]
        Ten = 10,
        [Description("J")]
        Jack = 11,
        [Description("Q")]
        Queen = 12,
        [Description("K")]
        King = 13,
    }
}