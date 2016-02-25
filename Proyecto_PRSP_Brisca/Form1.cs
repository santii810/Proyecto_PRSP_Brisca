﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_PRSP_Brisca
{
    public partial class Form1 : Form
    {


        Jugador jug1 = new Jugador();
        Jugador jug2 = new Jugador();
        int jugadoresInscritos = 0;
        int turno = 1;
        string triunfo;
        Queue<string> baraja = new Queue<string>();
        int cartasEnMesa = 0;
        Jugada jugada = new Jugada();
        List<string> mazoJug1 = new List<string>();
        List<string> mazoJug2 = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        /* Inserto las cartas de la baraja española representando un simbolo a cada palo
        */
        #region barajar
        private void barajar()
        {
            List<string> tmp = new List<string>();
            for (int i = 0; i <= 10; i++)
            {
                tmp.Add(i + "!");
                tmp.Add(i + "@");
                tmp.Add(i + "#");
                tmp.Add(i + "%");
            }
            DesordenarLista(tmp);
            foreach (string item in tmp)
            {
                baraja.Enqueue(item);
            }
            triunfo = baraja.Last();

        }
        private static List<string> DesordenarLista(List<string> input)
        {
            List<string> ordList = input;
            List<string> desList = new List<string>();

            Random randNum = new Random();
            while (ordList.Count > 0)
            {
                int val = randNum.Next(0, ordList.Count - 1);
                desList.Add(ordList[val]);
                ordList.RemoveAt(val);
            }
            return desList;
        }
        #endregion
        private void siguienteTurno()
        {
            turno++;
            if (turno > 2) turno -= 2;
        }
        private void ManejarCliente(TcpClient cli)
        {
            string data;
            NetworkStream ns = cli.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            sw.WriteLine("$REGISTRAR$nombre$edad$sexo$");
            sw.WriteLine("$INICIAR$");
            sw.WriteLine("$CARTAS$");
            sw.WriteLine("$TURNO$");
            sw.WriteLine("$HECHAR_CARTA$carta$");
            sw.WriteLine("$COGER_CARTA$");
            sw.WriteLine("$ULTIMO_TURNO");
            sw.WriteLine("$TRIUNFO$");
            sw.WriteLine("$CARTAS_RESTANTES$");
            sw.WriteLine("$RECUENTO$");
            sw.WriteLine("$RESULTADO$");
            sw.Flush();
            while (true)
            {
                try
                {
                    data = sr.ReadLine();
                    string[] subdatos = data.Split('$');
                    switch (subdatos[1])
                    {
                        #region inscribir
                        case "REGISTRAR":
                            switch (jugadoresInscritos)
                            {
                                case 0:
                                    jug1.nombre = subdatos[2];
                                    int edad;
                                    int.TryParse(subdatos[3], out edad);
                                    jug1.edad = edad;
                                    jug1.sexo = subdatos[4];
                                    jug1.id = cli.Client.RemoteEndPoint.ToString();
                                    jugadoresInscritos++;

                                    sw.Flush();
                                    while (jugadoresInscritos != 2)
                                    {
                                        Thread.Sleep(1000);
                                        sw.Write("$OK$");
                                        sw.Flush();
                                    }
                                    break;
                                case 1:
                                    jug2.nombre = subdatos[2];
                                    int.TryParse(subdatos[3], out edad);
                                    jug2.edad = edad;
                                    jug2.sexo = subdatos[4];
                                    jug2.id = cli.Client.RemoteEndPoint.ToString();
                                    jugadoresInscritos++;
                                    sw.Write("$OK$");
                                    sw.Flush();
                                    break;
                                default:
                                    sw.WriteLine("#NOK#ya hay dos jugadores");
                                    sw.Flush();
                                    break;
                            }
                            break;
                        #endregion

                        #region iniciar
                        case "INICIAR":
                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                jug1.listoParaIniciarJuego = true;
                            }
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                jug2.listoParaIniciarJuego = true;
                            }
                            while (!jug1.listoParaIniciarJuego || !jug2.listoParaIniciarJuego)
                            {
                                sw.WriteLine("$OK");
                                sw.Flush();
                            }
                            barajar();
                            break;
                        #endregion

                        #region dar cartas
                        case "CARTAS":
                            for (int i = 0; i < 3; i++)
                            {
                                jug1.cartas.Add(baraja.Dequeue());
                                jug2.cartas.Add(baraja.Dequeue());
                            }
                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                                sw.WriteLine("$OK${0}${1}${2}${3}$", jug1.cartas.ElementAt(0), jug1.cartas.ElementAt(1), jug1.cartas.ElementAt(2), triunfo);
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                                sw.WriteLine("$OK${0}${1}${2}${3}$", jug1.cartas.ElementAt(0), jug1.cartas.ElementAt(1), jug1.cartas.ElementAt(2), triunfo);
                            sw.Flush();
                            break;
                        #endregion

                        #region conocer turno
                        case "TURNO":
                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                while (turno != 1) Thread.Sleep(1000);

                                if (cartasEnMesa == 0) sw.WriteLine("$OK$");
                                else if (cartasEnMesa == 1) sw.WriteLine("$OK${0}", jugada.cartaJug2);
                                sw.Flush();
                            }
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                while (turno != 2) Thread.Sleep(1000);

                                if (cartasEnMesa == 0) sw.WriteLine("$OK$");
                                else if (cartasEnMesa == 1) sw.WriteLine("$OK${0}", jugada.cartaJug1);
                                sw.Flush();
                            }
                            break;
                        #endregion

                        #region echar carta
                        case "ECHAR_CARTA":
                            if (jugada.primeraCarta != 0) jugada.primeraCarta = turno;

                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                int tmpNumCarta;
                                string tmpStrCarta;
                                int.TryParse(subdatos[2], out tmpNumCarta);
                                tmpStrCarta = jug1.cartas.ElementAt(tmpNumCarta);
                                jug1.cartas.RemoveAt(tmpNumCarta);
                                jugada.cartaJug1 = tmpStrCarta;
                                siguienteTurno();

                                sw.WriteLine("$OK$");
                                sw.Flush();
                            }
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                int tmpNumCarta;
                                string tmpStrCarta;
                                int.TryParse(subdatos[2], out tmpNumCarta);
                                tmpStrCarta = jug2.cartas.ElementAt(tmpNumCarta);
                                jug2.cartas.RemoveAt(tmpNumCarta);
                                jugada.cartaJug2 = tmpStrCarta;
                                siguienteTurno();

                                sw.WriteLine("$OK$");
                                sw.Flush();
                            }
                            //al hechar las 2 cartas calculo el ganador
                            if (jugada.cartaJug1 != "" && jugada.cartaJug2 != "")
                            {
                                turno = jugada.ganador();
                                jugada.ultimoGanador = turno;
                                if (jugada.ultimoGanador == 1)
                                {
                                    mazoJug1.Add(jugada.ultimaCartaJug1);
                                    mazoJug1.Add(jugada.ultimaCartaJug2);
                                }
                                else if (jugada.ultimoGanador == 2)
                                {
                                    mazoJug2.Add(jugada.ultimaCartaJug1);
                                    mazoJug2.Add(jugada.ultimaCartaJug2);
                                }
                                jugada.reset();
                            }
                            break;
                        #endregion

                        #region coger carta
                        case "COGER_CARTA":
                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                while (turno != 1) Thread.Sleep(1000);
                                jug1.cartas.Add(baraja.Dequeue());
                                siguienteTurno();

                                sw.WriteLine("$OK${0}$", jug1.cartas.Last());
                                sw.Flush();
                            }
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                            {
                                while (turno != 2) Thread.Sleep(1000);

                                jug2.cartas.Add(baraja.Dequeue());
                                siguienteTurno();

                                sw.WriteLine("$OK${0}$", jug2.cartas.Last());
                                sw.Flush();
                            }
                            break;
                        #endregion

                        #region triunfo
                        case "TRIUNFO":
                            sw.WriteLine("$OK${0}$", triunfo);
                            sw.Flush();
                            break;
                        #endregion

                        #region cartas restantes
                        case "CARTAS_RESTANTES":

                            sw.WriteLine("$OK${0}$", baraja.Count);
                            sw.Flush();

                            break;
                        #endregion

                        #region ultimo resultado
                        case "ULTIMO_TURNO":
                            sw.WriteLine("$OK${0}${1}${2}$", jugada.ultimaCartaJug1, jugada.ultimaCartaJug2, jugada.ultimoGanador);
                            sw.Flush();
                            break;
                        #endregion

                        #region recuento
                        case "RESULTADO":
                            if (jug1.id == cli.Client.RemoteEndPoint.ToString())
                            {

                                sw.WriteLine();
                                sw.Flush();
                            }
                            if (jug2.id == cli.Client.RemoteEndPoint.ToString())
                            {

                                sw.WriteLine();
                                sw.Flush();
                            }
                            break;
                        #endregion



                        default:
                            break;
                    }

                }
                catch (Exception error)
                {
                    Console.WriteLine("Error: {0}", error.ToString());
                    break;
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpListener newsock = new TcpListener(IPAddress.Any, 2000);
            newsock.Start();

            Console.WriteLine("Esperando por cliente");

            while (true)
            {
                TcpClient cliente = newsock.AcceptTcpClient(); //linea bloqueante
                Thread t = new Thread(() => this.ManejarCliente(cliente));
                //t.IsBackground = true;
                t.Start();
            }
        }
    }
}