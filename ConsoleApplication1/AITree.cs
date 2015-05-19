using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * AITree class handles possible future moves that will be evaluated for win conditions to determine the best possible future move to make when
 * the AI decides which move to make
 */
namespace Connect4
{
    class AITree 
    {
        public Board Game { get; set; }         // Get and set methods for an instance of a Board object
        public int Move { get; set; }           // Get and set methods for whichever column the node represents a piece being placed
        public int Depth { get; set; }          // Get and set methods for the depth of a node in the tree
        public AITree Parent { get; set; }      // Get and set methods for the parent of node in the tree
        public List<AITree> Children { get; set; }          // Get and set methods for a list containing nodes of children 
        public bool idealMove { get; set; }     // Get and set methods for a boolean determining whether the node is an ideal move or not
        public int winCount { get; set; }       // Get and set methods for an integer that shows the amount of ways to win in the next 3 moves

        /*
         * The constructor for the node representing the root
         */
        public AITree(Board game)
        {
            this.Move = 0;
            this.Game = game;
        }

        /*
         * The constructor for nodes representing the possible choices
         */
        public AITree(int move, Board game, String[,] state, int depth)
        {
            this.Move = move;
            this.Game = game;
            this.Game.SetBoard(state);
            this.idealMove = true;
            this.winCount = 0;
        }

        /*
         * Handles adding children to the current node for if the AI is the 1st player
         */
        public void AddChildrenP1(int depth)
        {
            AITree node;
            if (this.Children == null)                                // Creates a list for the node if it doesn't exist
            {
                    this.Children = new List<AITree>();
            }
            for (int i = 1; i <= this.Game.GetColumns(); i++)         // Creates a node in the children list for each possible move
            {
                Board temp = this.Game.Clone();
                temp.Player1Move(i-1, temp);
                if (temp.WinnerFound())                               // Sets win to true if the current nodes move makes a win
                {
                    temp.SetWinFound(true);
                }
                node = new AITree(i, temp, temp.GetBoard(), depth) { Parent = this };
                Children.Add(node);
            }
        }

        /*
         * Handles adding children to the current node for if the AI is the 2nd player
         */
        public void AddChildrenP2(int depth)
        {
            AITree node;
            if (this.Children == null)                                  // Creates a list for the node if it doesn't exist
            {
                this.Children = new List<AITree>();
            }
            for (int i = 1; i <= this.Game.GetColumns(); i++)           // Creates a node in the children list for each possible move
            {
                Board temp = this.Game.Clone();
                temp.Player2Move(i-1,temp);
                if (temp.WinnerFound())                                 // Sets win to true if the current nodes move makes a win
                {
                    temp.SetWinFound(true);
                }
                node = new AITree(i, temp, temp.GetBoard(), depth) { Parent = this};
                Children.Add(node); 
            }
        }

    }
}
