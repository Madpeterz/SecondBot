using System.Text.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Talk back to the rabbit MQ")]
    public class RabbitMQ(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Send a message to the rabbit server the bot is connected to")]
        [ArgHints("Qname", "Name of the queue to send the message to","TEXT","exampleq")]
        [ArgHints("message", "Message to send to the queue","TEXT","Hello world")]
        [ReturnHints("Message sent to the queue")]
        [ReturnHintsFailure("Error sending message: ...")]
        [ReturnHintsFailure("RabbitMQ service not available")]
        [ReturnHintsFailure("RabbitMQ service not running")]
        [ReturnHintsFailure("Qname is empty")]
        [ReturnHintsFailure("Message is empty")]    
        [CmdTypeDo()]
        public object SendMessageToQ(string Qname,string message)
        {
            if(master.RabbitService == null)
            {
                return Failure("RabbitMQ service not available");
            }
            if(master.RabbitService.isRunning() == false)
            {
                return Failure("RabbitMQ service not running");
            }
            if((Qname == null) || (Qname.Length < 1))
            {
                return Failure("Qname is empty");
            }
            if ((message == null) || (message.Length < 1))
            {
                return Failure("Message is empty");
            }
            try
            {
                KeyValuePair<bool,string> reply = master.RabbitService.SendMessage(Qname, message);
                if(reply.Key == false)
                {
                    return Failure("Error sending message: " + reply.Value);
                }
                return BasicReply("Message sent successfully to " + Qname);
            }
            catch (Exception ex)
            {
                return Failure("Error sending message: " + ex.Message);
            }
        }

        
    }
}
