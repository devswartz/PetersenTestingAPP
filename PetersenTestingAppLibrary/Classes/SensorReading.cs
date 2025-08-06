using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace PetersenTestingAppLibrary.Classes;

    public class SensorReading
    {
        public required string SensorID { get; set; }
        public DateTime TimeStamp { get; set; }
        public double PressurePSI { get; set; }
        public double BatteryVoltage { get; set; }
        public double Temperature { get; set; }
    }


