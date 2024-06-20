using System;
using System.Collections.Generic;

namespace WestLakeShape.Motion
{
    public class IOStatusSourceManager
    {
        private static readonly Lazy<IOStatusSourceManager> _instance = new Lazy<IOStatusSourceManager>(() => new IOStatusSourceManager());
        private readonly Dictionary<string, IOStateSource> _sources = new Dictionary<string, IOStateSource>();
        private IOStatusSourceManager() { }

        public static IOStatusSourceManager GetInstance()
        {
            return _instance.Value;
        }

        public void Register(IOStateSource source)
        {
            _sources.Add(source.Name, source);
        }

        public IOStateSource Get(string name)
        {
            return _sources[name];
        }
    }
}
