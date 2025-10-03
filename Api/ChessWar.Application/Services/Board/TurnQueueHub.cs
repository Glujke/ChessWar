using System.Threading.Channels;

namespace ChessWar.Application.Services.Board;

public sealed class TurnQueueHub
{
    public Channel<TurnRequest> TurnChannel { get; }

    public TurnQueueHub()
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };

        TurnChannel = System.Threading.Channels.Channel.CreateBounded<TurnRequest>(options);
    }
}


