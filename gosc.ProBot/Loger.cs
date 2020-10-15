using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;

namespace gosc.ProBot
{
    class Loger
    {
        string writePath;
        private TextBlock frontStatusBlock;

        public Loger(TextBlock frontStatusBlock)
        {
           writePath = "log.txt";           
           this.frontStatusBlock = frontStatusBlock;
        }

        public void WrireLog(string messege)
        {
            using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
            {
                sw.Write(DateTime.Now);
                sw.WriteLine(" " +messege);               
            }

            frontStatusBlock.Dispatcher.Invoke(new Action(() => frontStatusBlock.Text += "\n" + messege));
        }
    }
}
