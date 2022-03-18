using EmbedIO;
using EmbedIO.Routing;
using OpenMetaverse;
using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    public class Funds : CommandsAPI
    {
        public Funds(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Requests the current balance and requests the balance to update.")]
        [ReturnHints("Current fund level")]
        [ReturnHintsFailure("Funds commands are disabled")]
        [Route(HttpVerbs.Get, "/Balance/{token}")]
        public object Balance(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Funds commands are disabled", "Balance");
            }
            getClient().Self.RequestBalance();
            return BasicReply(getClient().Self.Balance.ToString(), "Balance");
        }

        [About("Makes the bot pay a avatar")]
        [ReturnHints("Accepted")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Amount out of range")]
        [ReturnHintsFailure("Invaild amount")]
        [ReturnHintsFailure("Transfer funds to avatars disabled")]
        [ArgHints("avatar", "URLARG", "the avatars UUID or Firstname Lastname")]
        [ArgHints("amount", "URLARG", "the amount to pay (from 1 to current balance)")]
        [Route(HttpVerbs.Get, "/PayAvatar/{avatar}/{amount}/{token}")]
        public object PayAvatar(string avatar, string amount, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Transfer funds to avatars disabled", "PayAvatar", new [] { avatar, amount });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return BasicReply("avatar lookup", "PayAvatar", new [] { avatar, amount });
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount", "PayAvatar", new [] { avatar, amount });
            }
            if ((amountvalue < 0) || (amountvalue > getClient().Self.Balance))
            {
                return Failure("Amount out of range", "PayAvatar", new [] { avatar, amount });
            }
            getClient().Self.GiveAvatarMoney(avataruuid, amountvalue);
            return BasicReply("Accepted", "PayAvatar", new [] { avatar, amount });
        }

        [About("Makes the bot pay a object")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Primname is empty")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ReturnHintsFailure("Invaild amount")]
        [ReturnHintsFailure("Amount out of range")]
        [ReturnHintsFailure("Funds commands are disabled")]
        [ArgHints("object", "URLARG", "UUID of the object to pay")]
        [ArgHints("primname", "URLARG", "The name of the prim on the object to pay")]
        [ArgHints("amount", "URLARG", "the amount to pay (from 1 to current balance)")]
        [Route(HttpVerbs.Get, "/PayObject/{object}/{primname}/{amount}/{token}")]
        public object PayObject(string objectuuid,string primname,string amount,string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Funds commands are disabled", "PayObject", new [] { objectuuid, primname, amount });
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object UUID", "PayObject", new [] { objectuuid, primname, amount });
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount", "PayObject", new [] { objectuuid, primname, amount });
            }
            if((amountvalue < 0) || (amountvalue > getClient().Self.Balance))
            {
                return Failure("Amount out of range", "PayObject", new [] { objectuuid, primname, amount });
            }
            if(SecondbotHelpers.notempty(primname) == false)
            {
                return Failure("Primname is empty", "PayObject", new [] { objectuuid, primname, amount });
            }
            getClient().Self.GiveObjectMoney(objectUUID, amountvalue, primname);
            return BasicReply("ok", "PayObject", new [] { objectuuid, primname, amount });
        }
    }
}
