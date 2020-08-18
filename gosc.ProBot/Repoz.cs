using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gosc.ProBot
{
    class Repoz
    {
        public string Code { get; set; }
        public bool flag { get; set; }

        public Repoz(string Code, bool Data)
        {
            this.Code = Code;
            this.flag = Data;
        }
    }
}
