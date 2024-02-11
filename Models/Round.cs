namespace Models {
    public class Round {
        public int Number {get; private set;}
        public string SolutionWord {get; private set;}
        public string Winner {get; private set;}
        public int TotalSeconds {get; private set;}
        public Round(int number, string solutionWord, string winner, int totalSeconds) {
            Number = number;
            SolutionWord = solutionWord;
            Winner = winner;
            TotalSeconds = totalSeconds;
        }
    }
}