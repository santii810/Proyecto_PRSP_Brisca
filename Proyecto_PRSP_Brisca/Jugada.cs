using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_PRSP_Brisca
{
    class Jugada
    {
        public string cartaJug1 { get; set; }
        public string cartaJug2 { get; set; }
        public string triunfo { get; set; }
        public int primeraCarta { get; set; }

        public string ultimaCartaJug1 { get; set; }
        public string ultimaCartaJug2 { get; set; }
        public int ultimoGanador { get; set; }


        public int ganador()
        {
            //si los palos son iguales gana la mas alta
            if (obtenerPalo(cartaJug1) == obtenerPalo(cartaJug2))
            {
                if (obtenerNumero(cartaJug1) > obtenerNumero(cartaJug2))
                    return 1;
                else
                    return 2;
            }
            //palos diferentes
            else
            {
                if (esTriunfo(cartaJug1))
                    return 1;
                if (esTriunfo(cartaJug2))
                    return 2;
            }
            //si los palos son diferentes y no se triunfa gana siempre la primera carta
            return primeraCarta;
        }

        private char obtenerPalo(string carta)
        {
            return carta.ElementAt(carta.Length - 1);
        }
        private int obtenerNumero(string carta)
        {
            return Int32.Parse(cartaJug2.Remove(carta.Length - 1));
        }
        private bool esTriunfo(string carta)
        {
            return obtenerPalo(carta) == obtenerPalo(triunfo);
        }
        public void reset()
        {
            ultimaCartaJug1 = cartaJug1;
            ultimaCartaJug2 = cartaJug2;
            cartaJug1 = "";
            cartaJug2 = "";
            primeraCarta = 0;
        }
    }
}
