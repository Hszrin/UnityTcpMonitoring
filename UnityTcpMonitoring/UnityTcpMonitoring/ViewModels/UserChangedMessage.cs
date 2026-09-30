using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTcpMonitoringApp.ViewModels
{
    public class UserChangedMessage : ValueChangedMessage<string>
    {
        public UserChangedMessage(string value) : base(value)
        {
        }
    }
}
