using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
                    notecardendpoint server_reply = new notecardendpoint();
                    bool server_reply_ok = true;

                    try
                    {
                        server_reply = JsonConvert.DeserializeObject<notecardendpoint>(endpoint_checks.Content);
                    }
                    catch
                    {
                        server_reply_ok = false;
                    }
                    if (server_reply_ok == true)
                    {
                        if (server_reply.status == true)
                        {
                            if (server_reply.NotecardTitle.Length > 3)
                            {
                                return bot.SendNotecard(server_reply.NotecardTitle, server_reply.NotecardContent, (UUID)server_reply.AvatarUUID);
                            }
                            else
                            {
                                Failed("Notecard title is to short");
                            }
                        }
                        return Failed(server_reply.message);
                    }
                    else
                    {
                        return Failed("FetchNextNotecard - HTTP error: " + endpoint_checks.StatusCode.ToString() + " " + endpoint_checks.Content + "");
                    }
                }
                else
                {
                    return Failed("FetchNextNotecard - HTTP error DataObject broken Status:"+ endpoint_checks.StatusCode.ToString()+" @ "+ attempt_endpoint+"");
                }
            }
            return false;
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
