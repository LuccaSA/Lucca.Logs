using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Lucca.Logs.Shared.Exceptional
{
    /// <summary>
    /// From : https://github.com/NickCraver/StackExchange.Exceptional/blob/main/src/StackExchange.Exceptional.Shared/Error.cs
    /// </summary>
    [Serializable]
    public class Error
    {

        /// <summary>
        /// The ID on this error, strictly for primary keying on persistent stores.
        /// </summary>
        [JsonIgnore]
        public long Id { get; set; }

        /// <summary>
        /// Unique identifier for this error, generated on the server it came from.
        /// </summary>
        public Guid GUID { get; set; }
         
        /// <summary>
        /// Only allocate this dictionary if there's a need.
        /// </summary>
        private void InitCustomData() => CustomData = CustomData ?? new Dictionary<string, string>();

        /// <summary>
        /// Whether this error is protected from deletion.
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        /// For notifier usage - whether this error is a duplicate (already seen recently).
        /// Recent is defined by the <see cref="ErrorStoreSettings.RollupPeriod"/> setting.
        /// </summary>
        [JsonIgnore]
        public bool IsDuplicate { get; set; }

        /// <summary>
        /// The <see cref="Exception"/> instance used to create this error.
        /// </summary>
        [JsonIgnore]
        public Exception Exception { get; set; }

        /// <summary>
        /// The name of the application that threw this exception.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The category of this error, usage is up to the user.
        /// It could be a tag, or severity, etc.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The hostname where the exception occurred.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// The type error.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The source of this error.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Exception message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The detail/stack trace of this error.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// The hash that describes this error.
        /// </summary>
        public int? ErrorHash { get; set; }

        /// <summary>
        /// The time in UTC that the error originally occurred.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The time in UTC that the error last occurred.
        /// </summary>
        public DateTime? LastLogDate { get; set; }

        /// <summary>
        /// The HTTP Status code associated with the request.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// The server variables collection for the request.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection ServerVariables { get; set; }

        /// <summary>
        /// The query string collection for the request.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection QueryString { get; set; }

        /// <summary>
        /// The form collection for the request.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection Form { get; set; }

        /// <summary>
        /// A collection representing the client cookies of the request.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection Cookies { get; set; }

        /// <summary>
        /// A collection representing the headers sent with the request.
        /// </summary>
        [JsonIgnore]
        public NameValueCollection RequestHeaders { get; set; }

        /// <summary>
        /// A collection of custom data added at log time.
        /// </summary>
        public Dictionary<string, string> CustomData { get; set; }

        /// <summary>
        /// The number of newer Errors that have been discarded because they match this Error and fall 
        /// within the configured <see cref="ErrorStoreSettings.RollupPeriod"/> <see cref="TimeSpan"/> value.
        /// </summary>
        public int? DuplicateCount { get; set; }

        /// <summary>
        /// The commands associated with this error. For example: SQL queries, Redis commands, elastic queries, etc.
        /// </summary>
        public List<Command> Commands { get; set; }

        /// <summary>
        /// Date this error was deleted (for stores that support deletion and retention, e.g. SQL)
        /// </summary>
        public DateTime? DeletionDate { get; set; }

        /// <summary>
        /// The URL host of the request causing this error.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The URL *path* of the request causing this error, e.g. /MyContoller/MyAction
        /// </summary>
        [JsonProperty("Url")] // Legacy compatibility
        public string UrlPath { get; set; }

        /// <summary>
        /// The complete URL of the request causing this error.
        /// </summary>
        public string FullUrl { get; set; }

        /// <summary>
        /// The HTTP Method causing this error, e.g. GET or POST.
        /// </summary>
        public string HTTPMethod { get; set; }

        /// <summary>
        /// The IPAddress of the request causing this error.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// JSON populated from database stored, deserialized after if needed.
        /// </summary>
        [JsonIgnore]
        public string FullJson { get; set; }

        /// <summary>
        /// Returns the value of the <see cref="Message"/> property.
        /// </summary>
        public override string ToString() => Message;
        
        /// <summary>
        /// Variables strictly for JSON serialization, to maintain non-dictionary behavior.
        /// </summary>
        public List<NameValuePair> ServerVariablesSerializable
        {
            get => GetPairs(ServerVariables);
            set => ServerVariables = GetNameValueCollection(value);
        }
        /// <summary>
        /// Variables strictly for JSON serialization, to maintain non-dictionary behavior.
        /// </summary>
        public List<NameValuePair> QueryStringSerializable
        {
            get => GetPairs(QueryString);
            set => QueryString = GetNameValueCollection(value);
        }
        /// <summary>
        /// Variables strictly for JSON serialization, to maintain non-dictionary behavior.
        /// </summary>
        public List<NameValuePair> FormSerializable
        {
            get => GetPairs(Form);
            set => Form = GetNameValueCollection(value);
        }
        /// <summary>
        /// Variables strictly for JSON serialization, to maintain non-dictionary behavior.
        /// </summary>
        public List<NameValuePair> CookiesSerializable
        {
            get => GetPairs(Cookies);
            set => Cookies = GetNameValueCollection(value);
        }
        /// <summary>
        /// Variables strictly for JSON serialization, to maintain non-dictionary behavior.
        /// </summary>
        public List<NameValuePair> RequestHeadersSerializable
        {
            get => GetPairs(RequestHeaders);
            set => RequestHeaders = GetNameValueCollection(value);
        }

        /// <summary>
        /// Gets a JSON representation for this error.
        /// </summary>
        public string ToJson() => JsonConvert.SerializeObject(this);
         
        /// <summary>
        /// Deserializes provided JSON into an Error object.
        /// </summary>
        /// <param name="json">JSON representing an Error.</param>
        /// <returns>The Error object.</returns>
        public static Error FromJson(string json) => JsonConvert.DeserializeObject<Error>(json);

        /// <summary>
        /// Serialization class in place of the NameValueCollection pairs.
        /// </summary>
        /// <remarks>This exists because things like a querystring can halve multiple values, they are not a dictionary.</remarks>
        public class NameValuePair
        {
            /// <summary>
            /// The name for this variable.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The value for this variable.
            /// </summary>
            public string Value { get; set; }
        }

        private List<NameValuePair> GetPairs(NameValueCollection nvc)
        {
            var result = new List<NameValuePair>();
            if (nvc == null) return null;

            for (int i = 0; i < nvc.Count; i++)
            {
                result.Add(new NameValuePair { Name = nvc.GetKey(i), Value = nvc.Get(i) });
            }
            return result;
        }

        private NameValueCollection GetNameValueCollection(List<NameValuePair> pairs)
        {
            var result = new NameValueCollection();
            if (pairs == null) return null;

            foreach (var p in pairs)
            {
                result.Add(p.Name, p.Value);
            }
            return result;
        }

         
    }
}
