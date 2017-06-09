using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Termit.Providers.Contracts;

namespace Termit.Providers
{
    public class AppLoggerProvider :
        ILoggerProvider
    {
        #region Private Members

        private TextWriterTraceListener _listener;

        #endregion

        public AppLoggerProvider()
        {
            _listener = new TextWriterTraceListener("Output.log", "listener");
        }

        public void Write(string message)
        {
            _listener.WriteLine($"[{DateTime.Now}] - {message}");
            _listener.Flush();
        }
    }
}
