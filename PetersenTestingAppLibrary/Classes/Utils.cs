using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetersenTestingAppLibrary.Classes
{
    public class Utils
    {

        public TimeZoneInfo centralzone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

        public string sqlServerConnection { get; set; } = "Data Source=sql,1433;Initial Catalog=PETERSENTESTING;user id=ptestinguser;Password=zQl7FlRP25kcMPySi7BY60;Connection Timeout=30;TrustServerCertificate=True";


    }
}
