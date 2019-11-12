using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Gyomu.Common
{
    public interface Configurator
    {
        string MachineName { get; }
        IPAddress Address { get; }
        string Username { get; }
        int UniqueInstanceIDPerMachine { get; }
        string Region { get; }
        Common.User User { get; }

        string Mode { get; }

        short ApplicationID { get; set; }


    }
}
