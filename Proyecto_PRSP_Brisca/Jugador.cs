using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_PRSP_Brisca
{
    class Jugador
    {
        public string nombre { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
        public int puerto { get; set; }
        public bool listoParaIniciarJuego { get; set; }
        public List<string> cartas { get; set; } = new List<string>();

        public Jugador()
        {
            listoParaIniciarJuego = false;
        }
    }
}
