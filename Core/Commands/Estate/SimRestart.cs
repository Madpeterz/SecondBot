namespace BSB.Commands.Estate
{
    public class SimRestart : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "True|False","Number" }; } }
        public override string[] ArgHints { get { return new[] { "Restart mode: True=Restart sim,False=Cancel restart","delay in secs to restart (Min 30 - max 240) defaults to 60 if not given" }; } }
        public override string Helpfile { get { return "Sends the message [ARG 1] to the current sim"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetClient.Network.CurrentSim.IsEstateManager == true)
                {
                    
                    if(bool.TryParse(args[0],out bool mode) == true)
                    {
                        if(mode == false)
                        {
                            bot.GetClient.Estate.CancelRestart();
                        }
                        else
                        {
                            int delay_restart = 60;
                            if(args.Length == 2)
                            {
                                int.TryParse(args[1], out delay_restart);
                            }
                            bot.GetClient.Estate.RestartRegion(delay_restart);
                        }
                    }
                    else
                    {
                        return Failed("True or False request for arg 1");
                    }
                }
                else
                {
                    return Failed("Not an estate manager here");
                }
            }
            return false;
        }
    }
}
