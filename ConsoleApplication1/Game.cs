using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace Connect4
{
    public delegate void ServerSentMoveHandler(int column);
    public delegate void GameOverHandler();

    class Game
    {
        private static event ServerSentMoveHandler RecievedMove;
        private static event GameOverHandler EndGame;

        private static TcpClient tc;
				static NetworkStream ns;
				static StreamReader sr;
        static StreamWriter sw;
        private static List<Connection_Work> connections;
        private static int ID = 0;
        static Board boardState = new Board();

        private static void incoming() 
        {
            Console.WriteLine("Client thread connected and listening.");
            Console.Out.Flush();
            while (true)
            {
                string s = sr.ReadLine(); //streamreader readline will block until a newline character is hit.
                Console.WriteLine("Server sends: " + s);

                // player 2 moves, tell our ai to move
                // fire ai move event
                // main subscribes a program to this.
                int val;
                if(int.TryParse(s, out val))
                {
                    var receivedMove = RecievedMove;

                    if (receivedMove != null)
                    {
                        receivedMove(int.Parse(s));
                    }
                }
            }
        }

        static void cw_RaiseInputReceived(string s, string i)
        {
            // broadcast
            foreach (Connection_Work c in connections)
            {
                // only send to other clients, not the one that sent the message
                if (c.ID != i)
                    c.Send(s);
            }
        }


        // Does opponent move first, then ai
        // Ai is treated as player 2. 

        private static void AiPlayer2(int col)
        {
            boardState.Player1Move(col, boardState);
            Console.WriteLine(boardState.toString());
            if (boardState.WinnerFound())
            {
                AnnounceWinner("You win!");
                //the incoming() will hang because no more inputs will come through
                return;
            }

            var column = boardState.AIMoveP2(boardState);
            sw.WriteLine(column);
            sw.Flush();
            boardState.Player2Move(column, boardState);
            Console.WriteLine(boardState.toString());
            if (boardState.WinnerFound())
            {
                AnnounceWinner("I win");
                //the incoming() thread will block maybe do an event to break
                return;
            }
            //tell them to make move
        }



        // Does opponent move first, then ai
        // AI is treated as player 1.

        private static void AiPlayer1(int col)
        {
            boardState.Player2Move(col, boardState);
            Console.WriteLine(boardState.toString());
            if (boardState.WinnerFound())
            {
                AnnounceWinner("You win!");
                //the incoming() will hang because no more inputs will come through
                return;
            }
            var column = boardState.AIMoveP1(boardState);
            boardState.Player1Move(column, boardState);
            Console.WriteLine(boardState.toString());
            sw.WriteLine(column);
            sw.Flush();
            if (boardState.WinnerFound())
            {
                AnnounceWinner("I win");
                //the incoming() thread will block maybe do an event to break
                return;
            }
            //tell them to make move
        }



        // Announces win and fires event to close thread

        private static void AnnounceWinner(string winner)
        {
            Console.Out.WriteLine(winner);

            var gameOver = EndGame;

            if (gameOver != null)
            {
                gameOver();
            }
        }

        static void Main(string[] args)
        {
            String network;
            String host;
            String IP;
            int port;
            String row;            //variable for rows the user sets
            String col;            //variable for the columns the user sets
            String win;            //variable for the win condition the user sets
            String move;           //variable for which column the user wants to move to
            String ai;             //variable for determining whether or not you want to play vs AI
            String turn;           //variable for determining whose turn is first
            int count = 0;         //the count used for determining whose turn it is
            String victor = "";    //the string used for determining the victory message
            
            //The user sets the parameters for the board class
            Console.WriteLine("Is this across a network? Yes (y) or No (n)");
            network = Console.ReadLine();
            
            if(network == "y") {
                Console.WriteLine("Is this the client (c) or the server (s)?");
                host = Console.ReadLine();
                if (host == "c")
                {
                    Console.WriteLine("Will you be using AI? (y) or (n)");
                    ai = Console.ReadLine();
                    Console.WriteLine("How many rows?");
                    row = Console.ReadLine();
                    Console.WriteLine("How many columns?");
                    col = Console.ReadLine();
                    Console.WriteLine("How many in a row to win?");
                    win = Console.ReadLine();
                    Console.WriteLine("Would you like to move 1st? (1) or 2nd? (2)");
                    turn = Console.ReadLine();
                    Console.WriteLine("IP to connect to? ");
                    IP = Console.ReadLine();
                    Console.WriteLine("Port to connect to? (4523 is suggested)");
                    port = Int32.Parse(Console.ReadLine());

                    tc = new TcpClient();
                    tc.Connect(IPAddress.Parse(IP), port);
                    ns = tc.GetStream();
                    
                    sr = new StreamReader(ns);
                    sw = new StreamWriter(ns);
                    boardState.SetRows(int.Parse(row));
                    boardState.SetColumns(int.Parse(col));
                    boardState.SetWinCondition(int.Parse(win));
                    boardState.SetBoard(int.Parse(row), int.Parse(col));

                    sw.WriteLine("The board is: "+row+"x"+col+ " with a win condition of "+win+" pieces in a row");
                    sw.Flush();

                    Console.WriteLine("Connected to Server. Server responded: " + sr.ReadLine());
                    Console.WriteLine("Starting");

                    string input;

                    if (ai == "y")
                    {
                        if (turn == "1")
                        {
                            RecievedMove += new ServerSentMoveHandler(AiPlayer1);
                            int myMove = boardState.AIMoveP1(boardState);
                            boardState.Player1Move(myMove, boardState);
                            sw.WriteLine(myMove);
                            sw.Flush();
                        }
                        else
                            RecievedMove += new ServerSentMoveHandler(AiPlayer2);

                        //Make first move
                        boardState.Player1Move(boardState.AIMoveP1(boardState), boardState);
                        Thread listen = new Thread(new ThreadStart(incoming));

                        //game over close the connection
                        EndGame += new GameOverHandler(listen.Abort);

                        listen.Start();
                    }



                    //if ai is player 2




                    else if (ai == "n")
                    {
                        do
                        {
                            Console.WriteLine(boardState.toString());
                            Console.WriteLine("Make your move: (0-" + (int.Parse(col) - 1) + ")");
                            input = Console.ReadLine();
                            boardState.Player1Move(int.Parse(input), boardState);
                            if (boardState.WinnerFound())
                            {
                                victor = "You win!";
                                input = "QUIT";
                            }
                            if (input != "QUIT")
                            {
                                sw.WriteLine(input);
                                sw.Flush(); // DON'T FORGET THIS OR THE MESSAGE WON'T SEND! :)
                            }
                            
                        } while (input != "QUIT");
                        Console.WriteLine(victor);
                    }
                    //else if (ai == "y")
                    //{
                    //    do
                    //    {
                    //        Console.WriteLine("Make your move: (0-" + (int.Parse(col) - 1) + ")");
                    //        input = state.AIMoveP1(state) + "";
                    //        state.Player1Move(int.Parse(input), state);
                    //        if (state.WinnerFound())
                    //        {
                    //            victor = "You win!";
                    //            input = "QUIT";
                    //        }
                    //        if (input != "QUIT")
                    //        {
                    //            sw.WriteLine(input);
                    //            sw.Flush(); // DON'T FORGET THIS OR THE MESSAGE WON'T SEND! :)
                    //        }
                    //    } while (input != "QUIT");
                    //    Console.WriteLine(victor);
                    //}
                    else
                    {
                        Console.WriteLine("Invalid Input for using AI!");
                    }
                }
                else if (host == "s")
                {
                    connections = new List<Connection_Work>();

                    TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 4523);
                    listener.Start();

                    while (true)
                    {
                        if (listener.Pending())
                        {
                            TcpClient tc = listener.AcceptTcpClient();
                            Connection_Work cw = new Connection_Work(tc, ID.ToString());
                            ID++;
                            cw.RaiseInputReceived += new Connection_Work.InputReceived(cw_RaiseInputReceived);
                            connections.Add(cw);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Input!");
                }
            }
                
            else if (network == "n")
            {

                Console.WriteLine("How many rows?");
                row = Console.ReadLine();
                Console.WriteLine("How many columns?");
                col = Console.ReadLine();
                Console.WriteLine("How many in a row to win?");
                win = Console.ReadLine();
                Console.WriteLine("Is this match versus AI? (y) or versus another player? (n)");
                ai = Console.ReadLine();
                //If the player is versus the AI
                if (ai == "y")
                {
                    // Determines if the player is going 1st or second
                    Console.WriteLine("Would you like to move 1st? (1) or 2nd? (2)");
                    turn = Console.ReadLine();

                    // If the player is first
                    if (turn == "1")
                    {
                        try
                        {
                            boardState = new Board(int.Parse(row), int.Parse(col), int.Parse(win));
                            while (!boardState.WinnerFound())                                             //Continues while a winner isn't found
                            {
                                Console.WriteLine(boardState.toString());
                                if (count % 2 == 0)                                                  //Sets the players turn
                                {
                                    Console.WriteLine("Player 1's move: Which column would you like to place your piece?");
                                    try
                                    {
                                        move = Console.ReadLine();
                                        boardState.Player1Move(int.Parse(move) - 1, boardState);                //The players move
                                        if (boardState.WinnerFound())                                     //If the player makes a winning move then sets the victor string
                                        {
                                            victor = "Player 1 wins!";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.StackTrace);
                                    }
                                }
                                else
                                {
                                    if (count == 1 && boardState.GetLastMove() == boardState.GetColumns()/2) {
                                        boardState.Player2Move((boardState.GetColumns()/2)-1,boardState);
                                    }
                                    else if (count == 1 && boardState.GetLastMove() != boardState.GetColumns() / 2)
                                    {
                                        boardState.Player2Move(boardState.GetColumns() / 2, boardState);
                                    }
                                    else
                                    {
                                        boardState.Player2Move(boardState.AIMoveP2(boardState), boardState);                          // 2nd player AI's move
                                        if (boardState.WinnerFound())                        // If the AI wins the move then sets victor string
                                        {
                                            victor = "Player 2 wins!";
                                        }
                                    }
                                }
                                count++;
                            }
                            Console.WriteLine(boardState.toString());
                            if (victor == "")                                        // After the game is complete it displays the victor
                            {
                                Console.WriteLine("It's a tie!");
                            }
                            else
                            {
                                Console.WriteLine(victor);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }

                    //If the player is 2nd
                    else if (turn == "2")
                    {
                        try
                        {
                            boardState = new Board(int.Parse(row), int.Parse(col), int.Parse(win));
                            while (!boardState.WinnerFound())                                            //Continues while a winner isn't found
                            {
                                Console.WriteLine(boardState.toString());
                                if (count % 2 == 0)                                                 // AI's turn
                                {
                                    if (count == 0)                                                 //If the it's the first turn, default to the center space
                                    {
                                        boardState.Player1Move((boardState.GetColumns()) / 2, boardState);
                                    }
                                    else                                                            //If it's not the first turn
                                    {
                                        boardState.Player1Move(boardState.AIMoveP1(boardState), boardState);                                      // The AI makes its move
                                        if (boardState.WinnerFound())                                    // If the AI wins, set the victor string
                                        {
                                            victor = "Player 1 wins!";
                                        }
                                    }
                                }
                                else                                                                 // Players turn
                                {
                                    Console.WriteLine("Player 2's move: Which column would you like to place your piece?");
                                    try
                                    {
                                        move = Console.ReadLine();
                                        boardState.Player2Move(int.Parse(move) - 1, boardState);                // Player's move
                                        if (boardState.WinnerFound())                                      // If the player wins set the victor string
                                        {
                                            victor = "Player 2 wins!";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.StackTrace);

                                    }
                                }
                                count++;
                            }
                            Console.WriteLine(boardState.toString());
                            if (victor == "")
                            {
                                Console.WriteLine("It's a tie!");
                            }
                            else
                            {
                                Console.WriteLine(victor);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input");
                    }
                }
                // If the player is versus another player
                else if (ai == "n")
                {
                    try
                    {
                        boardState = new Board(int.Parse(row), int.Parse(col), int.Parse(win));
                        while (!boardState.WinnerFound())                               //Continues while a winner has not been found
                        {
                            Console.WriteLine(boardState.toString());
                            if (count % 2 == 0)                                    //1st Player
                            {
                                Console.WriteLine("Player 1's move: Which column would you like to place your piece?");
                                try
                                {
                                    move = Console.ReadLine();
                                    boardState.Player1Move(int.Parse(move) - 1, boardState); // 1st Player's move
                                    if (boardState.WinnerFound())
                                    {
                                        victor = "Player 1 wins!";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.StackTrace);
                                }
                            }
                            else                                                     // 2nd Player
                            {
                                Console.WriteLine("Player 2's move: Which column would you like to place your piece?");
                                try
                                {
                                    move = Console.ReadLine();
                                    boardState.Player2Move(int.Parse(move) - 1, boardState);   // 2nd Player's move
                                    if (boardState.WinnerFound())
                                    {
                                        victor = "Player 2 wins!";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.StackTrace);
                                }
                            }
                            count++;
                        }
                        Console.WriteLine(boardState.toString());
                        if (victor == "")
                        {
                            Console.WriteLine("It's a tie!");
                        }
                        else
                        {
                            Console.WriteLine(victor);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                // If someone puts the wrong input
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
            else
            {
                Console.WriteLine("Invalid Input");
            }
            Console.ReadLine();                   // Keeps the console open
        }
    }
}