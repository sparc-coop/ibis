using Ibis.Features.Sparc.Realtime;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Collections.Concurrent;

namespace Ibis.Features._Plugins;

public record AudioConnection(string SessionId, SpeechRecognizer SpeechClient, VoiceAudioStream AudioStream);

public class AzureListener : IListener
{
    readonly HttpClient Client;
    readonly string SubscriptionKey;
    static readonly List<AudioConnection> _audioConnections = new();

    public AzureListener(IConfiguration configuration)
    {
        SubscriptionKey = configuration.GetConnectionString("Speech");

        Client = new HttpClient
        {
            BaseAddress = new Uri("	https://eastus.tts.speech.microsoft.com")
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
    }

    public async Task<string> BeginListeningAsync()
    {
        var audioStream = new VoiceAudioStream();

        var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
        var audioConfig = AudioConfig.FromStreamInput(audioStream, audioFormat);
        var speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, "eastus");
        var speechClient = new SpeechRecognizer(speechConfig, audioConfig);

        speechClient.Recognized += SpeechClient_Recognized;
        speechClient.Recognizing += SpeechClient_Recognizing;
        speechClient.Canceled += SpeechClient_Canceled;

        string sessionId = speechClient.Properties.GetProperty(PropertyId.Speech_SessionId);

        _audioConnections.Add(new(sessionId, speechClient, audioStream));

        await speechClient.StartContinuousRecognitionAsync();

        return sessionId;
    }

    public Task ListenAsync(string sessionId, byte[] audioChunk)
    {
        _audioConnections.FirstOrDefault(x => x.SessionId == sessionId)?.AudioStream
            .Write(audioChunk, 0, audioChunk.Length);

        return Task.CompletedTask;
    }

    private void SpeechClient_Canceled(object? sender, SpeechRecognitionCanceledEventArgs e)
    {
        var audioConnection = _audioConnections.FirstOrDefault(x => x.SessionId == e.SessionId);
        if (audioConnection == null) return;
        
        audioConnection.SpeechClient.Dispose();
        _audioConnections.Remove(audioConnection);
    }

    private void SpeechClient_Recognizing(object? sender, SpeechRecognitionEventArgs e)
    {
        var audioConnection = _audioConnections.FirstOrDefault(x => x.SessionId == e.SessionId);
        if (audioConnection == null) return;
    }

    private void SpeechClient_Recognized(object? sender, SpeechRecognitionEventArgs e)
    {
        var audioConnection = _audioConnections.FirstOrDefault(x => x.SessionId == e.SessionId);
        if (audioConnection == null) return;
    }

    //internal async Task<List<Message>> TranscribeSpeechFromFile(Message message, byte[] bytes, string fileName)
    //{
    //    var speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, "eastus");
    //    var audioConfig = IbisHelpers.OpenWavFile(bytes);

    //    var messages = new List<Message>();

    //    try
    //    {
    //        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);
    //        var stopRecognition = new TaskCompletionSource<int>();

    //        recognizer.Recognized += (s, e) =>
    //        {
    //            if (e.Result.Reason == ResultReason.RecognizedSpeech)
    //            {
    //                Message newMessage = new(message.SubroomId!, message.UserId, message.Language, SourceTypes.Upload, message.UserName, message.UserInitials);
    //                newMessage.SetTimestamp(e.Result.OffsetInTicks, e.Result.Duration);
    //                newMessage.SetText(e.Result.Text);
    //                if (message.SubroomId != null)
    //                    newMessage.SetSubroomId(message.SubroomId);
    //                messages.Add(newMessage);
    //            }
    //        };

    //        recognizer.SessionStopped += (s, e) =>
    //        {
    //            stopRecognition.TrySetResult(0);
    //        };

    //        Console.WriteLine("Transcribing wav file...");
    //        await recognizer.StartContinuousRecognitionAsync();
    //        Task.WaitAny(new[] { stopRecognition.Task });
    //        return messages;
    //    }
    //    catch (Exception ex)
    //    {
    //        var testing = ex.Message;
    //        return new();
    //    }
    //}
}

public class VoiceAudioStream : PullAudioInputStreamCallback
{
    private readonly EchoStream _dataStream = new();
    private ManualResetEvent? _waitForEmptyDataStream = null;

    public override int Read(byte[] dataBuffer, uint size)
    {
        if (_waitForEmptyDataStream != null && !_dataStream.DataAvailable)
        {
            _waitForEmptyDataStream.Set();
            return 0;
        }

        return _dataStream.Read(dataBuffer, 0, dataBuffer.Length);
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        _dataStream.Write(buffer, offset, count);
    }

    public override void Close()
    {
        if (_dataStream.DataAvailable)
        {
            _waitForEmptyDataStream = new ManualResetEvent(false);
            _waitForEmptyDataStream.WaitOne();
        }

        _waitForEmptyDataStream?.Close();
        _dataStream.Dispose();
        base.Close();
    }
}

public class EchoStream : MemoryStream
{
    private readonly ManualResetEvent _DataReady = new(false);
    private readonly ConcurrentQueue<byte[]> _Buffers = new();

    public bool DataAvailable { get { return !_Buffers.IsEmpty; } }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _Buffers.Enqueue(buffer.Take(count).ToArray());
        _DataReady.Set();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _DataReady.WaitOne();

        if (!_Buffers.TryDequeue(out byte[]? lBuffer))
        {
            _DataReady.Reset();
            return -1;
        }

        if (!DataAvailable)
            _DataReady.Reset();

        Array.Copy(lBuffer, buffer, lBuffer.Length);
        return lBuffer.Length;
    }
}

