using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterSecondBotShared.Static;
using OpenMetaverse;
using RestSharp;

namespace BSB.Commands.script
{
    public class ScriptHttp : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "TEXT", "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "File endpoint", "Avatar Name/UUID to recive script" }; } }
        public override string Helpfile { get { return "[StreamAdmin]\nThis command is part of the streamadmin commands set\n No end user support will be given."; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                string attempt_endpoint = args[0];

                if(attempt_endpoint.Contains("https://github.com/"))
                {
                    attempt_endpoint = attempt_endpoint.Replace("https://github.com/", "https://raw.githubusercontent.com/");
                }

                var client = new RestClient(attempt_endpoint);
                var request = new RestRequest("", Method.GET);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                IRestResponse endpoint_checks = client.Get(request);
                if (endpoint_checks.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        if (endpoint_checks.IsSuccessful == true)
                        {
                            if (endpoint_checks.Content.Length > 3)
                            {
                                if (UUID.TryParse(args[1], out UUID Avatar))
                                {
                                    return bot.SendScript(attempt_endpoint.Split('/').LastOrDefault().Replace("%20", " "), endpoint_checks.Content, Avatar);
                                }
                                else
                                {
                                    Failed("Avatar UUID not valid");
                                }
                            }
                            else
                            {
                                Failed("script title is to short");
                            }
                        }
                        return Failed(endpoint_checks.ErrorMessage);
                    }
                    catch (Exception e)
                    {
                        return Failed("Error: " + e.Message + "");
                    }
                }
                else
                {
                    return Failed("HTTP error DataObject broken Status:" + endpoint_checks.StatusCode.ToString() + " @ " + attempt_endpoint + "");
                }
            }
            return false;
        }
    }

    public class scriptendpoint : endpoint
    {
        public string AvatarUUID { get; set; }
        public string scriptTitle { get; set; }
        public string scriptContent { get; set; }
    }

    public class endpoint
    {
        public bool status { get; set; }
        public string message { get; set; }
    }
}
