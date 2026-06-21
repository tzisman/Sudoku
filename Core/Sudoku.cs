// נוצר על ידי Tamar — לוגיקת Sudoku (העתק מדויק, ללא שינוי בפונקציות)

namespace Sudoku
{
    internal record SudokuGame(int SudokuRow, int SudokuNum, int SudokuColumn)
    {
        const int SUDOKU_SIZE = 9;

        readonly private int[,] mat = new int[SUDOKU_SIZE, SUDOKU_SIZE];
        private int sudokuRow;
        private int sudokuColumn;
        private int sudokuNum;
        private int counter;
        private bool isError = false;

        readonly private Random random = new();

        public int[,] Mat { get; set; } = new int[SUDOKU_SIZE, SUDOKU_SIZE];

        public void PrintSudoku(int[,] matrix)
        {
            for (int i = 0; i < SUDOKU_SIZE; i++)
            {
                for (int j = 0; j < SUDOKU_SIZE; j++)
                {
                    if (j == SUDOKU_SIZE - 1)
                    {
                        if (i == 2 || i == 5)
                        {
                            Console.WriteLine(matrix[i, j]);
                            Console.WriteLine("_____________________");
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine(matrix[i, j]);
                        }
                    }
                    else if (j % 3 == 2)
                    {
                        Console.Write(matrix[i, j] + " | ");
                    }
                    else
                    {
                        Console.Write(matrix[i, j] + " ");
                    }
                }

            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            counter++;
        }

        public bool IsInColumn(int num, int column)
        {
            for (int i = 0; i < 9; i++)
            {
                if (mat[i, column] == num)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInRow(int num, int row)
        {
            for (int i = 0; i < 9; i++)
            {
                if (mat[row, i] == num)
                {
                    return true;
                }
            }
            return false;
        }

        static public int GetIndexColumn(int num)
        {
            if (num % 3 == 0)
            {
                return num;
            }
            if (num % 3 == 1)
            {
                return num - 1;
            }
            return num - 2;
        }

        static public int GetIndexRow(int num)
        {
            if (num % 3 == 0)
            {
                return num;
            }
            if (num % 3 == 1)
            {
                return num - 1;
            }
            return num - 2;
        }

        public bool IsInSquare(int num, int column, int row)
        {
            for (int i = GetIndexRow(row); i < GetIndexRow(row) + 3; i++)
            {
                for (int j = GetIndexColumn(column); j < GetIndexColumn(column) + 3; j++)
                {
                    if (mat[i, j] == num)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CheckRow(int row)
        {
            int count;
            for (int i = 1; i < 10; i++)
            {
                count = 0;
                if (!IsInRow(i, row))
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (!IsInColumn(i, j) && !IsInSquare(i, j, row) && mat[row, j] == 0)
                        {
                            count++;
                            sudokuColumn = j;
                        }
                    }
                    if (count == 1)
                    {
                        counter++;
                        mat[row, sudokuColumn] = i;
                        //PrintSudoku(mat);
                    }
                    if (count == 0)
                    {
                        isError = true;
                    }
                }
            }
        }

        public void CheckColumn(int column)
        {
            int count;
            for (int i = 1; i < 10; i++)
            {
                count = 0;
                if (!IsInColumn(i, column))
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (!IsInRow(i, j) && !IsInSquare(i, column, j) && mat[j, column] == 0)
                        {
                            count++;
                            sudokuRow = j;
                        }
                    }
                    if (count == 1)
                    {
                        counter++;
                        mat[sudokuRow, column] = i;
                    }
                    if (count == 0)
                    {
                        isError = true;
                    }
                }
            }
        }

        public void CheckSquare(int row, int column)
        {
            column = GetIndexColumn(column);
            row = GetIndexRow(row);
            int count;
            for (int i = 1; i < 10; i++)
            {
                count = 0;
                if (!IsInSquare(i, column, row))
                {
                    for (int j = row; j < row + 3; j++)
                    {
                        for (int k = column; k < column + 3; k++)
                        {
                            if (!IsInRow(i, j) && !IsInColumn(i, k) && mat[j, k] == 0)
                            {
                                count++;
                                sudokuRow = j;
                                sudokuColumn = k;
                                sudokuNum = i;
                            }
                        }
                    }
                    if (count == 1)
                    {
                        if (count == 1)
                        {
                            counter++;
                            mat[sudokuRow, sudokuColumn] = i;
                        }
                        if (count == 0)
                        {
                            isError = true;
                        }
                    }
                }
            }
        }

        public void CheckNum(int row, int column)
        {
            int count = 0;
            if (mat[row, column] == 0)
            {
                for (int i = 1; i < 10; i++)
                {
                    if (!IsInRow(i, row) && !IsInSquare(i, column, row) && !IsInColumn(i, column))
                    {
                        count++;
                        sudokuNum = i;
                    }
                }
                if (count == 1)
                {
                    counter++;
                    mat[row, column] = sudokuNum;
                }
                if (count == 0)
                {
                    isError = true;
                }

            }

        }



        public bool IsFool()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (mat[i, j] == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public bool SolveSudoku()
        {
            counter = 1;
            while (counter != 0)
            {
                counter = 0;
                for (int i = 0; i < 9; i++)
                {
                    CheckRow(i);
                }

                for (int i = 0; i < 9; i++)
                {
                    CheckColumn(i);
                }

                for (int i = 0; i < 9; i += 3)
                {
                    for (int j = 0; j < 9; j += 3)
                    {
                        CheckSquare(i, j);
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        CheckNum(i, j);
                    }
                }


            }
            //PrintSudoku(mat);
            return IsFool();
        }

        public int[,] ReturnReplace(int[,] matrix)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    matrix[i, j] = mat[i, j];
                }
            }
            return matrix;
        }

