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

        //Khởi tạo Sudoku
        public Sudoku()
        {
            //Cài dặt giá trị ban đầu cho các Cell Sudoku.
            Clear();
            ClearResult();
            ClearCellsRoot();
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

        //Khôi phục trạng thái ban đầu của Sudoku
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
        //Load sudoku từ file có sẵn
        public void LoadFile(string input)
        {
            Clear();

            char[] unwanted = new char[] { ' ' };
            try
            {
                // Mở file sử dụng stream reader
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

        //Trò chơi Sudoku mới.
        public void NewGame(int difficult)
        {
            Difficult = difficult;
            CreateSudoku();

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


        public void CreateSudoku()
        {
            Clear();
            Completed = false;
            Creating = true;
            CreateRandom();
            solution = new Sudoku(this);
        }

        //Cập nhật giá trị có thể điền vào ô số Sudoku
        public void UpdatePossible()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].Possible = GetFullList();
                    for (int k = 1; k <= 9; k++)
                    {
                        if (!Feasible(Cells, i, j, k))
                        {
                            Cells[i, j].Possible.Remove(k);
                        }
                    }
                }
            }
        }

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
                    Cells[i, j].Value = 0;
                    Cells[i, j].ReadOnly = false;
                    Cells[i, j].Hightlight = false;
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
                    CellsRoot[i, j].Value = 0;
                    CellsRoot[i, j].ReadOnly = false;
                    CellsRoot[i, j].Hightlight = false;
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
                    Result[i, j].Column = j;
                    Result[i, j].Row = i;
                    Result[i, j].Value = 0;
                    Result[i, j].ReadOnly = false;
                    Result[i, j].Hightlight = false;
                    Result[i, j].Possible = GetFullList();
                }
            }
        }

        public bool CheckOneCell(int i, int j)
        {
            bool valid = true;
            countResult = 0;
            SolveSudoku(Cells, 0, 0);

            if ((Cells[i, j].Value != 0) && (Cells[i, j].Value != Result[i, j].Value) | (Result[i,j].Value ==0))
            {
                valid = false;
            }
            return valid;
        }


        public void CreateRandom()
        {
            //Đáp án gán = 0 
            countResult = 0;
            //Điền các số vào các ma trận 3x3 theo đường chéo (ma trận thứ 1-5-9)
            FillDiagonal();
            //Clear result để cập nhật kết quả
            ClearResult();
            //Chạy giải Sudoku để fill 81 ô
            SolveSudoku(Cells, 0, 0);
            //Gán Cells chính = Result
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j].CopyCell(Result[i, j]);
                }
            }

            NumCells = 0;//Đếm số ô cần giữ lại theo mức độ khó
            while (NumCells <= Difficult)
            {
                Random ranRow = new Random();
                Random ranCol = new Random();
                int indexRow = ranRow.Next(0, 9);
                int indexCol = ranCol.Next(0, 9);
                if (Cells[indexRow, indexCol].Value != 0)
                {
                    NumCells++;
                    Cells[indexRow, indexCol].Value = 0;
                    UpdatePossible();
                }
            }
            Completed = true;
            //Cuối cùng gán CellsRoot để dùng lại sao này
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    CellsRoot[i, j].CopyCell(Cells[i, j]);
                }
            }
        }

        //Sinh ngẫu nhiên các số trong ma trận 3x3 theo đường chéo 1-5-9
        public void FillDiagonal()
        {
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
                        while (Feasible(Cells, k + i, k + j, Cells[k + i, k + j].Value));
                    }
                }
            }
        }

        //List of 1 - 9 
        private List<int> GetFullList()
        {
            return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        //Hàm giải game Sudoku
        public bool SolveGame()
        {
            //Đặt biến đếm = 0
            countResult = 0;
            bool validResult = true;
            //Xóa result 
            ClearResult();
            //Gọi hàm SolveSudoku
            SolveSudoku(Cells, 0, 0);

            //Kiểm tra xem nếu có ô nào có giá trị = 0 thì hàm sẽ có giá trị false
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Result[i, j].Value == 0)
                    {
                        validResult = false;
                        break;
                    }
                }
            }
            //Nếu validResult = true tức là có đáp án thì gán đáp án cho Cells 
            if (validResult)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        Cells[i, j].Value = Result[i, j].Value;
                        UpdatePossible();
                    }
                }
            }
            else
            {
                return validResult;
            }
            return validResult;
        }

        //Giải sudoku backtracking
        public void SolveSudoku(Cell[,] CellsSolve, int x, int y)
        {
            //Nếu có 1 đáp án rồi thì dừng 
            if (countResult > 0)
            {
                return;
            }
            else if (y == 9)
            {
                if (x == 8)
                {
                    //Return đáp án và gán countResult để dừng không tìm đáp án khác nữa.
                    countResult++;
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            Result[i, j].Value = CellsSolve[i, j].Value;
                        }
                    }
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
                // Chạy từ 1 - 9 
                for (k = 1; k <= 9; k++)
                {
                    //Nếu giá trị K hợp lệ thì gán cho đáp án
                    if (Feasible(CellsSolve, x, y, k))
                    {
                        CellsSolve[x, y].Value = k;
                        //Đi tiếp đến ô thứ x, y+1
                        SolveSudoku(CellsSolve, x, y + 1);
                        //Quay lui lại gán giá trị ô x,y = 0;
                        CellsSolve[x, y].Value = 0;
                    }
                }
            }
            //Nếu ô x, y có giá trị rồi thì gọi tiếp ô x, y+1
            else
            {
                SolveSudoku(CellsSolve, x, y + 1);
            }
        }

        //Kiểm tra game
        public bool CheckComplete()
        {
            List<int> CheckList = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                CheckList = GetFullList();
                for (int j=0; j <9; j++)
                {
                    if (CheckList.Contains(Cells[i,j].Value))
                    {
                        CheckList.Remove(Cells[i, j].Value);
                    }
                    else
                    {
                        return false;
                    }
                }
                CheckList = GetFullList();
                for (int j = 0; j < 9; j++)
                {
                    if (CheckList.Contains(Cells[j, i].Value))
                    {
                        CheckList.Remove(Cells[j, i].Value);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            //Check box
            //Kiểm tra 9 ma trận 3x3
            for (int i = 2; i < 9; i += 3)
            {
                for (int j = 2; j < 9; j += 3)
                {
                    CheckList = GetFullList();
                    for (int k = i-2; k <=i; k++)
                    {
                        for (int l = j-2; l<=j; l++)
                        {
                            if (CheckList.Contains(Cells[k, l].Value))
                            {
                                CheckList.Remove(Cells[k, l].Value);
                            }
                            else return false;
                        }
                    }
                }
            }
            return true;
        }

        //Kiểm tra giá trị hợp lệ
        public bool Feasible(Cell[,] CellsSolve, int x, int y, int k)
        {
            int i = 0, j = 0;
            //Kiểm tra hàng thứ x           
            for (i = 0; i < 9; i++)
            {
                if (CellsSolve[x, i].Value == k) return false;
            }
            //Kiểm tra cột thứ y
            for (i = 0; i < 9; i++)
            {
                if (CellsSolve[i, y].Value == k) return false;
            }
            //Kiểm tra ma trận 3x3
            int a = x / 3, b = y / 3;
            for (i = 3 * a; i < 3 * a + 3; i++)
            {
                for (j = 3 * b; j < 3 * b + 3; j++)
                {
                    if (CellsSolve[i, j].Value == k) return false;
                }
            }
            return true;
        }
    }
}
