using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ibis.Features._Plugins.Slack.Entities
{
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

    }
}
