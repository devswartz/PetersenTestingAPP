using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PetersenTestingAppLibrary.Classes
{
    public class TestHeader
    {
        public int HeaderId { get; set; }
        public int? ORDUNIQ { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Item { get;set; }
        public string? ItemDesc { get;set; }
        public string? RatedMaxInflationPSI { get;set; }
        public string? DeflatedOrInternalDiaInches { get; set; }
        public string? MaxInflatedDiaInches { get; set; }
        public string? Customer { get;set; }
        public string? CustomerName { get; set; }
        public int? Closed { get; set; }
        public DateTime? ClosedDT { get; set; }
        public string? AudtUser { get; set; }
        public DateTime? AudtTime { get; set; }
        public DateTime? ORDDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? SONUM { get; set; }
        public string? TestNumber { get; set; }
        public int? NotForTest { get; set; }
        public string? CustomParam { get; set; }
        public string Channel {  get; set; }
        public DateTime? LastEntryDate { get; set; }

    }
}
