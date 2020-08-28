using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace gosc.ProBot
{
    class FlagSinhron
    {
        public FlagSinhron(bool flag)
        {
            this.flag = flag;
        }
         bool flag = false;

        public  bool Value
        {
            get
            {
                return flag;
            }
            set
            {
                flag = value;
                Console.WriteLine("Флаг изменён. Сейчас он равен: " +flag);
            }
        }
    }
}