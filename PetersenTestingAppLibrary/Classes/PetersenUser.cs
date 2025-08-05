using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetersenTestingAppLibrary.Classes
{
    public class PetersenUser
    {

        public string UserId { get; set; } //azure id
        public string Name { get; set; } // full name
        public string EmployeeId { get; set; }
        public string Email { get; set; }
        public int item_correction_form {  get; set; }
        public int item_correction_approver { get; set; }
        public int petersen_testing { get; set; }


    }
}
