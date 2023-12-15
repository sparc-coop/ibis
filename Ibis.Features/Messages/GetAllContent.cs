﻿using System.Collections.Concurrent;

namespace Ibis.Messages;
public record GetAllContentRequest(string RoomSlug, string? Language = null, List<string>? Messages = null, Dictionary<string, string>? Tags = null, int? Take = null);
public record GetAllContentResponse(string Name, string Slug, string Language, List<Message> Content);
public class GetAllContent : PublicFeature<GetAllContentRequest, GetAllContentResponse>
{
    public GetAllContent(IRepository<Message> messages, IRepository<Room> rooms, IRepository<User> users, ITranslator translator, TypeMessage typeMessage)
    {
        Messages = messages;
        Rooms = rooms;
        Users = users;
        Translator = translator;
        TypeMessage = typeMessage;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public ITranslator Translator { get; }
    public TypeMessage TypeMessage { get; }

    public override async Task<GetAllContentResponse> ExecuteAsync(GetAllContentRequest request)
    {
        var user = await Users.GetAsync(User);
        var room = await GetRoomAsync(request.RoomSlug, user);
        var language = request.Language ?? user?.Avatar.Language ?? room.Languages.First().Id;
        
        // Change the publish strategy so this call doesn't return until EVERYTHING is done
        ((Rooms as CosmosDbRepository<Room>)?.Context as BlossomContext)?.SetPublishStrategy(PublishStrategy.ParallelWhenAll);

        var roomMessages = await Messages.Query.Where(x => x.RoomId == room.RoomId).ToListAsync();
        await UpdateMessageDeprecationStatus(roomMessages, request.Messages);

        if (request.Messages?.Any() == true)
            await PostNewContentAsync(room.RoomId, request.Messages ?? new(), user);
        
        await AddLanguageIfNeededAsync(room, language);

        var content = await GetAllMessagesInUserLanguageAsync(request, room, language);

        return new(room.Name, room.Slug, language, content);
    }

    private async Task<List<Message>> GetAllMessagesInUserLanguageAsync(GetAllContentRequest request, Room room, string language)
    {
        IQueryable<Message> query = Messages.Query(room.Id)
                    .Where(x => x.Language == language && x.Text != null)
                    .OrderBy(y => y.Timestamp);

        if (request.Take != null)
            query = query.Take(request.Take.Value);

        List<Message> postList = await query.ToListAsync();

        // Optionally filter the content by tag
        if (request.Tags != null)
        {
            foreach (var tag in request.Tags.Keys)
            {
                postList = postList
                    .Where(x => x.Tags.Any(y => y.Key == tag && y.Value == request.Tags[tag]))
                    .ToList();
            }
        }

        return postList;
    }

    private async Task AddLanguageIfNeededAsync(Room room, string languageId)
    {
        if (!room.Languages.Any(x => x.Id == languageId))
        {
            var language = await Translator.GetLanguageAsync(languageId) 
                ?? throw new Exception("Language not found!");
            
            room.AddLanguage(language);
            await Rooms.UpdateAsync(room);

            var messages = await Messages.Query
                .Where(x => x.RoomId == room.RoomId && x.SourceMessageId == null)
                .ToListAsync();

            var translatedMessages = new ConcurrentBag<Message>();
            await Parallel.ForEachAsync(messages, async (message, token) =>
            {
                var result = await room.TranslateAsync(message, Translator);
                result.ForEach(x => translatedMessages.Add(x));
            });

            await Messages.UpdateAsync(messages);
            try
            {
                await Messages.AddAsync(translatedMessages);
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
    
    private async Task PostNewContentAsync(string roomId, List<string> incomingMessages, User? user)
    {
        var messages = await Messages.Query
                    .Where(x => x.RoomId == roomId && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        var newContent = incomingMessages.Where(x => !messages.Any(y => y.Tag == x)).ToList();

        foreach (var message in newContent)
        {
            var request = new TypeMessageRequest(roomId, message, message);
            await TypeMessage.ExecuteAsUserAsync(request, user ?? Ibis.Users.User.System);
        }
    }

    private async Task<Room> GetRoomAsync(string slug, User? user)
    {
        var room = slug.StartsWith("http")
            ? Rooms.Query.FirstOrDefault(x => x.Name == slug)
            : Rooms.Query.FirstOrDefault(x => x.Id == slug || x.Slug == slug);
        if (room == null)
        {
            room = new Room(slug, "Content", user ?? Ibis.Users.User.System);
            await Rooms.AddAsync(room);
        }

        return room;
    }
    private async Task UpdateMessageDeprecationStatus(List<Message> roomMessages, List<string> currentHtmlContent)
    {
        foreach (var message in roomMessages)
        {
            message.IsDeprecated = currentHtmlContent == null || !currentHtmlContent.Contains(message.Text);
        }

        await Messages.UpdateAsync(roomMessages);
    }
}
