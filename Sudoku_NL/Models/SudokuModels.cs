using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sudoku_NL.Models
{
    public class SudokuModels
    {
        private static Sudoku sudokuGame;

        public static Sudoku SudokuGame
        {
            get
            {
                if (sudokuGame == null)
                {
                    sudokuGame = new Sudoku();
                   
                    sudokuGame.NewGame(50);
                }
                return sudokuGame;
            }
        }
    }
}