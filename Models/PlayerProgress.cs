using Models.Enums;

namespace Models {
    public class PlayerProgress {
        public string PlayerUsername {get; set;}
        public List<CellState> CellStates;
        public PlayerProgress(string playerUsername, List<CellState> cellStates)
        {
            PlayerUsername = playerUsername;
            CellStates = cellStates;
        }

        public string CellCssClasses(CellState cellState) {
            if (cellState == CellState.Correct) {
                return "bg-green-500";
            } else if (cellState == CellState.WrongPosition) {
                return "bg-yellow-500";
            }
            return "bg-white-500";
        }
    }
}