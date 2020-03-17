using System;
using System.Collections.Generic;
using System.Text;

namespace NP_API_To_DB.Models
{
    public class NationalParkSqlInfo
    {

        public int SqlId { get; set; } = 0;
        public NationalParkServiceJsonData parkData { get; set; }
        public NationalParkSqlInfo(NationalParkServiceJsonData data)
        {
            parkData = data;
        }

    }
}
