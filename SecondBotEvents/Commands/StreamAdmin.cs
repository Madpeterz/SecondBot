using Newtonsoft.Json;
using OpenMetaverse;
using RestSharp;
using SecondBotEvents.Services;
using System;

namespace SecondBotEvents.Commands
{
    public class StreamAdmin : CommandsAPI
    {
        public StreamAdmin(EventsSecondBot setmaster) : base(setmaster)
        {
        }


        [About("A streamadin command")]
        [ReturnHints("True|False")]
        [ReturnHintsFailure("Bad reply:  ...")]
        [ReturnHintsFailure("Endpoint is empty")]
        [ReturnHintsFailure("Endpointcode is empty")]
        [ReturnHintsFailure("HTTP status code: ...")]
        [ReturnHintsFailure("Error: ...")]
        [ReturnHintsFailure("Notecard title is to short")]
        [ArgHints("endpoint", "The end point")]
        [ArgHints("endpointcode", "The end point code")]
        public object FetchNextNotecard(string endpoint, string endpointcode)
        {
            if (SecondbotHelpers.notempty(endpoint) == false)
            {
                return Failure("Endpoint is empty", new [] { endpoint, endpointcode });
            }
            if (SecondbotHelpers.notempty(endpointcode) == false)
            {
                return Failure("Endpointcode is empty", new [] { endpoint, endpointcode });
            }

            string attempt_endpoint = endpoint + "sys.php";
            string token = SecondbotHelpers.GetSHA1(SecondbotHelpers.UnixTimeNow().ToString() + "NotecardNext" + endpointcode);
            var client = new RestClient(attempt_endpoint);
            var request = new RestRequest("Notecard/Next", Method.Post);
            string unixtime = SecondbotHelpers.UnixTimeNow().ToString();
            request.AddParameter("token", token);
            request.AddParameter("unixtime", unixtime);
            request.AddParameter("method", "Notecard");
            request.AddParameter("action", "Next");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            RestResponse endpoint_checks = client.ExecutePostAsync(request).Result;
            if (endpoint_checks.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return Failure("HTTP status code: " + endpoint_checks.StatusCode.ToString(), new [] { endpoint, endpointcode });
            }
            try
            {
                NotecardEndpoint server_reply = JsonConvert.DeserializeObject<NotecardEndpoint>(endpoint_checks.Content);
                if (server_reply.status == false)
                {
                    return Failure("Bad reply: " + server_reply.message, new [] { endpoint, endpointcode });
                }
                if (server_reply.NotecardTitle.Length < 3)
                {
                    return Failure("Notecard title is to short", new [] { endpoint, endpointcode });
                }
                ProcessAvatar(server_reply.AvatarUUID);
                if(avataruuid == UUID.Zero)
                {
                    return Failure("Unable to unpack avatar", new[] { endpoint, endpointcode });
                }
                bool result = master.botClient.SendNotecard(server_reply.NotecardTitle, server_reply.NotecardContent, avataruuid);
                if (result == false)
                {
                    return Failure("Failed to create/send notecard", new[] { endpoint, endpointcode });
                }
                return BasicReply("ok");
            }
            catch (Exception e)
            {
                return Failure("Error: " + e.Message + "", new [] { endpoint, endpointcode });
            }
        }

        public class NotecardEndpoint : BasicEndpoint
        {
            public string AvatarUUID { get; set; }
            public string NotecardTitle { get; set; }
            public string NotecardContent { get; set; }
        }

        public class BasicEndpoint
        {
            public bool status { get; set; }
            public string message { get; set; }
        }
    }
}
