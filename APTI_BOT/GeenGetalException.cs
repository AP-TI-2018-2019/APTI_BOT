using System;
using System.Collections.Generic;
using System.Text;

namespace APTI_BOT
{
    class GeenGetalException : Exception
    {
        public GeenGetalException() : base("Je hebt geen getal meegegeven")
        {
        }
    }
}
