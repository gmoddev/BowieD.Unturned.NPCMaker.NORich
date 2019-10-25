﻿using BowieD.Unturned.NPCMaker.Localization;
using System;

namespace BowieD.Unturned.NPCMaker.NPC.Rewards
{
    public sealed class RewardReputation : Reward
    {
        public override RewardType Type => RewardType.Reputation;
        public override string UIText
        {
            get
            {
                return $"{LocalizationManager.Current.Reward["Type_Reputation"]} x{Value}";
            }
        }
        public Int32 Value { get; set; }
    }
}
