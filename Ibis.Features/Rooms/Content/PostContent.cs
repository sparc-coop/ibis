﻿namespace Ibis.Features.Rooms;
public record PostContentRequest(string RoomSlug, string Language, List<string> Messages, bool AsHtml = false);
public class PostContent : PublicFeature<PostContentRequest, GetAllContentResponse>
{
    public PostContent(IRepository<Message> messages, IRepository<Room> rooms, IRepository<User> users, ITranslator translator, TypeMessage typeMessage)
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

    public override async Task<GetAllContentResponse> ExecuteAsync(PostContentRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var room = await GetRoomAsync(request.RoomSlug, user);

        await AddLanguageIfNeeded(room, request.Language);

        var result = await GetAllMessagesAsync(request, room);

        var untranslatedMessages = request.Messages.Where(x => !result.Any(y => y.Tag == x)).ToList();
        if (untranslatedMessages.Any())
        {
            await AddAdditionalMessages(room.Id, untranslatedMessages, user);
            result = await GetAllMessagesAsync(request, room);
        }

        result = result.Where(x => request.Messages.Contains(x.Tag)).ToList();

        return new(room.Name, room.Slug, result);
    }

    private async Task AddAdditionalMessages(string roomId, List<string> additionalMessages, User? user)
    {
        foreach (var message in additionalMessages)
            await TypeMessage.ExecuteAsUserAsync(new(roomId, message, message), user ?? Features.Users.User.System);
    }

    private async Task<List<GetContentResponse>> GetAllMessagesAsync(PostContentRequest request, Room room)
    {
        List<Message> postList = await Messages.Query
                    .Where(x => x.RoomId == room.Id && x.Language == request.Language && x.Text != null)
                    .OrderByDescending(y => y.Timestamp)
                    .ToListAsync();

        List<GetContentResponse> result = new();
        foreach (var message in postList)
        {
            result.Add(new(message.Tag ?? message.Id, request.AsHtml ? message.Html() : message.Text!, message.Language, message.Audio?.Url, message.Timestamp));
        }

        return result;
    }

    private async Task AddLanguageIfNeeded(Room room, string languageId)
    {
        if (!room.Languages.Any(x => x.Id == languageId))
        {
            var language = await Translator.GetLanguageAsync(languageId);
            if (language == null)
                throw new Exception("Language not found!");

            room.AddLanguage(language);
            await Rooms.UpdateAsync(room);
        }
    }

    private async Task<Room> GetRoomAsync(string slug, User? user)
    {
        var room = Rooms.Query.FirstOrDefault(x => x.Slug == slug);
        if (room == null)
        {
            if (user != null)
            {
                room = new Room(slug, user);
                await Rooms.AddAsync(room);
            }
            else
            {
                throw new NotFoundException($"Room {slug} not found");
            }
        }

        return room;
    }
}