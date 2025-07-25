using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BVGF.Connection
{
    public class AppSettings
    {
        //public const string BaseApiUrl = "https://localhost:7055/api/";
        // On Android emulator
        public const string BaseApiUrl = "http://172.26.80.1:5151/api/";

    }

    public static class Endpoints
    {
        public const string Login = "MstMember/login";
        public const string SearchMember = "MstMember";
        public const string CategaryDrp = "MstCategary/DropDown";

          
    }
}
