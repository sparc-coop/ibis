using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ibis.Features._Plugins.Slack.Entities
{
    public class Event
    {
        public string? type { get; set; }
        public string? channel { get; set; }
        public string? user { get; set; }
        public string? text { get; set; }
        public string? ts { get; set; }
        public Edited? edited { get; set; }
        public Attachment[]? attachments { get; set; }
    }

    public class Attachment
    {
        public string? fallback { get; set; }
        public string? color { get; set; }
        public string? pretext { get; set; }
        public string? author_name { get; set; }
        public string? author_link { get; set; }
        public string? author_icon { get; set; }
        public string? title { get; set; }
        public string? title_link { get; set; }
        public string? text { get; set; }
        public Field[]? fields { get; set; }
        public string? image_url { get; set; }
        public string? thumb_url { get; set; }
        public string? footer { get; set; }
        public string? footer_icon { get; set; }
        public int? ts { get; set; }
    }

    public class Field
    {
        public string? title { get; set; }
        public string? value { get; set; }
        public bool? _short { get; set; }
    }


    public class Edited
    {
        public string? user { get; set; }
        public string? ts { get; set; }
    }


    public class Postobject
    {
        [JsonPropertyName("event")]
        public Event? _event { get; set; }

        [JsonPropertyName("token")]
        public string? token { get; set; }
        public string? challenge { get; set; }
        public string? team_id { get; set; }
        public string? api_app_id { get; set; }
        public string? type { get; set; }
        public string[]? authed_users { get; set; }
        public Authorization[]? authorizations { get; set; }
        public string? event_id { get; set; }
        public string? event_context { get; set; }
        public int? event_time { get; set; }
    }

    public class Item
    {
        public string? type { get; set; }
        public string? channel { get; set; }
        public string? ts { get; set; }
    }

    public class Authorization
    {
        public string? enterprise_id { get; set; }
        public string? team_id { get; set; }
        public string? user_id { get; set; }
        public bool? is_bot { get; set; }
    }
}
