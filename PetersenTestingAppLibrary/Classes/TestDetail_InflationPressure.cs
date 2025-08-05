using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetersenTestingAppLibrary.Classes
{
    public class TestDetail_InflationPressure
    {
        public int DetailID { get; set; }
        public int HeaderID { get; set; }
        public string ReferenceNumber { get; set; }
        public int ORDUNIQ { get; set; }
        public string Item {  get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public DateTime TestTime { get; set; } = DateTime.Now;
        public DateTime StartTestDateTime {  get; set; } = DateTime.Now;
        public Decimal InflatedPSIG { get; set; }
        public int PressureAdded { get; set; }
        public string InflationMedium { get; set; }
        public Decimal AreaTempFahrenheit { get; set; }
        public string Comments {  get; set; }
        public string AUDTUser { get; set; }
        public DateTime AUDTTime { get; set; }
        public PetersenUser? AUDUser { get; set; }
        public string TestNumber { get; set; }
        public string Initial {  get; set; }
        public DateTime TestEndDate {  get; set; }
        public DateTime TestEndTime { get; set; }
        public string TestStatus { get; set; }
        public string FailureReason { get; set; }
        public DateTime? EndTestDateTime { get; set; }


    }
}
