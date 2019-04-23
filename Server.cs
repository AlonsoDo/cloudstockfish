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
        public List<string>ListaVariantes = new List<string>();        
        public StringBuilder Output = new StringBuilder();
        public static List<int> IdHilos = new List<int>();
        //public static allSockets = new List<IWebSocketConnection>();
        public static IWebSocketConnection webSocket;
        public static List<Fleck.IWebSocketConnection> allSockets = new List<Fleck.IWebSocketConnection>();

        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;
            //var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            //var hilos = new List<Thread>();    
            

        server.Start(socket =>
            {
                socket.OnOpen = () =>
                    {
                        Console.WriteLine("Open!");
                        allSockets.Add(socket);

                        

                    };
                socket.OnClose = () =>
                    {
                        Console.WriteLine("Close!");
                        int Index = allSockets.IndexOf(socket);
                        allSockets.Remove(socket);                     

                        /*Thread th = hilos.ElementAt(Index); 
                        hilos.RemoveAt(Index);
                        th.Abort();*/
                    };
                socket.OnMessage = message =>
                    {
                        Console.WriteLine(message);
                        //allSockets.ToList().ForEach(s => s.Send("Echo: " + message));



                        ClaseMultiHilo oMessage = JsonConvert.DeserializeObject<ClaseMultiHilo>(message);

                        //Primera Conexion
                        if (oMessage.SubEvento == "PrimeraConexion")
                        {
                            // Creamos una instancia de la clase multi hilo y seteamos los campos que normalmente pasariamos como parametros
                            ClaseMultiHilo cmh = new ClaseMultiHilo();

                            cmh.SubEvento = oMessage.SubEvento;
                            cmh.FEN = oMessage.FEN;
                            cmh.Depth = oMessage.Depth;
                            cmh.MultiPv = oMessage.MultiPv;

                            cmh.webSocket = socket;

                            // Creamos un delegado para el método OutListText()                        
                            ThreadStart ts = new ThreadStart(cmh.OutListText);

                            // Creamos un hilo para ejecutar el delegado...
                            Thread t = new Thread(ts);

                            // Iniciamos la ejecucion del nuevo hilo
                            t.Start();

                            IdHilos.Add(t.ManagedThreadId);

                            Console.WriteLine("hilo: " + t.ManagedThreadId);

                            // Esperamos a que termine la ejecucion del hilo
                            t.Join();

                            Console.WriteLine("Fin de la ejecución. Presione una tecla para salir.");
                            Console.ReadLine();
                        }
                        else if (oMessage.SubEvento == "SiguientesConexiones")
                        {
                            Console.WriteLine("Test");
                        }
                        else if (oMessage.SubEvento == "StopCalc")
                        {
                            Console.WriteLine("Stop");
                        }


                    };
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

    class ClaseMultiHilo
    {
        public string SubEvento;
        public string FEN;
        public string Depth;
        public string MultiPv;
        public IWebSocketConnection webSocket;
        private Process process = new Process();

        public void OutListText()
        {
            Console.WriteLine("Recibiendo parametros...");
            Thread.Sleep(2000);
            Console.WriteLine("SubEvento: " + this.SubEvento + " FEN: " + this.FEN + " Depth: " + this.Depth + " MultiPv: " + this.MultiPv);
            
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

            process.StandardInput.WriteLine("position fen " + this.FEN);
            process.StandardInput.Flush();

            process.StandardInput.WriteLine("setoption name multipv value " + this.MultiPv);
            process.StandardInput.Flush();

            Thread.Sleep(1000);

            process.StandardInput.WriteLine("go depth " + this.Depth); 
            process.StandardInput.Flush();


            //process.StandardInput.WriteLine("stop");
            //process.StandardInput.Flush();



            process.BeginOutputReadLine();

            process.WaitForExit();

            process.Close();
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(Environment.NewLine + outLine.Data);

            Thread.Sleep(1000);

            webSocket.Send(outLine.Data);
            
            //var Socket = 
            //Console.WriteLine(Environment.NewLine + "Proceso parado");
            //process.Close();

            //Output.Append(Environment.NewLine + outLine.Data);
            //ListaVariantes.Add(Environment.NewLine + outLine.Data);
            //Console.WriteLine(Environment.NewLine + outLine.Data);
            //Console.WriteLine(ListaVariantes.Count);
            /*if (ListaVariantes.Count == 250)
            {
                process.StandardInput.WriteLine("stop");
                process.StandardInput.AutoFlush = true;
                process.StandardInput.Close();
            }*/
        }
    }
}
