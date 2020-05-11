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
        public webui(JsonConfig setconfig,SecondBot setbot)
        {
            bot = setbot;
            siteconfig = setconfig;
        }
        public KeyValuePair<bool, string> Post_process(List<string> args, Dictionary<string,string> post_args)
        {
            if(args != null)
            {
                if(args.Count == 1)
                {
                    if(args[0] == "ajax")
                    {
                        // webui only cares about ajax post requests

                    }
                }
            }
            return new KeyValuePair<bool, string>(false, "");
        }
        public KeyValuePair<string,byte[]> Get_Process(List<string> args,CookieCollection cookies)
        {
            Cookie logincookie = null;
            foreach(Cookie C in cookies)
            {
                if(C.Name == "logincookie")
                {
                    logincookie = C;
                    break;
                }
            }
            if (logincookie == null)
            {
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
                        return new KeyValuePair<string, byte[]>("text/html", null);
                    }
                }
                string layout = helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "full.layout");
                layout = layout.Replace("[[page_content]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "login.block"));
                layout = layout.Replace("[[url_base]]", siteconfig.HttpPublicUrlBase);
                layout = layout.Replace("[[html_title_after]]", "Secondbot web UI");
                layout = layout.Replace("[[html_title]]", siteconfig.userName);
                layout = layout.Replace("[[html_cs_top]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.css.layout"));
                layout = layout.Replace("[[html_js_bottom]]", helpers.ReadResourceFile(Assembly.GetExecutingAssembly(), "cdn.js.layout"));
                
                layout = layout.Replace("[[html_js_onready]]", "");
                return new KeyValuePair<string, byte[]>("text/html", Encoding.UTF8.GetBytes(layout));

            }
            else
            {
                return new KeyValuePair<string, byte[]>("text/html", null);
            }
        }
    }

}
