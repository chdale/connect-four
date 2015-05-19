using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ConsoleApplication1
{
    class Connection_Work
    {
        private TcpClient tc;
        NetworkStream ns;
        StreamWriter sw;
        StreamReader sr;
        public string ID;
        

        public delegate void InputReceived(string s, string i);
        public event InputReceived RaiseInputReceived;

        public Connection_Work(TcpClient _connection, string id)
        {
            tc = _connection;
            ns = tc.GetStream();
            sw = new StreamWriter(ns);
            sr = new StreamReader(ns);
            ID = id;

            Console.WriteLine("Client " + ID + " connected.");

            Thread t = new Thread(new ThreadStart(DoWork));
            t.Start();
        }

        private void DoWork()
        {
            sw.WriteLine("CONNECTED_PLAYING");
            sw.Flush();

            string s;
            while (true)
            {
                Thread.Sleep(0);
                s = sr.ReadLine();
                Console.WriteLine("Svr: " + s);
                RaiseInputReceived(s, ID);
            }
        }

        public void Send(string s)
        {
            Console.WriteLine("Sending '" + s + "' to client");
            sw.WriteLine(s);
            sw.Flush();
        }
    }
}
