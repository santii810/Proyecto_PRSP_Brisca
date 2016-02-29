using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        delegate void DelegadoRespuesta();

        const int COD_INSCIPCION = 1;
        const int COD_INSCRIPCION_FALLIDA = 2;
        const int COD_INSCRIPCION_OK = 3;

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
        }
        private void updateIU(int cod)
        {
            panelConectando.Visibility = Visibility.Collapsed;
            panelRegistro.Visibility = Visibility.Collapsed;

            switch (cod)
            {
                case COD_INSCIPCION:
                    panelConectando.Visibility = Visibility.Visible;
                    break;
                case COD_INSCRIPCION_FALLIDA:
                    panelRegistro.Visibility = Visibility.Visible;
                    break;
                case COD_INSCRIPCION_OK:
                    panelRegistro.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        private void buttonInscribir_Click(object sender, RoutedEventArgs e)
        {
            int edad;
            if (textBoxNombre.Text != "" && int.TryParse(textBoxEdad.Text, out edad) && (radioButtonMasculino.IsChecked == true || radioButtonFemenino.IsChecked == true))
            {
                string sexo = "f";
                if (radioButtonMasculino.IsChecked == true)
                    sexo = "m";
                jugador = new Jugador
                {
                    nombre = textBoxNombre.Text,
                    edad = edad,
                    sexo = sexo
                };
            }
            try
            {
                client = new TcpClient(this.textBoxIp.Text, 2000);
                Ns = client.GetStream();
                sr = new StreamReader(Ns);
                sw = new StreamWriter(Ns);
                updateIU(COD_INSCIPCION);
                dato = sr.ReadLine();
                string[] respuesta = dato.Split('#');
                if(respuesta[1]== "OK")
                {
                    updateIU(COD_INSCRIPCION_OK);
                }
                else
                {
                    MessageBox.Show(respuesta[2]);
                    updateIU(COD_INSCRIPCION_FALLIDA);
                }

            }
            catch (Exception error)
            {
                Console.WriteLine("Error: " + error.ToString());
            }
            else
            {
                MessageBox.Show("Datos introducidos incorrectos");
            }
        }
    }
}
