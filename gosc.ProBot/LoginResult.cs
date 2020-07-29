using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gosc.ProBot
{
    public class LoginResult
    {
        public bool success;
        public bool emailauth_needed;
        public bool captcha_needed;

        public string message;
        public string captcha_gid;
        public string emailsteamid;
    }
}
