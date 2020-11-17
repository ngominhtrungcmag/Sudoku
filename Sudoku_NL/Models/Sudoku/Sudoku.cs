using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Globalization;

namespace Sudoku_NL.Models
{
    public class Sudoku
    {
        public Cell[,] Cells = new Cell[9, 9];
        public Cell[,] CellsRoot = new Cell[9, 9];
        public Cell[,] Result = new Cell[9, 9];
        
        public Cell SelectedCell = new Cell();
        public Sudoku solution;
        public Sudoku nextSudoku;
        public bool Completed = false;
        int NumCells;
        int Difficult = 30;
        int countResult = 0;
        public bool Creating = false;

        public Sudoku()
        {
            Clear();
            ClearResult();
            ClearCellsRoot();
            //DifficultyCells.Add(Difficulty.Easy, 30);
            //DifficultyCells.Add(Difficulty.Medium, 40);
            //DifficultyCells.Add(Difficulty.Hard, 50);
        }

        public Sudoku(Sudoku source)
        {
            CopyCells(source);
        }

        private void CopyCells(Sudoku source)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j] == null)
                    {
                        Cells[i, j] = new Cell();
                    }
                    Cells[i, j].CopyCell(source.Cells[i, j]);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(CellsRoot[i, j].Value + " ");
                    Cells[i, j].CopyCell(CellsRoot[i, j]);
                }
                Console.WriteLine();
            }
        }
        //Load sudoku
        public void LoadFile(string input)
        {
            // List<string> list = new List<string>();
            Clear();

            char[] unwanted = new char[] { ' ' };
            try
            {

                // Open the text file using a stream reader
                using (var reader = new StreamReader(input))
                {
                    int row = 0;

                    string line = reader.ReadLine();

                    while (line != null)
                    {
                        int column = 0;
                        string[] numbersString = line.Split(unwanted);
                        foreach (string a in numbersString)
                        {
                            var number = a.Trim();
                            if (number != "" && Int32.Parse(number) != 0)
                            {
                                Cells[row, column].Value = Int32.Parse(number);
                                Cells[row, column].ReadOnly = true;
                            }
                            column++;
                        }
                        line = reader.ReadLine();
                        row++;
                    }
                }
                UpdatePossible();
            }
            catch (IOException e)
            {
                Console.WriteLine("Không thể đọc file này :");
                Console.WriteLine(e.Message);
            }

            SetReadOnly();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                 
                    CellsRoot[i, j].CopyCell(Cells[i, j]);
                }
            }
        }

        //New sudoku
        public void NewGame(int difficult)
        {
            Difficult = difficult;
            CreateSudoku();
          
            Console.WriteLine("After NewGame....");

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {

                    Console.Write(Cells[i, j].Value + " ");
                }
                Console.WriteLine();
            }
            UpdatePossible();
            SetReadOnly();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    CellsRoot[i, j].CopyCell(Cells[i, j]);
                }
            }
        }
        public void CreateNextSudoku(bool useThread)
        {
            nextSudoku = new Sudoku();

            if (useThread)
            {
                nextSudoku.CreateSudokuThread();
            }
            else
            {
                nextSudoku.CreateSudoku();
            }
        }
        public void SetReadOnly()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].ReadOnly = (Cells[i, j].Value != 0);
                }
            }
        }

        public void CreateSudokuThread()
        {
            Thread thread = new Thread(CreateSudoku);
            thread.Start();
        }

        public void CreateSudoku()
        {
            Clear();
            Completed = false;
            Creating = true;

            CreateRandom();

            solution = new Sudoku(this);
        }

        public bool IsValidPossibles(int row)
        {
            bool valid = true;
            for (int j = 0; j < 9; j++)
            {
                if (Cells[row, j].Possible.Count == 0 && Cells[row, j].Value == 0)
                {
                    valid = false;
                }
            }
            return valid;
        }


        public void UpdatePossible()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].Possible = GetFullList();
                    for (int k = 1; k <= 9; k++)
                    {
                        if (!feasible(Cells, i, j, k))
                        {
                            Cells[i, j].Possible.Remove(k);
                        }
                    }
                }
            }


        }

        //public void UpdateRowPossibles(int row)
        //{
        //    List<int> rowValues = new List<int>();

        //    for (int i = 0; i < 9; i++)
        //    {
        //        if (Cells[row, i].Value > 0)
        //        {
        //            rowValues.Add(Cells[row, i].Value);
        //        }
        //    }

        //    for (int i = 0; i < 9; i++)
        //    {
        //        //Cells[row, i].Possibles = GetFullList();
        //        Cells[row, i].Possible.RemoveAll(m => rowValues.Contains(m) && m != Cells[row, i].Value);

        //        //Console.Write("Row " + row + " [ " + row + i + "] :");
        //        //Cells[row, i].Possible.ForEach(Console.Write);
        //        //Console.WriteLine();
        //        //Cells [row, i].Highlight = (SelectedCell != null && SelectedCell.Row == row);
        //    }
        //}

        //public void UpdateColumnPossibles(int row)
        //{
        //    List<int> rowValues = new List<int>();

        //    for (int i = 0; i < 9; i++)
        //    {
        //        if (Cells[i, row].Value > 0)
        //        {
        //            rowValues.Add(Cells[i, row].Value);
        //        }

        //    }

        //    for (int i = 0; i < 9; i++)
        //    {
        //        //Cells[i, row].Possibles = GetFullList();
        //        Cells[i, row].Possible.RemoveAll(m => rowValues.Contains(m) && m != Cells[i, row].Value);

        //        //Console.Write("Col " + row + " [ " + i + row + "] :");
        //        //Cells[i, row].Possible.ForEach(Console.Write);
        //        //Console.WriteLine();
        //        //Cells[i, row].Highlight = (SelectedCell != null && SelectedCell.Column == row);
        //    }
        //}

        //public void UpdateBoxPossibles(int box)
        //{
        //    for (int j = 0; j < 9; j++)
        //    {
        //        for (int k = 0; k < 9; k++)
        //        {
        //            List<int> boxValues = GetValuesInBox(Cells[j, k].Box);
        //            Cells[j, k].Possible.RemoveAll(m => boxValues.Contains(m) && m != Cells[j, k].Value);

        //            //Console.Write("box " + box + " [ " + j + k + "] :");
        //            //Cells[j, k].Possible.ForEach(Console.Write);
        //            //Console.WriteLine();
        //        }
        //    }
        //}


        //private List<int> GetValuesInBox(int box)
        //{
        //    List<int> boxValues = new List<int>();

        //    for (int i = 0; i < 9; i++)
        //    {
        //        for (int j = 0; j < 9; j++)
        //        {
        //            if (Cells[i, j].Box == box && Cells[i, j].Value != 0)
        //            {
        //                boxValues.Add(Cells[i, j].Value);
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }

        //    return boxValues;
        //}


        public void Clear()
        {

            Completed = false;
            NumCells = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Cells[i, j] == null)
                    {
                        Cells[i, j] = new Cell();
                    }
                    Cells[i, j].Row = i;
                    Cells[i, j].Column = j;
                    Cells[i, j].Box = GetBoxNum(i, j);
                    Cells[i, j].Value = 0;
                    Cells[i, j].ReadOnly = false;
                    Cells[i, j].Possible = GetFullList();
                }
            }
        }
        public void ClearCellsRoot()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (CellsRoot[i, j] == null)
                    {
                        CellsRoot[i, j] = new Cell();
                    }
                    CellsRoot[i, j].Row = i;
                    CellsRoot[i, j].Column = j;
                    CellsRoot[i, j].Box = GetBoxNum(i, j);
                    CellsRoot[i, j].Value = 0;
                    CellsRoot[i, j].ReadOnly = false;
                    CellsRoot[i, j].Possible = GetFullList();
                }
            }
        }

        public void ClearResult()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Result[i, j] == null)
                    {
                        Result[i, j] = new Cell();
                    }
                    Result[i, j] = new Cell();
                    Result[i, j].Box = GetBoxNum(i, j);
                    Result[i, j].Column = j;
                    Result[i, j].Row = i;
                    Result[i, j].Value = 0;
                    Result[i, j].ReadOnly = false;
                    Result[i, j].Possible = GetFullList();
                }
            }
        }

        public bool CheckOneCell(int i, int j)
        {
            bool valid = true;
            SolveSudoku(Cells, 0, 0);

            if ((Cells[i,j].Value != 0) && (Cells[i,j].Value != Result[i, j].Value))
            {
                valid = false;
            }
            return valid;
        }

        public void CreateRandom()
        {
            countResult = 0;
            Console.WriteLine("CreateRandom....");

            FillDiagonal();
            Console.WriteLine("Afeter FillDiagonal");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(Cells[i, j].Value + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("SolveSudoku...");

            ClearResult();
            SolveSudoku(Cells, 0, 0);


            Console.WriteLine("Afeter SolveSudoku");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(Result[i, j].Value + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Cells remove");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].CopyCell(Result[i, j]);
                    //    Console.Write(Result[i, j].Value + " ");
                }
                //  Console.WriteLine();
            }

            Console.WriteLine("Remove Cells....");
            NumCells = 0;
            Console.WriteLine("Form model difficult: " + Difficult);
            while (NumCells <= Difficult)
            {
                //Console.WriteLine("NumCells: " + NumCells);
                Random ranRow = new Random();
                Random ranCol = new Random();
                int indexRow = ranRow.Next(0, 9);
                int indexCol = ranCol.Next(0, 9);
                //   Console.WriteLine(Cells[indexRow, indexCol].Value);
                if (Cells[indexRow, indexCol].Value != 0)
                {
                    NumCells++;
                    Cells[indexRow, indexCol].Value = 0;
                    UpdatePossible();
                }
            }
            Console.WriteLine("Completed");
            Completed = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    CellsRoot[i, j].CopyCell(Cells[i, j]);
                }
            }

        }

        public void FillDiagonal()
        {
            Console.WriteLine("Run FillDiagonal...");
            Clear();
            
            for (int k = 0; k < 9; k = k + 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        do
                        {
                            if (Cells[k + i, k + j].Possible.Count > 0)
                            {
                                Random rand = new Random();
                                int index = rand.Next(0, Cells[k + i, k + j].Possible.Count);
                                Cells[k + i, k + j].Value = Cells[k + i, k + j].Possible[index];
                                UpdatePossible();
                            }

                        }
                        while (feasible(Cells, k + i, k + j, Cells[k + i, k + j].Value));  
                    }
                }
            }    
        }
        //List of 1 - 9 
        private List<int> GetFullList()
        {
            return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
        public string GetSudokuString()
        {
            string str = String.Empty;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    str += Cells[i, j].Value.ToString() + "|";
                }
            }
            return str;
        }

        //Này quan trọng nè: giải thuật giải sudoku

        public void SolveGame()
        {
            countResult = 0;
            Console.WriteLine("-----Giai game ne--------");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j ++)
                {
                    Console.Write(Cells[i, j].Value + " ");
                }
                Console.WriteLine();
            }
            ClearResult();
            SolveSudoku(Cells, 0, 0);
            Console.WriteLine("AfterSolveGame...");
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].CopyCell(Result[i, j]);
                    Console.Write(Cells[i, j].Value + " ");
                }
                Console.WriteLine();
            }
        }


        public void SolveSudoku(Cell[,] CellsSolve, int x, int y)
        {
            if (countResult > 0)
            {
                return;
            }    
            else if (y == 9)
            {
                if (x == 8)
                {
                    countResult++;
                    // Console.WriteLine("Solution");
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            Result[i, j].Value = CellsSolve[i, j].Value;
                            //Console.Write(Result[i, j].Value + " ");
                        }
                        //Console.WriteLine("");
                    }
                    // System.Environment.Exit(0);
                    return;
                }
                else
                {
                    SolveSudoku(CellsSolve, x + 1, 0);
                }
            }
            else if (CellsSolve[x, y].Value == 0)
            {
                int k = 0;
                for (k = 1; k <= 9; k++)
                {
                    if (feasible(CellsSolve, x, y, k))
                    {
                        CellsSolve[x, y].Value = k;
                        SolveSudoku(CellsSolve, x, y + 1);
                        CellsSolve[x, y].Value = 0;
                    }
                }
            }
            else
            {
                SolveSudoku(CellsSolve, x, y + 1);
            }
            //  Console.WriteLine(" End SolveSudoku" + x + " " + y);
        }

        //public void SolveSudoku(int x, int y)
        //{
        //    if (countResult > 0)
        //    {
        //        return;
        //    }
        //    else if (y == 9)
        //    {
        //        if (x == 8)
        //        {
        //            countResult++;
        //            //Console.WriteLine("Solution");
        //            for (int i = 0; i < 9; i++)
        //            {
        //                for (int j = 0; j < 9; j++)
        //                {
        //                      Result[i, j].Value = Cells[i, j].Value;
        //                     // Console.Write(Cells[i, j].Value + " ");
        //                }
        //                Console.WriteLine("");
        //            }
        //            // System.Environment.Exit(0);
        //            return;
        //        }
        //        else
        //        {
        //            SolveSudoku(x + 1, 0);
        //        }
        //    }
        //    else if (Cells[x, y].Value == 0)
        //    {
        //        //for (int k = 1; k <= 9; k++)
        //        foreach (int k in Cells[x,y].Possible)
        //        {
        //            if (feasible(Cells, x, y, k))
        //            {
        //                Cells[x, y].Value = k;
        //                UpdatePossible();
        //                SolveSudoku(x, y + 1);
        //                Cells[x, y].Value = 0;
        //                UpdatePossible();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        SolveSudoku(x, y + 1);
        //    }
        //    //  Console.WriteLine(" End SolveSudoku" + x + " " + y);
        //}

        public bool feasible(Cell[,] CellsSolve, int x, int y, int k)
        {
            int i = 0, j = 0;
            
            for (i = 0; i < 9; i++)
            {
                if (CellsSolve[x,i].Value == k) return false;
            }

            for (i = 0; i < 9; i++)
            {
                if (CellsSolve[i,y].Value == k) return false;
            }
            
            int a = x / 3, b = y / 3;
            for (i = 3 * a; i < 3 * a + 3; i++)
            {
                for (j = 3 * b; j < 3 * b + 3; j++)
                {
                    if (CellsSolve[i,j].Value == k) return false;
                }
            }
            
            return true;
        }
        //Get box num
        private int GetBoxNum(int i, int j)
        {
            int box = 0;
            if (i < 3 && j < 3)
            {
                box = 1;
            }
            else if (i >= 3 && i < 6 && j < 3)
            {
                box = 2;
            }
            else if (i >= 6 && j < 3)
            {
                box = 3;
            }
            else if (i < 3 && j >= 3 && j < 6)
            {
                box = 4;
            }
            else if (i >= 3 && i < 6 && j >= 3 && j < 6)
            {
                box = 5;
            }
            else if (i >= 6 && j >= 3 && j < 6)
            {
                box = 6;
            }
            else if (i < 3 && j >= 6)
            {
                box = 7;
            }
            else if (i >= 6 && j >= 3 && j < 6)
            {
                box = 8;
            }
            else if (i >= 6 && j >= 6)
            {
                box = 9;
            }

            return box;
        }

    }

}
