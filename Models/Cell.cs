using Models.Enums;

namespace Models
{
    public class Cell
    {
        public string Value;
        public CellState State = CellState.Incorrect;
        public string[] CssClasses
        {
            get
            {
                if (State == CellState.Correct)
                    return ["bg-green-500"];
                else if (State == CellState.WrongPosition)
                    return ["bg-yellow-500"];
                else
                    return ["bg-white-500"];
            }
        }
        public Cell()
        {
        }
    }

}