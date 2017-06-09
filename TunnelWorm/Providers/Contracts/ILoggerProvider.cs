using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Termit.Providers.Contracts
{
    public interface ILoggerProvider
    {
        void Write(string message);
    }
}
