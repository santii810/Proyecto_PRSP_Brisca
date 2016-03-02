using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cliente
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Jugador jugador;
        TcpClient client;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
        string dato;
        string[] respuesta;
        private string triunfo;
        delegate void DelegadoRespuesta();
        List<string> cartas = new List<string>();
        int puertoEscucha;
        private IPHostEntry ìhe;

        const int COD_INICIO = 0;
        const int COD_REGISTRO_OK = 1;
        const int COD_REGISTRO_NOK = 2;
        const int COD_INSCRIPCION_OK = 3;
        const int COD_CONEXION_OK = 4;
        const int COD_ESPERANDO = 5;
        const int COD_INICIAR_NOK = 6;
        const int COD_INICIAR_OK = 7;
        const int COD_CARTAS = 8;

        public NetworkStream Ns
        {
            get
            {
                return ns;
            }

            set
            {
                ns = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            updateIU(COD_INICIO);
        }
        private void updateIU(int cod)
        {
            panelEsperando.Visibility = Visibility.Collapsed;
            panelRegistro.Visibility = Visibility.Collapsed;
            panelIniciar.Visibility = Visibility.Collapsed;
            panelJuego.Visibility = Visibility.Collapsed;
            panelConectar.Visibility = Visibility.Collapsed;

            switch (cod)
            {
                case COD_INICIO:
                    panelConectar.Visibility = Visibility.Visible;
                    break;
                case COD_CONEXION_OK:
                    panelRegistro.Visibility = Visibility.Visible;
                    break;
                case COD_REGISTRO_OK:
                    panelEsperando.Visibility = Visibility.Visible;
                    break;
                case COD_REGISTRO_NOK:
                    panelRegistro.Visibility = Visibility.Visible;
                    break;
                case COD_INSCRIPCION_OK:
                    panelIniciar.Visibility = Visibility.Visible;
                    break;
                case COD_ESPERANDO:
                    panelIniciar.Visibility = Visibility.Visible;
                    break;
                case COD_INICIAR_OK:
                    panelJuego.Visibility = Visibility.Visible;
                    break;
                case COD_INICIAR_NOK:
                    panelIniciar.Visibility = Visibility.Visible;
                    break;
                case COD_CARTAS:
                    carta1.Content = cartas[0];
                    carta2.Content = cartas[1];
                    carta3.Content = cartas[2];
                    break;
                default:
                    break;
            }
        }
        private void buttonConectar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ìhe = Dns.GetHostEntry(Dns.GetHostName());
                puertoEscucha = Convert.ToInt32(textBoxPuerto.Text);
                client = new TcpClient(this.textBoxIp.Text, 2000);
                Ns = client.GetStream();
                sr = new StreamReader(Ns);
                sw = new StreamWriter(Ns);
                updateIU(COD_CONEXION_OK);

            }
            catch (Exception error)
            {
                Console.WriteLine("Error: " + error.ToString());
            }
        }

        private void buttonInscribir_Click(object sender, RoutedEventArgs e)
        {
            int edad;
            if (textBoxNombre.Text != "" && int.TryParse(textBoxEdad.Text, out edad) && (radioButtonMasculino.IsChecked == true || radioButtonFemenino.IsChecked == true))
            {
                string sexo = "m";
                if (radioButtonMasculino.IsChecked == true)
                    sexo = "h";
                jugador = new Jugador
                {
                    nombre = textBoxNombre.Text,
                    edad = edad,
                    sexo = sexo,
                    puerto = puertoEscucha,
                    ip = ìhe.AddressList[0].ToString()
                };
                sw.WriteLine("$REGISTRAR${0}${1}${2}$", jugador.nombre, jugador.ip, jugador.puerto);
                updateIU(COD_ESPERANDO);
                dato = sr.ReadLine();
                respuesta = dato.Split('$');
                if (respuesta[1] == "OK")
                {
                    updateIU(COD_INSCRIPCION_OK);
                }
                else
                {
                    MessageBox.Show(respuesta[2]);
                    updateIU(COD_REGISTRO_NOK);
                }
            }
            else
            {
                MessageBox.Show("Datos introducidos incorrectos");
            }
        }

        private void buttonIniciar_Click(object sender, RoutedEventArgs e)
        {
            sw.WriteLine("$OK$");
            updateIU(COD_ESPERANDO);
            sr.ReadLine();
            dato = sr.ReadLine();
            string[] respuesta = this.dato.Split('$');
            if (respuesta[1] == "OK")
            {
                updateIU(COD_INICIAR_OK);
                sw.WriteLine("$CARTAS$");
                dato = sr.ReadLine();
                respuesta = dato.Split('$');
                cartas.Add(respuesta[2]);
                cartas.Add(respuesta[3]);
                cartas.Add(respuesta[4]);
                triunfo = respuesta[5];
                updateIU(COD_CARTAS);
            }
            else
            {
                MessageBox.Show(respuesta[2]);
                updateIU(COD_INICIAR_NOK);
            }
        }

    }
}
