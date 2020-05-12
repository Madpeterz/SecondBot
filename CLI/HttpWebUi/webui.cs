using BetterSecondBot.HttpServer;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace BetterSecondBot.HttpWebUi
{
    public class webui
    {
        JsonConfig siteconfig;
        SecondBot bot;
        Dictionary<string, long> vaildlogincookies = new Dictionary<string, long>();
        public webui(JsonConfig setconfig,SecondBot setbot)
        {
            bot = setbot;
            siteconfig = setconfig;
        }
        protected KeyValuePair<bool, bool> logincookievaild(HttpListenerRequest reqs, HttpListenerResponse resp)
        {
            return logincookievaild(reqs, resp, false);
        }
        protected KeyValuePair<bool,bool> logincookievaild(HttpListenerRequest reqs, HttpListenerResponse resp,bool force_logout)
        {
            foreach (Cookie C in resp.Cookies)
            {
                if (C.Name == "logincookie")
                {
                    Cookie old = C;
                    if (force_logout == true)
                    {
                        if (vaildlogincookies.ContainsKey(C.Value) == true)
                        {
                            vaildlogincookies.Remove(C.Value);
                        }
                        Console.WriteLine("Cookie wiped");
                    }
                    else
                    {
                        if (vaildlogincookies.ContainsKey(C.Value) == true)
                        {
                            if (vaildlogincookies[C.Value] > helpers.UnixTimeNow())
                            {
                                Console.WriteLine("Cookie ok");
                                return new KeyValuePair<bool, bool>(true, true);
                            }
                            else
                            {
                                Console.WriteLine("Cookie expired");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Cookie not vaild");
                        }
                    }
                    resp.Cookies.Remove(C);
                    C.Expired = true;
                    resp.Cookies.Add(C);
                    return new KeyValuePair<bool, bool>(true, false);
                }
            }
            Console.WriteLine("No cookie");
            return new KeyValuePair<bool, bool>(false, false);
        }
        public bool Post_process(json_http_reply reply,HttpListenerRequest reqs, HttpListenerResponse resp,List<string> args, Dictionary<string,string> post_args)
        {
            bool processed = false;
            if (args != null)
            {
                if(args.Count >= 1)
                {
                    if(args[0] == "ajax")
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
                        KeyValuePair<bool, bool> logincheck = logincookievaild(reqs, resp);

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
                                        Cookie logincookie = new Cookie("logincookie", newcookiecode);
                                        logincookie.Expires = DateTime.Now.AddDays(1);
                                        logincookie.Path = "/";
                                        resp.Cookies.Add(logincookie);
                                        Console.WriteLine("Cookie created: "+ logincookie.Value+" <=> "+ newcookiecode+"");
                                        reply.message = "ok";
                                        reply.redirect = "/";
                                    }
                                    else
                                    {
                                        reply.message = "fail";
                                    }
                                }
                                else
                                {
                                    reply.message = "Missing code";
                                }
                            }
                            else
                            {
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
                                    logincookievaild(reqs, resp, true);
                                    reply.message = "logged-out";
                                }
                                reply.message = "No action";
                            }
                            else
                            {
                                // login expired
                                reply.message = "login-expired";
                                reply.redirect = "/";
                                logincookievaild(reqs, resp,true);
                            }
                        }
                    }
                }
            }
            return !processed;
        }

        public KeyValuePair<string, byte[]> html_logged_out()
        {
            string layout = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "full.layout");
            layout = layout.Replace("[[page_content]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "login.block"));
            layout = layout.Replace("[[url_base]]", siteconfig.Http_PublicUrl);
            layout = layout.Replace("[[html_title_after]]", "Secondbot web UI");
            layout = layout.Replace("[[html_title]]", siteconfig.Basic_BotUserName);
            layout = layout.Replace("[[html_cs_top]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.css.layout"));
            layout = layout.Replace("[[html_js_bottom]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.js.layout"));

            layout = layout.Replace("[[html_js_onready]]", "");
            return new KeyValuePair<string, byte[]>("text/html", Encoding.UTF8.GetBytes(layout));
        }
        public KeyValuePair<string,byte[]> Get_Process(HttpListenerRequest reqs, HttpListenerResponse resp, List<string> args)
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
            KeyValuePair<bool, bool> logincheck = logincookievaild(reqs, resp);
            if(logincheck.Key == true)
            {
                if(logincheck.Value == false)
                {
                    logincookievaild(reqs, resp, true);
                }
            }
            if (logincheck.Value == false)
            {
                return html_logged_out();
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
                    logincookievaild(reqs, resp, true);
                    return html_logged_out();
                }
                else
                {
                    // logged in
                    return new KeyValuePair<string, byte[]>("text/html", Encoding.UTF8.GetBytes("logged in"));
                }
            }
        }
    }

}