        public void Replace(int[,] matrix)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    mat[i, j] = matrix[i, j];
                }
            }
        }

        public bool FixSudoku()
        {
            int[,] matrix = new int[SUDOKU_SIZE, SUDOKU_SIZE];
            int num;
            matrix = ReturnReplace(matrix);

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (mat[i, j] != 0)
                    {
                        num = mat[i, j];
                        mat[i, j] = 0;
                        if (SolveSudoku())
                        {
                            matrix[i, j] = 0;
                            Replace(matrix);
                        }
                        else
                        {
                            matrix[i, j] = num;
                            Replace(matrix);
                        }
                    }
                }
            }
            bool flag = SolveSudoku();
            Replace(matrix);
            PrintSudoku(mat);
            return flag;
        }

        public bool CreateSudoku()
        {
            bool flag = false;
            int[,] matrix = new int[SUDOKU_SIZE, SUDOKU_SIZE];
            Replace(matrix);
            while (!flag)
            {
                int row = random.Next(0, 9);
                int column = random.Next(0, 9);
                int num = random.Next(1, 9);
                if (matrix[row, column] == 0 && !IsInColumn(num, column) && !IsInRow(num, row) && !IsInSquare(num, column, row))
                {
                    matrix[row, column] = num;
                    mat[row, column] = num;
                    if (SolveSudoku() || isError)
                    {
                        flag = true;
                    }
                    matrix = Try(matrix);
                    //for (int i = 0; i < 9; i++)
                    //{
                    //    for (int j = 0; j < 9; j++)
                    //    {
                    //        if (matrix[i, j] == 0 && mat[i, j] != 0)
                    //        {
                    //            matrix[i, j] = -1;
                    //        }
                    //    }
                    //}
                }
            }
            if (isError)
            {
                isError = false;
                return false;
            }
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] == -1)
                    {
                        mat[i, j] = 0;
                    }
                }
            }
            return true;
        }

        public int[,] Try(int[,] matrix)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] == 0 && mat[i, j] != 0)
                    {
                        matrix[i, j] = -1;
                    }
                }
            }
            return matrix;
        }

        public void MidLevel()
        {
            int[,] matrix = new int[SUDOKU_SIZE, SUDOKU_SIZE];
            int count = 0;
            matrix = ReturnReplace(matrix);
            while (count < 5)
            {
                int row = random.Next(0, 9);
                int column = random.Next(0, 9);
                if (matrix[row, column] == 0)
                {
                    SolveSudoku();
                    matrix[row, column] = mat[row, column];
                    count++;
                }
            }
            Replace(matrix);
        }
        public void EasyLevel()
        {
            int[,] matrix = new int[SUDOKU_SIZE, SUDOKU_SIZE];
            int count = 0;
            matrix = ReturnReplace(matrix);
            while (count < 10)
            {
                int row = random.Next(0, 9);
                int column = random.Next(0, 9);
                if (matrix[row, column] == 0)
                {
                    SolveSudoku();
                    matrix[row, column] = mat[row, column];
                    count++;
                }
            }
            Replace(matrix);
        }

        public void VeryEasyLevel()
        {
            int[,] matrix = new int[SUDOKU_SIZE, SUDOKU_SIZE];
            int count = 0;
            matrix = ReturnReplace(matrix);
            while (count < 15)
            {
                int row = random.Next(0, 9);
                int column = random.Next(0, 9);
                if (matrix[row, column] == 0)
                {
                    SolveSudoku();
                    matrix[row, column] = mat[row, column];
                    count++;
                }
            }
            Replace(matrix);
        }
    }
}
