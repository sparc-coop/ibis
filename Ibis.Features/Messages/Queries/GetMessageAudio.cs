namespace Ibis.Messages;

public record GetMessageAudioRequest(string MessageId, string? VoiceId);
public class GetMessageAudio : Feature<GetMessageAudioRequest, AudioMessage?>
{
    public GetMessageAudio(ISpeaker synthesizer, IRepository<Message> messages)
    {
        Synthesizer = synthesizer;
        Messages = messages;
    }

    public ISpeaker Synthesizer { get; }
    public IRepository<Message> Messages { get; }

    public override async Task<AudioMessage?> ExecuteAsync(GetMessageAudioRequest request)
    {
        var message = await Messages.FindAsync(request.MessageId) 
            ?? throw new NotFoundException($"Message {request.MessageId} not found");
        
        if (message.Audio?.Url == null || (request.VoiceId != null && message.Audio.Voice != request.VoiceId))
        {
            var audio = await message.SpeakAsync(Synthesizer, request.VoiceId);
            if (request.VoiceId == null) // Only save in original voice
                await Messages.UpdateAsync(message);
            
            return audio;
        }
        
        return message.Audio;
    }
}
