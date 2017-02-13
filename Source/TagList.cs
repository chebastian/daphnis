using Newtonsoft.Json;
using System.Collections.Generic;

namespace BorderlessAlphaWin
{
    internal class TagList
    {
        public TagList()
        {
            Tags = new List<Tag>();
        }

        [JsonProperty("Tags")]
        public List<Tag> Tags
        {
            get; set;
        }
    }
}