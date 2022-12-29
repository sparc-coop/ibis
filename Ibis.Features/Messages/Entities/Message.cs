﻿using Markdig;

namespace Ibis.Features.Messages;

public record Word(long Offset, long Duration, string Text);
public record EditHistory(DateTime Timestamp, string Text);
public class Message : Root<string>
{
    public string RoomId { get; private set; }
    public string? SourceMessageId { get; private set; }
    public string Language { get; protected set; }
    public DateTime Timestamp { get; private set; }
    public DateTime? LastModified { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public UserAvatar User { get; private set; }
    public AudioMessage? Audio { get; private set; }
    public string? Text { get; private set; }
    public List<MessageTranslation> Translations { get; private set; }
    public decimal Charge { get; private set; }
    public string? Tag { get; set; }
    public List<MessageTag> Tags { get; set; }
    public List<EditHistory> EditHistory { get; private set; }
    public string Html => Markdown.ToHtml(Text ?? string.Empty);

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        User = new User().Avatar;
        Language = "";
        Translations = new();
        EditHistory = new();
        Tags = new();
    }

    public Message(string roomId, User user, string text, string? tag = null) : this()
    {
        RoomId = roomId;
        User = user.Avatar;
        Language = user.Avatar.Language ?? "";
        Audio = user.Avatar.Voice == null ? null : new(null, 0, user.Avatar.Voice);
        Timestamp = DateTime.UtcNow;
        Tag = tag;
        SetText(text);
    }

    public Message(Message sourceMessage, string toLanguage, string text, List<MessageTag> translatedTags) : this()
    {
        RoomId = sourceMessage.RoomId;
        SourceMessageId = sourceMessage.Id;
        User = new(sourceMessage.User);
        Audio = sourceMessage.Audio?.Voice == null ? null : new(null, 0, new(sourceMessage.Audio.Voice));
        Language = toLanguage;
        Timestamp = DateTime.UtcNow;
        Tag = sourceMessage.Tag;
        SetText(text);
        SetTags(sourceMessage.Tags);
        SetTags(translatedTags, false);
    }

    public void SetText(string text)
    {
        if (Text == text)
            return;

        if (Text != null)
            EditHistory.Add(new(LastModified ?? Timestamp, Text));

        Text = text;
        LastModified = DateTime.UtcNow;
        
        Broadcast(new MessageTextChanged(this));
    }

    internal async Task<AudioMessage?> SpeakAsync(ISpeaker engine, string? voiceId = null)
    {
        if (voiceId == null && Audio?.Voice == null)
            return null;

        var audio = await engine.SpeakAsync(this, voiceId);

        if (Audio == null)
        {
            Audio = audio;
            Broadcast(new MessageAudioChanged(this));
        }

        return audio;
    }

    internal bool HasTranslation(string languageId)
    {
        return Language == languageId
            || (Translations != null && Translations.Any(x => x.LanguageId == languageId));
    }
    
    internal void AddTranslation(Message translatedMessage)
    {
        if (HasTranslation(translatedMessage.Language))
        {
            // Set the newly translated message's ID to the existing translation so that it is updated in the repository
            var translation = Translations.FirstOrDefault(x => x.LanguageId == translatedMessage.Language);
            if (translation != null)
                translatedMessage.Id = translation.SourceMessageId;
        }
        else
        {
            Translations.Add(new(translatedMessage.Language, translatedMessage.Id));
        }
    }

    internal void SetTags(List<MessageTag> tags, bool fullReplace = true)
    {
        var keys = tags.Select(x => x.Key).ToList();
        if (fullReplace)
            Tags.RemoveAll(x => !keys.Contains(x.Key));
        
        foreach (var tag in tags)
        {
            var existing = Tags.FirstOrDefault(x => x.Key == tag.Key);
            if (existing != null)
                existing.Value = tag.Value;
            else
                Tags.Add(new(tag.Key, tag.Value, SourceMessageId == null && tag.Translate));
        }

        if (tags.Any(x => x.Translate))
            Broadcast(new MessageTextChanged(this));
    }

    internal void AddCharge(decimal cost, string description)
    {
        Charge += cost;
        Broadcast(new CostIncurred(this, description, cost));
    }

    internal void Delete()
    {
        DeletedDate = DateTime.UtcNow;
        Broadcast(new MessageDeleted(this));
    }
}
