using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRSelfHost.Hubs
{
    public class ScalingHub : Hub
    {
        public async Task Welcom(string scalingMachineID, string message, string unit)
        {
            await Clients.All.SendAsync("Welcom", scalingMachineID, message, unit);
        }
       
    }
}
