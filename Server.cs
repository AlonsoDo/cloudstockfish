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
        public StringBuilder Output = new StringBuilder();
        public static IWebSocketConnection webSocket;
        //public static List<Fleck.IWebSocketConnection>allSockets = new List<Fleck.IWebSocketConnection>();
        
        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;            
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            var allsockets = new List<IWebSocketConnection>();

            server.Start(socket =>
                {
                    socket.OnOpen = () =>
                        {
                            Console.WriteLine("Open!");
                            allsockets.Add(socket);
                        };
                    socket.OnClose = () =>
                        {
                            // Cerrar socket
                            Console.WriteLine("Close!");
                            allsockets.Remove(socket);
                        };
                    socket.OnMessage = message =>
                        {
                            Console.WriteLine(message);

                            allsockets.ToList().ForEach(s => s.Send(message));

                            Comunicacion oMessage = JsonConvert.DeserializeObject<Comunicacion>(message);

                            // Creamos una instancia de la clase multi hilo y seteamos los campos que normalmente pasariamos como parametros
                            Comunicacion cmh = new Comunicacion();

                            cmh.SubEvento = oMessage.SubEvento;
                            cmh.FEN = oMessage.FEN;
                            cmh.Segundos = oMessage.Segundos;
                            cmh.MultiPv = oMessage.MultiPv;
                            cmh.webSocket = socket;

                            // Creamos un delegado para el método OutListText()                        
                            ThreadStart ts = new ThreadStart(cmh.OutListText);

                            // Creamos un hilo para ejecutar el delegado...
                            Thread t = new Thread(ts);

                            // Iniciamos la ejecucion del nuevo hilo
                            t.Start();                            

                            Thread.Sleep(100);                         

                            // Esperamos a que termine la ejecucion del hilo
                            t.Join();                            

                            //Console.WriteLine("Fin de la ejecución. Presione una tecla para salir.");
                            //Console.ReadLine();
                        };
                });

                string input = Console.ReadLine();
                while (input != "exit")
                {
                    allsockets.ToList().ForEach(s => s.Send(input));
                    input = Console.ReadLine();
                }
                
                /*
                IdHilos.Add(t.ManagedThreadId);
                IdHilo = t.ManagedThreadId;
                Index = t.ManagedThreadId;
                cmh.process = t.ManagedThreadId;            
                foreach (Process proceso in Process.GetProcesses())
                {
                    if (proceso.ProcessName == "stockfish_10_x64_win")
                    {
                        proceso.Kill();
                    }
                }
                Process[] process = Process.GetProcesses();

                foreach (Process prs in process)
                {
                    //Console.WriteLine("Procesos corriendo " + prs.Id);
                    //Console.WriteLine("Procesos corriendo " + IdHilo);

                    if (prs.Id == IdHilo)
                    {
                        prs.Kill();
                        break;
                    }
                }
            /*ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
            foreach (ProcessThread thread in currentThreads)
            {
                // Do whatever you need
                Console.WriteLine("Procesos corriendo " + thread.Id);
            }
            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }*/
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
            Process process = new Process();

            Console.WriteLine("Recibiendo parametros...");
            
            //Thread.Sleep(100);       

            int Milisegundos = System.Convert.ToInt32(this.Segundos);
            Milisegundos = Milisegundos * 1000;

            Console.WriteLine("SubEvento: " + this.SubEvento + " FEN: " + this.FEN + " Segundos: " + this.Segundos + " MultiPv: " + this.MultiPv);
            
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

           // Thread.Sleep(10);

            webSocket.Send(outLine.Data);

            //Console.WriteLine("Profundidad: " + this.Depth);
            //Console.WriteLine("Profundidad: " + Depth);
            //Console.WriteLine("Nombre del proceso: " + process.ProcessName);

            //process.Close();

            //var Socket = 
            //Console.WriteLine(Environment.NewLine + "Proceso parado");
            //process.Close();

            //Output.Append(Environment.NewLine + outLine.Data);
            //ListaVariantes.Add(Environment.NewLine + outLine.Data);
            //Console.WriteLine(Environment.NewLine + outLine.Data);
            //Console.WriteLine(ListaVariantes.Count);

            /*process.StandardInput.WriteLine("stop");
            process.StandardInput.AutoFlush = true;
            process.StandardInput.Close();*/
        }   
    }
}


