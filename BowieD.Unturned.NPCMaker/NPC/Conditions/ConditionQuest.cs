﻿using BowieD.Unturned.NPCMaker.Common;
using BowieD.Unturned.NPCMaker.GameIntegration;
using BowieD.Unturned.NPCMaker.Localization;
using BowieD.Unturned.NPCMaker.NPC.Shared.Attributes;
using System.Linq;

namespace BowieD.Unturned.NPCMaker.NPC.Conditions
{
    [System.Serializable]
    public sealed class ConditionQuest : Condition
    {
        public override Condition_Type Type => Condition_Type.Quest;
        [AssetPicker(typeof(GameQuestAsset), "Control_SelectAsset_Quest", MahApps.Metro.IconPacks.PackIconMaterialKind.Exclamation)]
        public ushort ID { get; set; }
        public Quest_Status Status { get; set; }
        public Logic_Type Logic { get; set; }
        public bool Ignore_NPC { get; set; }
        public override string UIText
        {
            get
            {
                string outp;

                if (GameAssetManager.TryGetAsset<GameQuestAsset>(ID, out var asset))
                {
                    outp = LocalizationManager.Current.Condition["Type_Quest"] + $" [{ID}] '{asset.name}' ";
                }
                else
                {
                    outp = LocalizationManager.Current.Condition["Type_Quest"] + $" [{ID}] ";
                }

                switch (Logic)
                {
                    case Logic_Type.Equal:
                        outp += "= ";
                        break;
                    case Logic_Type.Not_Equal:
                        outp += "!= ";
                        break;
                    case Logic_Type.Greater_Than:
                        outp += "> ";
                        break;
                    case Logic_Type.Greater_Than_Or_Equal_To:
                        outp += ">= ";
                        break;
                    case Logic_Type.Less_Than:
                        outp += "< ";
                        break;
                    case Logic_Type.Less_Than_Or_Equal_To:
                        outp += "<= ";
                        break;
                }
                outp += LocalizationManager.Current.Condition[$"Quest_Status_{Status}"];
                return outp;
            }
        }

        public override void Apply(Simulation simulation)
        {
            if (Reset)
            {
                switch (simulation.GetQuestStatus(ID))
                {
                    case Quest_Status.Completed:
                        {
                            simulation.Flags.Remove(ID);
                        }
                        break;
                    case Quest_Status.Active:
                        {
                            simulation.Quests.Remove(ID);
                        }
                        break;
                    case Quest_Status.Ready:
                        {
                            simulation.Quests.Remove(ID);
                            simulation.Flags[ID] = 1;

                            NPCQuest questAsset = MainWindow.CurrentProject.data.quests.Single(d => d.ID == ID);

                            foreach (Condition c in questAsset.conditions)
                            {
                                c.Apply(simulation);
                            }

                            foreach (Rewards.Reward r in questAsset.rewards)
                            {
                                r.Give(simulation);
                            }
                        }
                        break;
                    default:
                    case Quest_Status.None:
                        break;
                }
            }
        }
        public override bool Check(Simulation simulation)
        {
            return SimulationTool.Compare(simulation.GetQuestStatus(ID), Status, Logic);
        }

        public override void Load(System.Xml.XmlNode node, int version)
        {
            base.Load(node, version);

            ID = node["ID"].ToUInt16();
            Status = node["Status"].ToEnum<Quest_Status>();
            Logic = node["Logic"].ToEnum<Logic_Type>();
            Ignore_NPC = node["Ignore_NPC"].ToBoolean();
        }

        public override void Save(System.Xml.XmlDocument document, System.Xml.XmlNode node)
        {
            base.Save(document, node);

            document.CreateNodeC("ID", node).WriteUInt16(ID);
            document.CreateNodeC("Status", node).WriteEnum(Status);
            document.CreateNodeC("Logic", node).WriteEnum(Logic);
            document.CreateNodeC("Ignore_NPC", node).WriteBoolean(Ignore_NPC);
        }
    }
}
