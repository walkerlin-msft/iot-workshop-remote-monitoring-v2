using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmServiceBusConsoleApp
{
    class C2DCommand
    {
        public const string COMMAND_TEMPERATURE_ALERT = "TEMPERATURE_ALERT";
        public const string COMMAND_TURN_ONOFF = "TURN_ONOFF";

        public string command { get; set; }
        public string value { get; set; }
        public string time { get; set; }
    }

}
