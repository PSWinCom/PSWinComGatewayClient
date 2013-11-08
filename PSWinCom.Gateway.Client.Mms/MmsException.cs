using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSWinCom.Gateway.Client
{
    public class MmsException : Exception
    {

        public MmsException()
            : base()
        {
        }

        public MmsException(string message)
            : base(message)
        {
        }

        public MmsException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
