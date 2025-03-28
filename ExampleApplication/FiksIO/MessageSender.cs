using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client;
using KS.Fiks.IO.Client.Models;
using Serilog;

namespace ExampleApplication.FiksIO;

public class MessageSender
{
    private readonly IFiksIOClient _fiksIoClient;
    private readonly AppSettings _appSettings;
    
    private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);


    public MessageSender(IFiksIOClient fiksIoClient, AppSettings appSettings)
    {
        _fiksIoClient = fiksIoClient;
        _appSettings = appSettings;
    }

    public async Task<Guid> Send(string messageType, Guid toAccountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var klientMeldingId = Guid.NewGuid();
            var klientKorrelasjonsId = Guid.NewGuid().ToString();
            Log.Information("MessageSender - sending messagetype {MessageType} to account id {AccountId} with klientMeldingId {KlientMeldingId} klientKorrelasjonsId {KlientKorrelasjonsid}", messageType, toAccountId, klientMeldingId, klientKorrelasjonsId);
            var sendtMessage = await _fiksIoClient
                .Send(new MeldingRequest(_appSettings.FiksIOConfig.FiksIoAccountId, toAccountId, messageType, klientMeldingId: klientMeldingId, klientKorrelasjonsId: klientKorrelasjonsId), "testfile.txt", cancellationToken)
                .ConfigureAwait(false);
            Log.Information("MessageSender - message sendt with messageid: {MessageId}", sendtMessage.MeldingId);
            return sendtMessage.MeldingId;
        }
        catch (Exception e)
        {
            Log.Error("MessageSender - could not send message to account id {AccountId}. Error: {ErrorMessage}",toAccountId, e.Message);
            throw;
        }
    }
}