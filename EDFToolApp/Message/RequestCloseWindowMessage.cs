using CommunityToolkit.Mvvm.Messaging.Messages;

namespace EDFToolApp.Message;
public class RequestCloseWindowMessage(bool close = true) : RequestMessage<bool>
{
    public bool Close { get; init; } = close;
}
