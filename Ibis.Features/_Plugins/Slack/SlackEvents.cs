using Sparc.Core;
using Sparc.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ibis.Features._Plugins.Slack.Entities;
using System.Text.Json.Serialization;
using Ibis.Features.Rooms;
using Microsoft.Extensions.Hosting;
using Sparc.Authentication.AzureADB2C;

namespace Ibis.Features._Plugins.Slack
{
    public class SlackEvents : PublicFeature<Postobject, string>
    {

        private readonly IConfiguration _config;
        public IRepository<Room> Rooms { get; set; }
        public IRepository<User> Users { get; set; }

        public IRepository<Message> Messages { get;  }
        public SlackEvents(IConfiguration config, IRepository<Room> rooms, IRepository<User> users, IRepository<Message> messages)
        {
            _config = config;
            Rooms = rooms;
            Users = users;
            Messages = messages;
        }

        public override async Task<string> ExecuteAsync(Postobject Json)
        {
            Postobject data = new Postobject();
            var returnStr = "";
            data = Json;

            // checks slack verification
            if (_config["Slack:Verify"] == Json.token)
            {
                if (Json.challenge != null)
                {
                    returnStr = Json.challenge;
                }

                if (Json._event != null)
                {
                    var usersearch = Users.Query.Where(u => u.FirstName.ToLower() == Json._event.attachments[0].author_name.ToLower()).FirstOrDefault();
                    var user = await Users.FindAsync(usersearch.Id);
                    var command = Json._event.text.Split(" ")[0];


                    if (command == "post")
                    {
                        var tag = Json._event.text.Split(" ")[1];
                        var existing = await Rooms.FindAsync(tag);

                        //if adding image or other files
                        if (Json._event.attachments != null)
                        {
                            foreach (var attachment in Json._event.attachments)
                            {
                                //attachment.image_url
                            }
                        }

                        if (existing != null)
                        {
                            Message message = new Message(existing.RoomId, user, Json._event.text);
                            await Messages.AddAsync(message);
                            var str = "/rooms/" + existing.RoomId;
                        } else
                        {
                            Room room = new(tag);
                            await Rooms.AddAsync(room);

                            string msgstr = String.Join(" ", Json._event.text.Split(" ").Skip(2));

                            Message message = new Message(room.RoomId, user, msgstr);
                            await Messages.AddAsync(message);
                            var str = "/rooms/" + room.RoomId;
                            returnStr = str;
                        }
                    }

                    //posts to channel with updated room

                    Payload payload = new Payload()
                    {
                        Channel = "C040XKHTEMA",
                        Text = returnStr
                    };

                   await new SlackEngine(_config).SlackApiPost(payload);
                }

            }
            else
            {

                returnStr = "Incorrect token";

                Payload payload = new Payload()
                {
                    Channel = "C040XKHTEMA",
                    Text = returnStr
                };
                await new SlackEngine(_config).SlackApiPost(payload);
            }

            return returnStr;
        }
    }
}