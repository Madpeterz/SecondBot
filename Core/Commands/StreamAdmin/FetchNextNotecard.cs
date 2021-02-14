using System;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using RestSharp;
using Newtonsoft.Json;

namespace BSB.Commands.StreamAdmin
{
    public class FetchNextNotecard : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "TEXT","TEXT" }; } }
        public override string[] ArgHints { get { return new[] { "Server endpoint","Server endpoint code" }; } }
        public override string Helpfile { get { return "[StreamAdmin]\nThis command is part of the streamadmin commands set\n No end user support will be given."; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                string attempt_endpoint = "" + args[0] + "endpoint.php";
                string token = helpers.GetSHA1(helpers.UnixTimeNow().ToString()+"notecardnext" + args[1]);
                var client = new RestClient("" + args[0] + "endpoint.php");
                var request = new RestRequest("notecard/next", Method.POST);
                request.AddParameter("token", token);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                IRestResponse endpoint_checks = client.Post(request);
                if (endpoint_checks.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        notecardendpoint server_reply = JsonConvert.DeserializeObject<notecardendpoint>(endpoint_checks.Content);
                        if (server_reply.status == true)
                        {
                            if (server_reply.NotecardTitle.Length > 3)
                            {
                                return bot.SendNotecard(server_reply.NotecardTitle, server_reply.NotecardContent, (UUID)server_reply.AvatarUUID);
                            }
                            else
                            {
                                return Failed("Notecard title is to short - "+ endpoint_checks.Content);
                            }
                        }
                        return Failed(server_reply.message);
                    }
                    catch (Exception e)
                    {
                        return Failed("Error: "+e.Message+"");
                    }
                }
                else
                {
                    return Failed("HTTP error DataObject broken Status:"+ endpoint_checks.StatusCode.ToString()+" @ "+ attempt_endpoint+"\n Message given (if any): "+endpoint_checks.Content);
                }
            }
            return Failed("incorrect number of args");
        }
    }

    public class notecardendpoint : endpoint
    { 
        public string AvatarUUID { get; set; }
        public string NotecardTitle { get; set; }
        public string NotecardContent { get; set; }
    }

    public class endpoint
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
}
