using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetatmoProxy.Services
{
    public interface IDayNightService
    {
        Task<string> IsSunOrMoonAsync();
    }
}
