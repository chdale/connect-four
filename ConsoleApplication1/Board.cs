using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Board 
    {
        int rows;                           //Amount of rows for the board
        int columns;                        //Amount of columns for the board
        int winCondition;                   //Amount of pieces in a row to win
        String[,] board;                    //String array to represent the board
        int tempRow;                        //Used to check for connect4 using that point in the board string array and checking its diagonals and lines
        int tempCol;                        //Used to check for connect4 using that point in the board string array and checking its diagonals and lines
        bool winFound = false;              //If it finds a win when creating the tree in AITRee.cs Lines , this boolean changes to true
        int lastMove;                       //Used to determine the first move of the AI if it's the 2nd player


        public Board()
        {
        }
        /*
         * Constructor for the initial board
         */
        public Board(int rows, int columns, int winCondition)
        {
            this.rows=rows;
            this.columns = columns;
            this.winCondition = winCondition;
            this.board = new String[rows,columns];
        }
        /*
         * Constructor for boards that will be entering the AITree and need an initial String array board to base its input off of
         */
        public Board(int rows, int columns, int winCondition, String[,] board)
        {
            this.rows = rows;
            this.columns = columns;
            this.winCondition = winCondition;
            this.board = board;
        }
        public void SetRows(int rows)
        {
            this.rows = rows;
        }
        public int GetRows()                      // Gets rows
        {
            return rows;
        }
        public int GetLastMove()                  // Gets last move
        {
            return lastMove;
        }
        public void SetColumns(int columns)
        {
            this.columns = columns;
        }
        public int GetColumns()                   // Gets columns
        {
            return columns;
        }

        public bool GetWinFound()                 // Gets winFound
        {
            return winFound;
        }

        public void SetWinFound(bool win)         // Sets winFound
        {
            this.winFound = win;
        }
        public void SetWinCondition(int win)
        {
            this.winCondition = win;
        }
        public int GetWinCondition()              // Gets winCondition
        {
            return winCondition;
        }

        public void SetBoard(String[,] board)     // Sets board string array
        {
            this.board = board;
        }

        public void SetBoard(int rows, int columns)
        {
            this.board = new String[rows, columns];
        }

        public String[,] GetBoard()               // Gets a shallow clone of the board string array
        {
            return (String[,])board.Clone();
        }

        public String toString()                   // ToString method for the board that outputs the string array in an easy to read format
        {
            String state = "";
            for (int i = 0; i < this.board.GetLength(0); i++)
            {
                for (int j = 0; j < this.board.GetLength(1); j++)
                {
                    if (this.board[i, j] == null)                        // Null entries on the string array are displayed by periods
                    {
                        state += ". ";
                    }
                    else
                    {
                        state += this.board[i, j] + " ";                 // Non-null entries are represented by themselves
                    }
                }
                state += "\n";                                           // Each row needs to be on a new row
            }
                return state;
        }
        /*
         * The method that handles AI moves if the AI is player 1
         */ 
        public int AIMoveP1(Board game)
        {
            Board tempBoard = game.Clone();                     // Shallow copies the original board                        
            AITree root = new AITree(tempBoard);                // Sets the root of the tree to the state of the original board
            root.AddChildrenP1(1);                              // Adds children to the root node based off of all possible moves

            foreach (AITree nodeD1 in root.Children)            // Checks each node in the list of children
            {
                if (nodeD1.Game.GetWinFound())                  // If it has the possibility of winning on the next turn it will make that move and finish its turn
                {
                    return nodeD1.Move - 1;
                }
                else                                            // If it doesn't have the possibility of winning in its next move
                {
                    nodeD1.AddChildrenP2(2);                    // Adds the children of each node in depth 1
                    foreach (AITree nodeD2 in nodeD1.Children)  // Goes through each children node of depth 2
                    {
                        if (nodeD2.Game.GetWinFound())          // If the opponent can win due to the previous move it makes the previous move not ideal
                        {
                            nodeD2.Parent.idealMove = false;
                        }
                        else                                    // If the move is ideal then it checks the next level to see
                        {
                            nodeD2.AddChildrenP1(3);                                  // Adds the children of each node in depth 2
                            foreach (AITree nodeD3 in nodeD2.Children)                // Goes through each children node of depth 3
                            {
                                if (nodeD3.Game.GetWinFound())                        // It checks to see if it can win on its next turn
                                {
                                    nodeD3.Parent.Parent.winCount++;                  // This increases the WinCount on the original node in depth 1
                                }
                            }
                        }
                    }
                }
            }
                int tempMove = 0;                          // This variable holds the column in which a piece will be placed
                int tempCount = 0;                         // This variable holds the amount of possible win conditions it can reach in the next 3 moves
                int idealMoveCount = 0;
                foreach (AITree node in root.Children)     // This will check every single action it can take on the turn
                {
                    // First it ensures that the next move won't lead to the victory of the enemy on the next turn
                    // Second it ensures that the win count is greater than or equal to the previous node move that was stored
                    // Third it ensures that there is room to place a piece
                    if (node.idealMove && node.winCount >= tempCount && root.Game.board[0, node.Move - 1] == null)
                    {
                        idealMoveCount++;
                        // This if statement ensures that if all other conditions equals, it will be placed near the center to enable more plays
                        if (node.winCount == tempCount && (Math.Abs(node.Move - (node.Game.columns / 2)) > Math.Abs(tempMove - (node.Game.columns / 2))) && idealMoveCount>1)
                        {
                        }
                        else
                        {
                            tempCount = node.winCount;
                            tempMove = node.Move;
                        }
                    }
                }

                if (tempMove != 0)                   // Places a move as long as there is an ideal move (Else the move variable won't change)
                {
                    return tempMove - 1;
                }
                else                                 // This does the same as above except it keeps on playing if there is no ideal moves
                {
                    foreach (AITree node in root.Children)
                    {
                        if (node.winCount >= tempCount && root.Game.board[0, node.Move - 1] == null)
                        {
                            tempCount = node.winCount;
                            tempMove = node.Move;
                        }
                    }
                    return tempMove-1;
                }
        }
        /*
        * The method that handles AI moves if the AI is player 1
        */ 
        public int AIMoveP2(Board game)
        {
            Board tempBoard = game.Clone();                     // Shallow copies the original board
            AITree root = new AITree(tempBoard);                // Sets the root of the tree to the state of the original board
            root.AddChildrenP2(1);                              // Adds children to the root node based off of all possible moves
            foreach (AITree nodeD1 in root.Children)            // Checks each node in the list of children
            {
                if (nodeD1.Game.GetWinFound())                  // If it has the possibility of winning on the next turn it will make that move and finish its turn
                {
                    return nodeD1.Move - 1;

                }
                else                                            // If it doesn't have the possibility of winning in its next move
                {
                    nodeD1.AddChildrenP1(2);                    // Adds the children of each node in depth 1
                    foreach (AITree nodeD2 in nodeD1.Children)  // Goes through each children node of depth 2
                    {
                        if (nodeD2.Game.GetWinFound())          // If the opponent can win due to the previous move it makes the previous move not ideal
                        {
                            nodeD2.Parent.idealMove = false;
                        }
                        else                                    // If the move is ideal then it checks the next level to see
                        {
                            nodeD2.AddChildrenP2(3);                                  // Adds the children of each node in depth 2
                            foreach (AITree nodeD3 in nodeD2.Children)                // Goes through each children node of depth 3
                            {
                                if (nodeD3.Game.GetWinFound())                        // It checks to see if it can win on its next turn
                                {
                                    nodeD3.Parent.Parent.winCount++;                  // This increases the WinCount on the original node in depth 1
                                }
                            }
                        }
                    }
                }
            }
                int tempMove = 0;                          // This variable holds the column in which a piece will be placed
                int tempCount = 0;                         // This variable holds the amount of possible win conditions it can reach in the next 3 moves
                int idealMoveCount = 0;
                foreach (AITree node in root.Children)     // This will check every single action it can take on the turn
                {
                    // First it ensures that the next move won't lead to the victory of the enemy on the next turn
                    // Second it ensures that the win count is greater than or equal to the previous node move that was stored
                    // Third it ensures that there is room to place a piece
                    if (node.idealMove && node.winCount >= tempCount && root.Game.board[0, node.Move - 1] == null)
                    {
                        idealMoveCount++;
                        // This if statement ensures that if all other conditions equals, it will be placed near the center to enable more plays (only works if more than 1 ideal move)
                        if (node.winCount == tempCount && (Math.Abs(node.Move - (node.Game.columns / 2)) > Math.Abs(tempMove - (node.Game.columns / 2))) && idealMoveCount>1)
                        {
                        }
                        else
                        {
                            tempCount = node.winCount;
                            tempMove = node.Move;
                        }
                    }
                }

                if (tempMove != 0)                   // Places a move as long as there is an ideal move (Else the move variable won't change)
                {
                    return tempMove - 1;
                }
                else                                 // This does the same as above except it keeps on playing if there is no ideal moves
                {
                    foreach (AITree node in root.Children)
                    {
                        if (node.winCount >= tempCount && root.Game.board[0, node.Move - 1] == null)
                        {
                            tempCount = node.winCount;
                            tempMove = node.Move;
                        }
                    }
                    return tempMove - 1;
                }
        }
        /*
         * Handles the move of player 1
         */
        public void Player1Move(int column, Board game)
        {
            int count = game.rows-1;                    // Defaults the count to the bottom row of the array
            this.lastMove = column;                     // Saves the players last move to determine the 2nd move of the AI
            while (count>=0)                            // Goes until reaching the top row
            {
                if (game.board[count, column] == null)  // If this spot is empty
                {
                    game.board[count, column] = "X";    // Place an X
                    game.tempCol = column;              // Sets the temp column variable to the current column
                    game.tempRow = count;               // Sets the temp row variable to the current column
                    break;
                }
                count--;
            }
            
        }

        public void Player2Move(int column, Board game)
        {
            int count = game.rows - 1;                    // Defaults the count to the bottom row of the array
            this.lastMove = column;                     // Saves the players last move to determine the 2nd move of the AI
            while (count >= 0)                            // Goes until reaching the top row
            {
                if (game.board[count, column] == null)  // If this spot is empty
                {
                    game.board[count, column] = "O";    // Place an X
                    game.tempCol = column;              // Sets the temp column variable to the current column
                    game.tempRow = count;               // Sets the temp row variable to the current column
                    break;
                }
                count--;
            }
        }

        /*
         * Shallow copy method
         */
        public Board Clone()                            
        {
            Board temp = new Board(this.GetRows(), this.GetColumns(), this.GetWinCondition(), this.GetBoard());
            return temp;
        }

        /*
         * This is the method that is called to determine whether or not there is a winner
         */
        public bool WinnerFound()
        {
            int diagCol;                                        // The variable placeholder for columns in the diagonal
            int diagRow;                                        // The variable placeholder for rows in the diagonal
            int count = 0;                                      // The count to represent how many pieces in a row
            String player = this.board[tempRow, tempCol];       // Sets the players piece to the most recently placed piece
            for (int i = 0; i < this.columns; i++)              // Checks the entire row for the preferred piece
            {
                if (this.board[this.tempRow, i] == null || this.board[this.tempRow, i] != player)        // If the place doesn't have the players piece then the count gets reset to 0
                {
                    count = 0;
                }
                else                            // If it has the players piece then the count goes up
                {
                    count++;
                }
                if (count == winCondition)      // If there is winCondition amount in a row, then a winner is found
                {
                    return true;
                }
            }
            count = 0;       // Resets the count before checking columns
            for (int i = 0; i < this.rows; i++)                  // Checks the entire column for the preferred piece
            {
                if (this.board[i, this.tempCol] == null || this.board[i, this.tempCol] != player)         // If the piece doesn't have the players piece then the count gets reset to 0
                {
                    count = 0;                 
                }
                else                             // If it has the players piece then the count goes up
                {
                    count++;
                }
                if (count == winCondition)       // If there is a winCondition amount in a row, then a winner is found
                {
                    return true;
                }
            }
            count = 0;                           // Resets count

            // Checks diagonal from top left to bottom right
            if (this.tempCol > this.tempRow)                           
            {
                diagCol = this.tempCol - this.tempRow;
                diagRow = 0;
            }
            else
            {
                diagRow = this.tempRow - this.tempCol;
                diagCol = 0;
            }
            while (diagRow < this.rows && diagCol < this.columns)
            {
                if (this.board[diagRow, diagCol] == null || this.board[diagRow, diagCol] != player)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == winCondition)
                {
                    return true;
                }
                
                diagRow++;
                diagCol++;
            }
            count = 0;
            //Checks diagonal from bottom left to top right
            if (this.rows-this.tempRow-1>this.tempCol)
            {
                diagCol = 0;
                diagRow = this.tempRow+tempCol;
            }
            else
            {
                diagRow = this.rows-1;
                diagCol = this.tempCol -(this.rows - this.tempRow - 1);
            }
            while (diagRow >= 0 && diagCol < this.columns)
            {
                if (this.board[diagRow, diagCol] == null || this.board[diagRow, diagCol] != player)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count == winCondition)
                {
                    return true;
                }
                diagRow--;
                diagCol++;
            }

            return false; //If Win conditions aren't met, then no winner is found
        }
    }
}
