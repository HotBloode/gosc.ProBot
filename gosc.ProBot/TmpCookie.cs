using System;

namespace gosc.ProBot
{
    public partial class MainWindow
    {
        public class TmpCookie
        {
            public string Name { get; }
            public string Value { get; }
            public string Domain { get; }
            public virtual string Path { get; }
            public DateTime? Expiry { get; }
            public TmpCookie(string name, string value, string domain, string path, DateTime? expiry)
            {
                Name = name;
                Value = value;
                Domain = domain;
                Path = path;
                Expiry = expiry;
            }
        }
    }
}
