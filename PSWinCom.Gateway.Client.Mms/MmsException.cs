using System;
using System.Collections.Generic;
using System.Linq;

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
