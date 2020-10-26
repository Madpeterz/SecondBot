using BetterSecondBotShared.bottypes;
using BetterSecondBotShared.Static;
using OpenMetaverse;
using System.Collections.Generic;

namespace BSB.bottypes
{
	public abstract class ActionsAutoAcceptBot : BasicBot
	{
		Dictionary<UUID, List<string>> accept_actions_storage = new Dictionary<UUID, List<string>>();
		Dictionary<UUID, List<string>> sticky_accept_actions_storage = new Dictionary<UUID, List<string>>();

		public bool Accept_action_from(string action, UUID avatar)
		{
			if(accept_actions_storage.ContainsKey(avatar) == true)
            {
				if (accept_actions_storage[avatar].Contains(action) == true)
				{
					return true;
				}
            }
			if(sticky_accept_actions_storage.ContainsKey(avatar) == true)
            {
				return sticky_accept_actions_storage[avatar].Contains(action);
			}
			return false;
		}
		public void Remove_action_from(string action, UUID avatar)
        {
			Remove_action_from(action, avatar, false);
		}

		public void Remove_action_from(string action, UUID avatar, bool remove_sticky)
        {
			if (accept_actions_storage.ContainsKey(avatar) == true)
            {
				if(accept_actions_storage[avatar].Contains(action) == true)
                {
					accept_actions_storage[avatar].Remove(action);
				}
				if(accept_actions_storage[avatar].Count == 0)
                {
					accept_actions_storage.Remove(avatar);
				}
			}
			if(remove_sticky == true)
            {
				if (sticky_accept_actions_storage.ContainsKey(avatar) == true)
				{
					if (sticky_accept_actions_storage[avatar].Contains(action) == true)
					{
						sticky_accept_actions_storage[avatar].Remove(action);
					}
					if (sticky_accept_actions_storage[avatar].Count == 0)
					{
						sticky_accept_actions_storage.Remove(avatar);
					}
				}
			}
		}
		public void Add_action_from(string action, UUID avatar)
        {
			Add_action_from(action, avatar, false);
		}
		public void Add_action_from(string action, UUID avatar, bool sticky)
		{
			if(sticky == true)
            {
				if (sticky_accept_actions_storage.ContainsKey(avatar) == false)
				{
					sticky_accept_actions_storage.Add(avatar, new List<string>());
                }
				if (sticky_accept_actions_storage[avatar].Contains(action) == false)
				{
					sticky_accept_actions_storage[avatar].Add(action);
				}
			}
			else
            {
				if (accept_actions_storage.ContainsKey(avatar) == false)
				{
					accept_actions_storage.Add(avatar, new List<string>());
				}
				if (accept_actions_storage[avatar].Contains(action) == false)
				{
					accept_actions_storage[avatar].Add(action);
				}
			}
		}
	}
}
