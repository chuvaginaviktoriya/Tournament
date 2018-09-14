using System;
using System.Collections.Generic;

namespace Tournament
{
    [Serializable]
    public class DoubleEliminationTournament: SingleEliminationTournament
    {
        private enum GameStatus
        {
            UpperBracketGame,
            LowerBracketGame,
            LastGame
        }

        private List<Participant> _lowerBracketParticipants;
        private GameStatus _gameStatus;

        public DoubleEliminationTournament(List<string> participants) : base(participants)
        {
            _lowerBracketParticipants = new List<Participant>();
        }

        public new Participant GetPlayingParticipants()
        {
            var isLastRoundEnded = RoundBracket == null || GameIndex >= RoundBracket.Count / 2;

            if (isLastRoundEnded)
                OrganizeRound();

            var meeting = GetPlayingMeeting();

            return meeting;
        }

        private Participant GetPlayingMeeting()
        {
            if (_gameStatus == GameStatus.LowerBracketGame)
                return _lowerBracketParticipants[GameIndex];
            else
                return UpperBracketParticipants[GameIndex];
        }

        public new void PlayGame(Side winnerSide)
        {
            string loser = DetectLoser(winnerSide);

            if (_gameStatus == GameStatus.LastGame)
                SetWinnersName(UpperBracketParticipants[GameIndex], winnerSide);
            else if (_gameStatus == GameStatus.UpperBracketGame)
            {
                SetWinnersName(UpperBracketParticipants[GameIndex], winnerSide);
                AddLoserToLowerBracket(winnerSide, loser);
            }
            else if (_gameStatus == GameStatus.LowerBracketGame)
                SetWinnersName(_lowerBracketParticipants[GameIndex], winnerSide);
            else
                return;

            GameIndex++;
            BinarySaver.SaveDoubleToBinnary(this);
        }

        private string DetectLoser(Side side)
        {
            if (side == Side.Left)
                return UpperBracketParticipants[GameIndex].Right.Name;
            if (side == Side.Right)
                return UpperBracketParticipants[GameIndex].Left.Name;

            throw new Exception("Side of winner is not set");
        }

        private void AddLoserToLowerBracket(Side side, string loser)
        {
            if (_lowerBracketParticipants?.Count < GameIndex * 2)
                _lowerBracketParticipants?.Insert(GameIndex, new Participant(loser));
            else
                _lowerBracketParticipants?.Insert(GameIndex * 2, new Participant(loser));
        }

        private void OrganizeRound()
        {
            if (UpperBracketParticipants.Count == 1 && _lowerBracketParticipants.Count == 1)
            {
                UpperBracketParticipants.Add(_lowerBracketParticipants[0]);
                _lowerBracketParticipants.RemoveAt(0);
                OrganizeRound(ref UpperBracketParticipants);
                _gameStatus = GameStatus.LastGame;
            }
            else if (UpperBracketParticipants.Count > _lowerBracketParticipants.Count)
            {
                OrganizeRound(ref UpperBracketParticipants);
                _gameStatus = GameStatus.UpperBracketGame;
            }
            else
            {
                OrganizeRound(ref _lowerBracketParticipants);
                _gameStatus = GameStatus.LowerBracketGame;
            }
        }

        public List<Participant> GetLowerBracket()
        {
            return new List<Participant>(_lowerBracketParticipants);
        }

        public override bool EndOfTheGame()
        {
            return base.EndOfTheGame() && _lowerBracketParticipants.Count==0;
        }

    }
}
