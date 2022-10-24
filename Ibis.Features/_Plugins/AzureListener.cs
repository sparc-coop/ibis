using Concentus.Oggfile;
using Concentus.Structs;
using Ibis.Features.Sparc.Realtime;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Collections.Concurrent;

namespace Ibis.Features._Plugins;

public record AudioConnection(string SessionId, SpeechRecognizer SpeechClient, VoiceAudioStream AudioStream);
public record SpeechSessionStarted(string SessionId) : SparcNotification(SessionId);
public record SpeechRecognizing(string SessionId, string Text, long Duration) : SparcNotification(SessionId);
public record SpeechRecognized(string SessionId, string Text, long Duration) : SparcNotification(SessionId);
public class AzureListener : IListener
{
    readonly HttpClient Client;
    readonly string SubscriptionKey;
    static readonly List<AudioConnection> _audioConnections = new();

    public Publisher Publisher { get; }

    public AzureListener(IConfiguration configuration, Publisher publisher)
    {
        SubscriptionKey = configuration.GetConnectionString("Speech");

        Client = new HttpClient
        {
            BaseAddress = new Uri("	https://eastus.tts.speech.microsoft.com")
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
        Publisher = publisher;
    }

    public async Task<string> BeginListeningAsync()
    {
        var audioStream = new VoiceAudioStream();
        var audioFormat = AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1);
        var audioConfig = AudioConfig.FromStreamInput(audioStream);
        var speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, "eastus");
        speechConfig.SetProperty(PropertyId.Speech_LogFilename, "speechlog.txt");
        var speechClient = new SpeechRecognizer(speechConfig, audioConfig);

        speechClient.SessionStarted += SpeechClient_SessionStarted;
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
        var connection = _audioConnections.FirstOrDefault(x => x.SessionId == sessionId);
        connection?.AudioStream.Write(audioChunk, 0, audioChunk.Length);

        //var wav = ConvertOggToWav(audioChunk);
        //connection?.AudioStream.Write(wav, 0, wav.Length);

        return Task.CompletedTask;
    }

    public void SpeechClient_Canceled(object? sender, SpeechRecognitionCanceledEventArgs e)
    {
        var audioConnection = _audioConnections.FirstOrDefault(x => x.SessionId == e.SessionId);
        if (audioConnection == null) return;

        audioConnection.SpeechClient.Dispose();
        _audioConnections.Remove(audioConnection);
    }

    public void SpeechClient_SessionStarted(object? sender, SessionEventArgs e)
    {
        Publisher.Publish(new SpeechSessionStarted(e.SessionId));
    }

    public void SpeechClient_Recognizing(object? sender, SpeechRecognitionEventArgs e)
    {
        Publisher.Publish(new SpeechRecognizing(e.SessionId, e.Result.Text, e.Result.Duration.Ticks));
    }

    public void SpeechClient_Recognized(object? sender, SpeechRecognitionEventArgs e)
    {
        Publisher.Publish(new SpeechRecognized(e.SessionId, e.Result.Text, e.Result.Duration.Ticks));
    }
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

public class EchoStream : Stream
{
    public override bool CanTimeout { get; } = true;
    public override int ReadTimeout { get; set; } = Timeout.Infinite;
    public override int WriteTimeout { get; set; } = Timeout.Infinite;
    public override bool CanRead { get; } = true;
    public override bool CanSeek { get; } = false;
    public override bool CanWrite { get; } = true;

    public bool CopyBufferOnWrite { get; set; } = false;

    private readonly object _lock = new object();

    // Default underlying mechanism for BlockingCollection is ConcurrentQueue<T>, which is what we want
    private readonly BlockingCollection<byte[]> _Buffers;
    private int _maxQueueDepth = 10;

    private byte[] m_buffer = null;
    private int m_offset = 0;
    private int m_count = 0;

    private bool m_Closed = false;
    private bool m_FinalZero = false; //after the stream is closed, set to true after returning a 0 for read()
    public override void Close()
    {
        m_Closed = true;

        // release any waiting writes
        _Buffers.CompleteAdding();
    }

    public bool DataAvailable
    {
        get
        {
            return _Buffers.Count > 0;
        }
    }

    private long _Length = 0L;
    public override long Length
    {
        get
        {
            return _Length;
        }
    }

    private long _Position = 0L;
    public override long Position
    {
        get
        {
            return _Position;
        }
        set
        {
            throw new NotImplementedException();
        }
    }

    public EchoStream() : this(10)
    {
    }

    public EchoStream(int maxQueueDepth)
    {
        _maxQueueDepth = maxQueueDepth;
        _Buffers = new BlockingCollection<byte[]>(_maxQueueDepth);
    }

    // we override the xxxxAsync functions because the default base class shares state between ReadAsync and WriteAsync, which causes a hang if both are called at once
    public new Task WriteAsync(byte[] buffer, int offset, int count)
    {
        return Task.Run(() => Write(buffer, offset, count));
    }

    // we override the xxxxAsync functions because the default base class shares state between ReadAsync and WriteAsync, which causes a hang if both are called at once
    public new Task<int> ReadAsync(byte[] buffer, int offset, int count)
    {
        return Task.Run(() =>
        {
            return Read(buffer, offset, count);
        });
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (m_Closed || buffer.Length - offset < count || count <= 0)
            return;

        byte[] newBuffer;
        if (!CopyBufferOnWrite && offset == 0 && count == buffer.Length)
            newBuffer = buffer;
        else
        {
            newBuffer = new byte[count];
            System.Buffer.BlockCopy(buffer, offset, newBuffer, 0, count);
        }
        if (!_Buffers.TryAdd(newBuffer, WriteTimeout))
            throw new TimeoutException("EchoStream Write() Timeout");

        _Length += count;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
            return 0;
        lock (_lock)
        {
            if (m_count == 0 && _Buffers.Count == 0)
            {
                if (m_Closed)
                {
                    if (!m_FinalZero)
                    {
                        m_FinalZero = true;
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }

                if (_Buffers.TryTake(out m_buffer, ReadTimeout))
                {
                    m_offset = 0;
                    m_count = m_buffer.Length;
                }
                else
                {
                    if (m_Closed)
                    {
                        if (!m_FinalZero)
                        {
                            m_FinalZero = true;
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            int returnBytes = 0;
            while (count > 0)
            {
                if (m_count == 0)
                {
                    if (_Buffers.TryTake(out m_buffer, 0))
                    {
                        m_offset = 0;
                        m_count = m_buffer.Length;
                    }
                    else
                        break;
                }

                var bytesToCopy = (count < m_count) ? count : m_count;
                System.Buffer.BlockCopy(m_buffer, m_offset, buffer, offset, bytesToCopy);
                m_offset += bytesToCopy;
                m_count -= bytesToCopy;
                offset += bytesToCopy;
                count -= bytesToCopy;

                returnBytes += bytesToCopy;
            }

            _Position += returnBytes;

            return returnBytes;
        }
    }

    public override int ReadByte()
    {
        byte[] returnValue = new byte[1];
        return (Read(returnValue, 0, 1) <= 0 ? -1 : (int)returnValue[0]);
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }
}
