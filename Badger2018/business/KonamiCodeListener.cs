using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Badger2018.business
{
    public class KonamiCodeListener
    {
        static readonly List<Key> Keys = new List<Key>{
                  Key.Up, 
                  Key.Up, 
                  Key.Down, 
                  Key.Down, 
                  Key.Left, 
                  Key.Right, 
                  Key.Left, 
                  Key.Right,
                  Key.B,
                  Key.A
                 };
        private static int _mPosition = -1;

        public static int Position
        {
            get { return _mPosition; }
            private set { _mPosition = value; }
        }

        public static bool IsCompletedBy(Key key)
        {

            if (Keys[Position + 1] == key)
            {
                // on avance
                Position++;
            }
            else if (Position == 1 && key == Key.Up)
            {
                // on ne bouge pas
            }
            else if (Keys[0] == key)
            {
                // restart at 1st
                Position = 0;
            }
            else
            {
                // no match in sequence
                Position = -1;
            }

            if (Position == Keys.Count - 1)
            {
                Position = -1;
                return true;
            }

            return false;
        }
    }

}

