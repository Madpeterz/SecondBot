using OpenMetaverse;
using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    [ClassInfo("Money makes the world go round")]
    public class Funds(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Requests the current balance and requests the balance to update.")]
        [ReturnHints("Current fund level")]
        [ReturnHintsFailure("Funds commands are disabled")]
        public object Balance()
        {
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Funds commands are disabled");
            }
            GetClient().Self.RequestBalance();
            return BasicReply(GetClient().Self.Balance.ToString());
        }

        [About("Makes the bot pay a avatar")]
        [ReturnHints("Accepted")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Amount out of range")]
        [ReturnHintsFailure("Invaild amount")]
        [ReturnHintsFailure("Transfer funds to avatars disabled")]
        [ArgHints("avatar", "Who to pay", "AVATAR")]
        [ArgHints("amount", "the amount to pay (from 1 to current balance)", "Number", "442")]
        public object PayAvatar(string avatar, string amount)
        {
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Transfer funds to avatars disabled", [avatar, amount]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return BasicReply("avatar lookup", [avatar, amount]);
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount", [avatar, amount]);
            }
            if ((amountvalue < 0) || (amountvalue > GetClient().Self.Balance))
            {
                GetClient().Self.RequestBalance();
                return Failure("Amount out of range current max: "+ GetClient().Self.Balance.ToString(), [avatar, amount]);
            }
            GetClient().Self.GiveAvatarMoney(avataruuid, amountvalue);
            return BasicReply("Accepted", [avatar, amount]);
        }

        [About("Makes the bot pay a object")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Primname is empty")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ReturnHintsFailure("Invaild amount")]
        [ReturnHintsFailure("Amount out of range")]
        [ReturnHintsFailure("Funds commands are disabled")]
        [ArgHints("object", "Object to pay","UUID")]
        [ArgHints("primname", "The name of the prim on the object to pay","Text","MyBank")]
        [ArgHints("amount", "the amount to pay (from 1 to current balance)","Number","312")]
        public object PayObject(string objectuuid,string primname,string amount)
        {
            if (master.CommandsService.myConfig.GetAllowFundsCommands() == false)
            {
                return Failure("Funds commands are disabled", [objectuuid, primname, amount]);
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object UUID", [objectuuid, primname, amount]);
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount", [objectuuid, primname, amount]);
            }
            if((amountvalue < 0) || (amountvalue > GetClient().Self.Balance))
            {
                return Failure("Amount out of range", [objectuuid, primname, amount]);
            }
            if(SecondbotHelpers.notempty(primname) == false)
            {
                return Failure("Primname is empty", [objectuuid, primname, amount]);
            }
            GetClient().Self.GiveObjectMoney(objectUUID, amountvalue, primname);
            return BasicReply("ok", [objectuuid, primname, amount]);
        }
    }
}
