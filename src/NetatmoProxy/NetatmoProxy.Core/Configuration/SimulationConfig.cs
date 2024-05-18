using NetatmoProxy.Model;
using NetatmoProxy.Model.Netatmo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetatmoProxy.Configuration
{
    public class SimulationConfig
    {
        public IEnumerable<Widget> Widgets { get; set; }
    }
}
