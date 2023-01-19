using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Text;
using CoreTweet.Core;

namespace CoreTweet.V2
{
    public class DMEvent : CoreBase
    {
        /// <summary>
        /// The id of the Direct Message event.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// The text included in the Direct Message.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// The type of event.
        /// </summary>
        [JsonProperty("event_type")]
        public DMEventType EventType { get; set; }

        /// <summary>
        /// The timestamp of the Direct Message event creation.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// The id of the user who sent the Direct Message.
        /// </summary>
        [JsonProperty("sender_id")]
        public long? SenderId { get; set; }

        /// <summary>
        /// The id of the conversation the Direct Message belongs to.
        /// </summary>
        [JsonProperty("dm_conversation_id")]
        public string DMConversationId { get; set; }

        /// <summary>
        /// The attached urls and media information for expansion.
        /// </summary>
        [JsonProperty("attachments")]
        public DMAttachments Attachments { get; set; }

        /// <summary>
        /// Expansion of a "shared" Tweet in the Direct Message.
        /// </summary>
        [JsonProperty("referenced_tweets")]
        public DMReferencedTweet[] ReferencedTweets { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [JsonProperty("participant_ids")]
        public long[] ParticipantIds { get; set; }
    }

    public class DMAttachments : CoreBase
    {
        /// <summary>
        /// List of unique identifiers of media attached to a direct message.
        /// </summary>
        [JsonProperty("media_keys")]
        public string[] MediaKeys { get; set; }

        /// <summary>
        /// Undocumented
        /// </summary>
        [JsonProperty("card_ids")]
        public string[] CardIds { get; set; }
    }

    public class DMReferencedTweet : CoreBase
    {
        /// <summary>
        /// The id of a "shared" Tweet in the Direct Message.
        /// </summary>
        public long Id { get; set; }
    }

    public class DMEventsResponse : ResponseBase
    {
        [JsonProperty("data")]
        public DMEvent[] Data { get; set; }

        [JsonProperty("includes")]
        public DMEventResponseIncludes Includes { get; set; }

        [JsonProperty("meta")]
        public DMEventsMeta Meta { get; set; }
    }

    public class DMEventResponseIncludes : CoreBase
    {
        [JsonProperty("tweets")]
        public Tweet[] Tweets { get; set; }

        [JsonProperty("users")]
        public User[] Users { get; set; }

        [JsonProperty("media")]
        public Media[] Media { get; set; }
    }

    public class DMEventsMeta : CoreBase
    {
        /// <summary>
        /// Included when there is an additional 'page' of data to request.
        /// </summary>
        [JsonProperty("next_token")]
        public string NextToken { get; set; }

        /// <summary>
        /// Included when there is an additional 'page' of data to request.
        /// </summary>
        [JsonProperty("previous_token")]
        public string PreviousToken { get; set; }

        /// <summary>
        /// Indicated the number of DMEvents in the response.
        /// </summary>
        [JsonProperty("result_count")]
        public int ResultCount { get; set; }
    }

    public class ManageDMResponse : ResponseBase
    {
        /// <summary>
        /// the id of the Direct Message conversation the Direct Message was added to.
        /// </summary>
        [JsonProperty("dm_conversation_id")]
        public string DMConversationId { get; set; }

        /// <summary>
        /// the id of the event created by this request.
        /// </summary>
        [JsonProperty("dm_event_id")]
        public long DMEventId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DMEventType
    {
        [EnumMember(Value = "MessageCreate")]
        MessageCreate,
        [EnumMember(Value = "ParticipantsJoin")]
        ParticipantsJoin,
        [EnumMember(Value = "ParticipantsLeave")]
        ParticipantsLeave
    }

    [Flags]
    public enum DMEventTypes
    {
        None              = 0x00000000,
        MessageCreate     = 0x00000001,
        ParticipantsJoin  = 0x00000002,
        ParticipantsLeave = 0x00000004,
        All               = 0x00000007,
    }

    public static class DMEventTypesExtensions
    {
        public static string ToQueryString(this DMEventTypes value)
        {
            if (value == DMEventTypes.None)
                return "";

            var builder = new StringBuilder();

            if ((value & DMEventTypes.MessageCreate) != 0)
                builder.Append("MessageCreate,");
            if ((value & DMEventTypes.ParticipantsJoin) != 0)
                builder.Append("ParticipantsJoin,");
            if ((value & DMEventTypes.ParticipantsLeave) != 0)
                builder.Append("ParticipantsLeave,");

            return builder.ToString(0, builder.Length - 1);
        }
    }

    /// <summary>
    /// List of fields to return in the Direct Message Event object. By default, the endpoint only returns <see cref="DMEventFields.Id"/> and <see cref="DMEventFields.EventType"/>.
    /// </summary>
    [Flags]
    public enum DMEventFields
    {
        None             = 0x00000000,
        Id               = 0x00000001,
        Text             = 0x00000002,
        EventType        = 0x00000004,
        CreatedAt        = 0x00000008,
        DMConversationId = 0x00000010,
        SenderId         = 0x00000020,
        ParticipantIds   = 0x00000040,
        ReferencedTweets = 0x00000080,
        Attachments      = 0x00000100,
        All              = 0x000001ff,
    }

    public static class DMEventFieldsExtensions
    {
        public static string ToQueryString(this DMEventFields value)
        {
            if (value == DMEventFields.None)
                return "";

            var builder = new StringBuilder();

            if ((value & DMEventFields.Id) != 0)
                builder.Append("id,");
            if ((value & DMEventFields.Text) != 0)
                builder.Append("text,");
            if ((value & DMEventFields.EventType) != 0)
                builder.Append("event_type,");
            if ((value & DMEventFields.CreatedAt) != 0)
                builder.Append("created_at,");
            if ((value & DMEventFields.DMConversationId) != 0)
                builder.Append("dm_conversation_id,");
            if ((value & DMEventFields.SenderId) != 0)
                builder.Append("sender_id,");
            if ((value & DMEventFields.ParticipantIds) != 0)
                builder.Append("participant_ids,");
            if ((value & DMEventFields.ReferencedTweets) != 0)
                builder.Append("referenced_tweets,");
            if ((value & DMEventFields.Attachments) != 0)
                builder.Append("attachments,");

            return builder.ToString(0, builder.Length - 1);
        }
    }

    /// <summary>
    /// List of fields to return in the Direct Message conversation events object.
    /// </summary>
    [Flags]
    public enum DMEventExpansions
    {
        None                 = 0x00000000,
        AttachmentsMediaKeys = 0x00000001,
        ReferencedTweetsId   = 0x00000002,
        SenderId             = 0x00000004,
        ParticipantIds       = 0x00000008,
        All                  = 0x0000000f,
    }

    public static class DMEventExpansionsExtensions
    {
        public static string ToQueryString(this DMEventExpansions value)
        {
            if (value == DMEventExpansions.None)
                return "";

            var builder = new StringBuilder();

            if ((value & DMEventExpansions.AttachmentsMediaKeys) != 0)
                builder.Append("attachments.media_keys,");
            if ((value & DMEventExpansions.ReferencedTweetsId) != 0)
                builder.Append("referenced_tweets.id,");
            if ((value & DMEventExpansions.SenderId) != 0)
                builder.Append("sender_id,");
            if ((value & DMEventExpansions.ParticipantIds) != 0)
                builder.Append("participant_ids,");

            return builder.ToString(0, builder.Length - 1);
        }
    }
}
