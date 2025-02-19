﻿using BowieD.Unturned.NPCMaker.Common;
using BowieD.Unturned.NPCMaker.Localization;
using System.Text;

namespace BowieD.Unturned.NPCMaker.NPC.Conditions
{
    [System.Serializable]
    public sealed class ConditionPlayerLifeVirus : Condition
    {
        public override Condition_Type Type => Condition_Type.Player_Life_Virus;
        public int Value { get; set; }
        public Logic_Type Logic { get; set; }
        public override string UIText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(LocalizationManager.Current.Condition["Type_Player_Life_Virus"] + " ");
                switch (Logic)
                {
                    case Logic_Type.Equal:
                        sb.Append("= ");
                        break;
                    case Logic_Type.Greater_Than:
                        sb.Append("> ");
                        break;
                    case Logic_Type.Greater_Than_Or_Equal_To:
                        sb.Append(">= ");
                        break;
                    case Logic_Type.Less_Than:
                        sb.Append("< ");
                        break;
                    case Logic_Type.Less_Than_Or_Equal_To:
                        sb.Append("<= ");
                        break;
                    case Logic_Type.Not_Equal:
                        sb.Append("!= ");
                        break;
                }
                sb.Append(Value);
                return sb.ToString();
            }
        }

        public override void Apply(Simulation simulation) { }
        public override bool Check(Simulation simulation)
        {
            return SimulationTool.Compare(simulation.Virus, Value, Logic);
        }
        public override string FormatCondition(Simulation simulation)
        {
            if (string.IsNullOrEmpty(Localization))
            {
                return null;
            }

            return string.Format(Localization, simulation.Virus, Value);
        }

        public override void Load(System.Xml.XmlNode node, int version)
        {
            base.Load(node, version);

            Value = node["Value"].ToInt32();
            Logic = node["Logic"].ToEnum<Logic_Type>();
        }

        public override void Save(System.Xml.XmlDocument document, System.Xml.XmlNode node)
        {
            base.Save(document, node);

            document.CreateNodeC("Value", node).WriteInt32(Value);
            document.CreateNodeC("Logic", node).WriteEnum(Logic);
        }
    }
}
