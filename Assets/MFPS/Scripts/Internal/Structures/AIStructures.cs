using System;

namespace MFPS.Runtime.AI
{
    [Serializable]
    public enum BotGameState : byte
    {
        Playing = 0,
        Death,
        Replaced,
        WaitingNextRound,
    }

    [Serializable]
    public class MFPSBotProperties
    {
        public string Name;
        public BotGameState GameState;
        public int Kills;
        public int Deaths;
        public int Score;
        public Team Team;
        public int ViewID;
    }
}