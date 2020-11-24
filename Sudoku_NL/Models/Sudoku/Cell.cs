using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sudoku_NL.Models
{
    public class Cell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public List<int> Possible = new List<int>();
        public string PossibleString
        {
            get
            {
                var str = String.Empty;
                foreach (int val in Possible)
                {
                    str += val.ToString() + ",";
                }
                if (str != String.Empty)
                {
                    return str.Remove(str.Length - 1, 1);
                }
                else
                {
                    return str;
                }

            }
        }
        private int _value;

        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }
        public string Id
        {
            get
            {
                return Row.ToString() + "_" + Column.ToString();
            }
        }
        public bool ReadOnly { get; set; }

        public Cell(Cell source)
        {
            CopyCell(source);
        }

        public Cell()
        {
            for (int i = 1; i <= 9; i++)
            {
                Possible.Add(i);
            }
        }

        public void CopyCell(Cell source)
        {
            Possible = new List<int>(source.Possible);
            Value = source.Value;
            Row = source.Row;
            Column = source.Column;
            ReadOnly = source.ReadOnly;
        }
    }
}
