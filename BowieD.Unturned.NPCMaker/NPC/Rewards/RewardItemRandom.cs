﻿using BowieD.Unturned.NPCMaker.Common;
using BowieD.Unturned.NPCMaker.GameIntegration;
using BowieD.Unturned.NPCMaker.Localization;
using BowieD.Unturned.NPCMaker.NPC.Shared.Attributes;
using System.Xml;

namespace BowieD.Unturned.NPCMaker.NPC.Rewards
{
    [System.Serializable]
    public sealed class RewardItemRandom : Reward
    {
        public override RewardType Type => RewardType.Item_Random;
        public override string UIText => $"{LocalizationManager.Current.Reward["Type_Item_Random"]} [{ID}] x{Amount}";
        [AssetPicker(typeof(GameSpawnAsset), "Control_SelectAsset_Spawn", MahApps.Metro.IconPacks.PackIconMaterialKind.Dice6)]
        public ushort ID { get; set; }
        [Range((byte)1, byte.MaxValue)]
        public byte Amount { get; set; }
        public bool Auto_Equip { get; set; }

        public override void Give(Simulation simulation)
        {
            ushort item = resolve(ID);
            if (item > 0)
            {
                simulation.Items.Add(new Simulation.Item()
                {
                    ID = item,
                    Amount = 1,
                    Quality = 100
                });
            }
            // MessageBox.Show("This action requires app to load all in-game assets, which i don't want to.");
        }
        ushort resolve(ushort id)
        {
            if (GameAssetManager.TryGetAsset<GameSpawnAsset>(ID, out var asset))
            {
                asset.resolve(out id, out bool isSpawn);
                if (isSpawn)
                    id = resolve(id);
                return id;
            }
            else
            {
                return 0;
            }
        }
        public override string FormatReward(Simulation simulation)
        {
            string text = Localization;

            if (string.IsNullOrEmpty(text))
            {
                text = LocalizationManager.Current.Simulation["Quest"].Translate("Default_Reward_Item_Random");
            }
            return string.Format(text, Amount);
        }

        public override void Load(XmlNode node, int version)
        {
            base.Load(node, version);

            ID = node["ID"].ToUInt16();
            Amount = node["Amount"].ToByte();

            if (version >= 11)
            {
                Auto_Equip = node["AutoEquip"].ToBoolean();
            }
            else
            {
                Auto_Equip = false;
            }
        }

        public override void Save(XmlDocument document, XmlNode node)
        {
            base.Save(document, node);

            document.CreateNodeC("ID", node).WriteUInt16(ID);
            document.CreateNodeC("Amount", node).WriteByte(Amount);
            document.CreateNodeC("AutoEquip", node).WriteBoolean(Auto_Equip);
        }
    }
}
