using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Fleck.Samples.ConsoleApp
{
    class Server
    {
        public static Comunicacion cmh = new Comunicacion();

        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                }; // End OnOpen

                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                }; // End OnClose

                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    //allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                    Comunicacion oMessage = JsonConvert.DeserializeObject<Comunicacion>(message);
                    cmh.SubEvento = oMessage.SubEvento;
                    cmh.FEN = oMessage.FEN;
                    cmh.Segundos = oMessage.Segundos;
                    cmh.MultiPv = oMessage.MultiPv;
                    cmh.webSocket = socket;                    

                    if (oMessage.SubEvento == "Calcular")
                    {
                        // Creamos un delegado para el método OutListText()                        
                        ThreadStart ts = new ThreadStart(cmh.OutListText);

                        // Cre=amos un hilo para ejecutar el delegado...
                        Thread t = new Thread(ts);                        

                        t.Start();                        
                    }
                }; // End OnMessage

            });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }

        }
    }

    class Comunicacion
    {
        public string SubEvento;
        public string FEN;
        public string Segundos;
        public string MultiPv;
        public IWebSocketConnection webSocket;

        public void OutListText()
        {
            Console.WriteLine("Recibiendo parametros...");
            Console.WriteLine("SubEvento: " + this.SubEvento + " FEN: " + this.FEN + " Segundos: " + this.Segundos + " MultiPv: " + this.MultiPv);

            int Milisegundos = System.Convert.ToInt32(this.Segundos);
            Milisegundos = Milisegundos * 1000;

            Process process = new Process();

            process.StartInfo.FileName = "stockfish_10_x64_win.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;

            process.OutputDataReceived += OutputHandler;

            process.Start();

            process.StandardInput.WriteLine("uci");
            process.StandardInput.Flush();

            process.StandardInput.WriteLine("isready");
            process.StandardInput.Flush();

            process.StandardInput.WriteLine("setoption name multipv value " + this.MultiPv);
            process.StandardInput.Flush();

            process.StandardInput.WriteLine("position fen " + this.FEN);
            process.StandardInput.Flush();

            process.StandardInput.WriteLine("go movetime " + Milisegundos.ToString());
            process.StandardInput.Flush();            

            process.BeginOutputReadLine();

            process.WaitForExit();

            process.Close();
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(Environment.NewLine + outLine.Data);
            webSocket.Send(outLine.Data);
        }

    } // Fin de la clase Comunicacion
}

/*
    public static List<Fleck.IWebSocketConnection>allSockets = new List<Fleck.IWebSocketConnection>();
    allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
    allsockets.ToList().ForEach(s => s.Send(message));
    if (!t.IsAlive) {
        t.Start();
    } 
    cmh.thread = t;
    t.Abort();
    t.Join();
    else if (oMessage.SubEvento == "Reset")
    {
        Console.WriteLine("Reset");
        //cmh.thread.Abort();
    };
    foreach (Process process in Process.GetProcesses())
    public Thread thread;

*/