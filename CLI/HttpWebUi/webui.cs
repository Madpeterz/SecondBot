using BetterSecondBot.HttpServer;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BSB.bottypes;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BetterSecondBot.HttpWebUi
{
    public class Webui
    {
        private JsonConfig siteconfig;
        private SecondBot bot;
        private Dictionary<string, long> vaildlogincookies = new Dictionary<string, long>();
        public Webui(JsonConfig setconfig, SecondBot setbot)
        {
            bot = setbot;
            siteconfig = setconfig;
        }
        protected KeyValuePair<bool, bool> Logincookievaild(HttpListenerResponse resp)
        {
            return Logincookievaild(resp, false);
        }
        protected KeyValuePair<bool, bool> Logincookievaild(HttpListenerResponse resp, bool force_logout)
        {
            foreach (Cookie C in resp.Cookies)
            {
                if (C.Name == "logincookie")
                {
                    if (force_logout == true)
                    {
                        if (vaildlogincookies.ContainsKey(C.Value) == true)
                        {
                            vaildlogincookies.Remove(C.Value);
                        }
                    }
                    else
                    {
                        if (vaildlogincookies.ContainsKey(C.Value) == true)
                        {
                            if (vaildlogincookies[C.Value] > helpers.UnixTimeNow())
                            {
                                return new KeyValuePair<bool, bool>(true, true);
                            }
                        }
                    }
                    resp.Cookies.Remove(C);
                    C.Expired = true;
                    resp.Cookies.Add(C);
                    return new KeyValuePair<bool, bool>(true, false);
                }
            }
            return new KeyValuePair<bool, bool>(false, false);
        }
        public bool Post_process(Json_http_reply reply, HttpListenerResponse resp, List<string> args, Dictionary<string, string> post_args)
        {
            bool processed = false;
            if (args != null)
            {
                if (args.Count >= 1)
                {
                    if (args[0] == "ajax")
                    {
                        processed = true;
                        // webui only cares about ajax post requests
                        string mod = "none";
                        string area = "none";
                        if (args.Count >= 2)
                        {
                            mod = args[1];
                        }
                        if (args.Count >= 3)
                        {
                            area = args[2];
                        }
                        KeyValuePair<bool, bool> logincheck = Logincookievaild(resp);

                        if (logincheck.Key == false)
                        {
                            if (mod == "login")
                            {
                                if (post_args.ContainsKey("logincode") == true)
                                {
                                    if (post_args["logincode"] == siteconfig.Security_WebUIKey)
                                    {
                                        reply.status = true;
                                        string newcookiecode = "";
                                        while ((newcookiecode == null) || (newcookiecode == "") || (vaildlogincookies.ContainsKey(newcookiecode) == true))
                                        {
                                            newcookiecode = helpers.GetSHA1(siteconfig.Security_WebUIKey + helpers.UnixTimeNow().ToString() + new Random().Next(12345).ToString());
                                        }
                                        vaildlogincookies.Add(newcookiecode, helpers.UnixTimeNow() + (5 * 60)); // auto logout after 5 mins
                                        Cookie logincookie = new Cookie("logincookie", newcookiecode)
                                        {
                                            Expires = DateTime.Now.AddDays(1),
                                            Path = "/"
                                        };
                                        resp.Cookies.Add(logincookie);
                                        Console.WriteLine("Cookie created: " + logincookie.Value + " <=> " + newcookiecode + "");
                                        reply.message = "ok";
                                        reply.redirect = "/";
                                    }
                                    else
                                    {
                                        reply.status = false;
                                        reply.message = "fail";
                                    }
                                }
                                else
                                {
                                    reply.status = false;
                                    reply.message = "Missing code";
                                }
                            }
                            else
                            {
                                reply.status = false;
                                reply.message = "No access";
                            }
                        }
                        else
                        {
                            if (logincheck.Value == true)
                            {
                                reply.status = true;
                                // logged in
                                if (mod == "logout")
                                {
                                    Logincookievaild(resp, true);
                                    reply.message = "logged-out";
                                    reply.redirect = "/";
                                    reply.status = false;
                                }
                                else if(mod == "interface")
                                {
                                    if (area == "get")
                                    {
                                        reply.message = JsonConvert.SerializeObject(bot.GetLastCommands(30));
                                    }
                                    else if(area == "send")
                                    {
                                        if (post_args.ContainsKey("message") == true)
                                        {
                                            string[] bits = post_args["message"].Split("|||");
                                            if(bits.Length == 2)
                                            {
                                                reply.status = bot.GetCommandsInterface.Call(bits[0], bits[1]);
                                                if(reply.status == true)
                                                {
                                                    reply.message = "ok";
                                                }
                                                else
                                                {
                                                    reply.message = "Failed";
                                                }
                                            }
                                            else
                                            {
                                                reply.status = bot.GetCommandsInterface.Call(post_args["message"]);
                                                if (reply.status == true)
                                                {
                                                    reply.message = "ok";
                                                }
                                                else
                                                {
                                                    reply.message = "Failed";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            reply.message = "No message";
                                            reply.status = false;
                                        }
                                    }
                                    else
                                    {
                                        reply.message = "Unknown area";
                                        reply.status = false;
                                    }
                                }
                                else if (mod == "localchat")
                                {
                                    if (area == "get")
                                    {
                                        reply.message = JsonConvert.SerializeObject(bot.getLocalChatHistory());
                                    }
                                    else if (area == "send")
                                    {
                                        if (post_args.ContainsKey("message") == true)
                                        {
                                            bot.AddToLocalChat("{Via WebGUI}", post_args["message"]);
                                            bot.GetClient.Self.Chat("{Via WebGUI} "+ post_args["message"], 0,ChatType.Normal);
                                            reply.message = "ok";
                                            reply.status = true;
                                        }
                                        else
                                        {
                                            reply.message = "No message";
                                            reply.status = false;
                                        }
                                    }
                                    else
                                    {
                                        reply.message = "Unknown area";
                                        reply.status = false;
                                    }
                                }
                                else if (mod == "groupchat")
                                {
                                    if (area == "list")
                                    {
                                        Dictionary<string, string> groupname_uuid = new Dictionary<string, string>();
                                        foreach (KeyValuePair<UUID, Group> entry in bot.MyGroups)
                                        {
                                            groupname_uuid.Add(entry.Key.ToString(), entry.Value.Name);
                                        }
                                        reply.message = JsonConvert.SerializeObject(groupname_uuid);
                                    }
                                    if (area == "get")
                                    {
                                        if (post_args.ContainsKey("groupuuid") == true)
                                        {
                                            if (UUID.TryParse(post_args["groupuuid"], out UUID groupuuid) == true)
                                            {
                                                reply.message = JsonConvert.SerializeObject(bot.GetGroupchat(groupuuid));
                                            }
                                            else
                                            {
                                                reply.message = "Unable to process group uuid";
                                                reply.status = false;
                                            }
                                        }
                                        else
                                        {
                                            reply.message = "No groupuuid";
                                            reply.status = false;
                                        }
                                    }
                                    else if (area == "send")
                                    {
                                        if (post_args.ContainsKey("groupuuid") == true)
                                        {
                                            if (UUID.TryParse(post_args["groupuuid"], out UUID groupuuid) == true)
                                            {
                                                if (post_args.ContainsKey("message") == true)
                                                {
                                                    bot.GetClient.Self.RequestJoinGroupChat(groupuuid);
                                                    Thread.Sleep(100);
                                                    bot.GetClient.Self.InstantMessageGroup(groupuuid, "{Via WebGUI} " + post_args["message"]);
                                                    bot.AddToGroupchat(groupuuid, "{Via WebGUI}", post_args["message"]);
                                                    reply.message = "ok";
                                                    reply.status = true;
                                                }
                                                else
                                                {
                                                    reply.message = "No message";
                                                    reply.status = false;
                                                }
                                            }
                                            else
                                            {
                                                reply.message = "Unable to process group uuid";
                                                reply.status = false;
                                            }
                                        }
                                        else
                                        {
                                            reply.message = "No groupuuid";
                                            reply.status = false;
                                        }
                                    }
                                    else
                                    {
                                        reply.message = "Unknown area";
                                        reply.status = false;
                                    }
                                }
                                else if (mod == "im")
                                {
                                    if (area == "list")
                                    {
                                        reply.message = JsonConvert.SerializeObject(bot.GetIMChatWindowKeyNames());
                                    }
                                    else if (area == "send")
                                    {
                                        if (post_args.ContainsKey("avataruuid") == true)
                                        {
                                            if (UUID.TryParse(post_args["avataruuid"], out UUID avataruuid) == true)
                                            {
                                                if (post_args.ContainsKey("message") == true)
                                                {
                                                    bot.GetCommandsInterface.Call("im", "" + avataruuid.ToString() + "~#~{Via WebGUI} " + post_args["message"]);
                                                    reply.message = "ok";
                                                    reply.status = true;
                                                }
                                                else
                                                {
                                                    reply.message = "No message";

                                                }
                                            }
                                            else
                                            {
                                                reply.message = "Unable to process avatar uuid";
                                                reply.status = false;
                                            }
                                        }
                                        else
                                        {
                                            reply.message = "No avataruuid";
                                            reply.status = false;
                                        }
                                    }
                                    else if (area == "get")
                                    {
                                        if (post_args.ContainsKey("avataruuid") == true)
                                        {
                                            if (UUID.TryParse(post_args["avataruuid"], out UUID avataruuid) == true)
                                            {
                                                reply.message = JsonConvert.SerializeObject(bot.GetIMChatWindow(avataruuid));
                                            }
                                            else
                                            {
                                                reply.message = "Unable to process avatar uuid";
                                                reply.status = false;
                                            }
                                        }
                                        else
                                        {
                                            reply.message = "No avataruuid";
                                            reply.status = false;
                                        }
                                    }
                                    else
                                    {
                                        reply.message = "Unknown area";
                                        reply.status = false;
                                    }
                                }
                                else
                                {
                                    reply.message = "No action";
                                    reply.status = false;
                                }
                            }
                            else
                            {
                                // login expired
                                reply.message = "login-expired";
                                reply.redirect = "/";
                                Logincookievaild(resp, true);
                            }
                        }
                    }
                }
            }
            return !processed;
        }
        public string Html_logged_in(string mod, string area)
        {
            string layout = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "sidemenu.layout");
            Dictionary<string, string> swaps = new Dictionary<string, string>();

            
            string pagetitle = "Dashboard";
            string pagecontent = "";
            string pageactions = "";
            string js_onready = "";
            string[] menu_text = new string[] { "Localchat", "Groupchat", "IMs", "Interface" };
            string[] menu_links = new string[] { "localchat", "groupchat", "im", "interface" };
            string[] meni_icons = new string[] { "fas fa-comment", "fas fa-comments", "fas fa-users","fas fa-robot" };
            int loop = 0;
            string menu = "";
            while (loop < menu_text.Count())
            {
                string active = "";
                if (menu_links[loop] == mod) active = "active";
                menu = "" + menu + " <li class=\"nav-item\"><a href=\"[[url_base]]"+menu_links[loop]+"\" class=\"nav-link " + active+ "\"><i class=\"" + meni_icons[loop] + " text-success\"></i> " + menu_text[loop] + "</a></li>";
                loop++;
            }
            swaps.Add("html_menu", menu);
            if (mod == "none")
            {
                pagecontent = "<p>Please select a menu item on the left</p>";
            }
            else if(mod == "localchat")
            {
                pagetitle = "Localchat";
                pagecontent = "<textarea readonly cols=\"82\" rows=\"14\" id=\"localchat\" name=\"localchat\"> - Loading local chat please wait -</textarea><br/>" +
                    "<form  method=\"post\" class=\"ajaxq\" action=\"[[url_base]]ajax/localchat/send\">" +
                    "<input type=\"text\" name=\"message\" id=\"message\" value=\"\" size=\"33\" class=\"form-control\" placeholder=\"Say in localchat\">" +
                    "<button type\"submit\" class=\"btn btn-primary\">Send</button>"+
                    "</form>";
                js_onready = "setInterval(function(){ update_localchat(\"" + siteconfig.Http_PublicUrl+"\"); },400);";

            }
            else if (mod == "groupchat")
            {
                pagetitle = "Group chat";
                pagecontent = "";
                if (area == "none")
                {
                    
                }

                
               
            }
            swaps.Add("html_title", pagetitle);
            swaps.Add("page_actions", pageactions);
            swaps.Add("page_content", pagecontent);
            swaps.Add("page_title", pagetitle);
            swaps.Add("html_cs_top", "");
            swaps.Add("html_js_bottom", "");
            swaps.Add("html_js_onready", js_onready);
            foreach (KeyValuePair<string,string> A in swaps)
            {
                layout = layout.Replace("[[" + A.Key + "]]", A.Value);
            }
            return layout;
        }
        public string Html_logged_out()
        {
            string layout = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "full.layout");
            layout = layout.Replace("[[page_content]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "login.block"));
            layout = layout.Replace("[[html_js_onready]]", "");
            layout = layout.Replace("[[html_cs_top]]", "");
            layout = layout.Replace("[[html_js_bottom]]", "");
            layout = layout.Replace("[[html_title]]", siteconfig.Basic_BotUserName);
            return layout;
        }
        public KeyValuePair<string, byte[]> Get_Process(HttpListenerResponse resp, List<string> args)
        {

            // get content
            if (args != null)
            {
                if (args.Count >= 1)
                {
                    if (args[0] == "theme")
                    {
                        if (args.Count >= 2)
                        {
                            if (args[1] == "images")
                            {
                                return new KeyValuePair<string, byte[]>("image/png", helpers.ReadResourceFileBinary(Assembly.GetExecutingAssembly(), args[2]));
                            }
                            else if (args[1] == "css")
                            {
                                return new KeyValuePair<string, byte[]>("text/css", Encoding.UTF8.GetBytes(helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), args[2])));
                            }
                            else if (args[1] == "js")
                            {
                                return new KeyValuePair<string, byte[]>("text/javascript", Encoding.UTF8.GetBytes(helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), args[2])));
                            }
                        }
                    }
                }
            }
            // get display
            KeyValuePair<bool, bool> logincheck = Logincookievaild(resp);
            if (logincheck.Key == true)
            {
                if (logincheck.Value == false)
                {
                    Logincookievaild(resp, true);
                }
            }
            string reply_with;
            if (logincheck.Value == false)
            {
                reply_with = Html_logged_out();
            }
            else
            {
                string mod = "none";
                string area = "none";
                if (args.Count >= 1)
                {
                    mod = args[0];
                }
                if (args.Count >= 2)
                {
                    area = args[1];
                }

                if (mod == "logout")
                {
                    Logincookievaild(resp, true);
                    reply_with = Html_logged_out();
                }
                else
                {
                    // logged in
                    reply_with = Html_logged_in(mod, area);
                }
            }
            reply_with = reply_with.Replace("[[url_base]]", siteconfig.Http_PublicUrl);
            reply_with = reply_with.Replace("[[html_title_after]]", "Secondbot web UI");
            reply_with = reply_with.Replace("[[html_cs_top_cdn]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.css.layout"));
            reply_with = reply_with.Replace("[[html_js_bottom_cdn]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.js.layout"));
            return new KeyValuePair<string, byte[]>("text/html", Encoding.UTF8.GetBytes(reply_with));
        }
    }
}
