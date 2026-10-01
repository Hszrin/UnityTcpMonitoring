using CommunityToolkit.Mvvm.Messaging.Messages;

namespace UnityTcpMonitoring.ViewModels
{
    public class UserChangedMessage : ValueChangedMessage<string>
    {
        public UserChangedMessage(string value) : base(value)
        {
        }
    }
}
