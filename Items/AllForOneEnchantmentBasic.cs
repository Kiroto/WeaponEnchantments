﻿using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using WeaponEnchantments.Common;
using System.Reflection;
using Terraria.GameContent.Creative;

namespace WeaponEnchantments.Items
{
	public enum EnchantmentTypeID : int
	{
		AllForOne,
		AmmoCost,
		ArmorPenetration,
		CatastrophicRelease,
		ColdSteel,
		CriticalStrikeChance,
		Damage,
		DangerSense,
		StatDefense,
		GodSlayer,
		HellsWrath,
		Hunter,
		JunglesFury,
		LifeSteal,
		Mana,
		Moonlight,
		ObsidianSkin,
		OneForAll,
		Peace,
		Scale,
		Speed,
		Spelunker,
		Splitting,
		War,
	}
	public enum UtilityEnchantmentNames
	{
		AmmoCost,
		DangerSense,
		Hunter,
		Mana,
		ObsidianSkin,
		Peace,
		Scale,
		Spelunker,
		War
	}
	public enum DamageTypeSpecificID
	{
		Default,
		Generic,
		Melee,
		MeleeNoSpeed,
		Ranged,
		Magic,
		Summon,
		SummonMeleeSpeed,
		MagicSummonHybrid,
		Throwing
	}//Located in DamageClassLoader.cs
	public class AllForOneEnchantmentBasic : ModItem
	{
		bool ToggleRarityNames = false;

		public static readonly string[] rarity = new string[5] { "Basic", "Common", "Rare", "SuperRare", "UltraRare" };
		public static readonly string[] displayRarity = new string[5] { "Basic", "Common", "Rare", "Epic", "Legendary" };
		public static readonly Color[] rarityColors = new Color[5] { Color.White, Color.Green, Color.Blue, Color.Purple, Color.Orange };

		public static readonly int defaultBuffDuration = 60;
		public int EnchantmentSize { private set; get; } = -1;
		public int EnchantmentType { private set; get; } = -1;
		public string EnchantmentTypeName { private set; get; }
		public string MyDisplayName { private set; get; }
		public float EnchantmentStrength { private set; get; }
		public bool Utility { private set; get; }
		public bool Unique { private set; get; }
		public bool Max1 { private set; get; } = false;
		public int DamageClassSpecific { private set; get; }
		public int RestrictedClass { private set; get; }
		public bool StaticStat { private set; get; }
		public string ShortToolTip { private set; get; }
		public string FullToolTip { private set; get; }
		private bool checkedStats = false;
		public List<int> Buff { private set; get; }	= new List<int>();
		public Dictionary<int, int> OnHitBuff { private set; get; } = new Dictionary<int, int>();
		public Dictionary<int, int> Debuff { private set; get; } = new Dictionary<int, int>();
		public bool Armor { private set; get; } = false;
		public int NewDamageType = -1;
		//public static string temp = "";
		public List<EnchantmentStaticStat> StaticStats { private set; get; } = new List<EnchantmentStaticStat>();
		public List<EStat> EStats { private set; get; } = new List<EStat>();
		public Dictionary<string, float> AllowedList { private set; get; } = new Dictionary<string, float>();
		public override string Texture => (GetType().Namespace + ".Sprites." + Name).Replace('.', '/');
        public override ModItem Clone(Item newEntity)
        {
			AllForOneEnchantmentBasic enchantment = (AllForOneEnchantmentBasic)base.Clone(newEntity);
			enchantment.StaticStats = new List<EnchantmentStaticStat>(StaticStats);
			enchantment.EStats = new List<EStat>(EStats);
			enchantment.OnHitBuff = new Dictionary<int, int>(OnHitBuff);
			enchantment.Debuff = new Dictionary<int, int>(Debuff);
			enchantment.Buff = new List<int>(Buff);
			enchantment.AllowedList = new Dictionary<string, float>(AllowedList);
			return enchantment;
		}
        public override void SetStaticDefaults()
        {
			GetDefaults();
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			string toolTip = "";
			switch ((EnchantmentTypeID)EnchantmentType)
			{
				case EnchantmentTypeID.AllForOne:
					toolTip = $"(item CD equal to {EnchantmentStrength * 0.8f}x use speed)";
					break;
				case EnchantmentTypeID.CatastrophicRelease:
					toolTip = "()";
					break;
				case EnchantmentTypeID.GodSlayer:
					toolTip = "(Bonus true damage based on enemy max hp)\n(Bonus damage not affected by LifeSteal)";
					break;
				case EnchantmentTypeID.LifeSteal:
					toolTip = "(remainder is saved to prevent always rounding to 0 for low damage weapons)";
					break;
				case EnchantmentTypeID.OneForAll:
					toolTip = "(Hiting an enemy will damage all nearby enemies)\n(WARNING - Destroys your projectiles upon hitting an enemy)";
					break;
				case EnchantmentTypeID.Splitting:
					toolTip = "(Chance to produce an extra projectile)";
					break;
				case EnchantmentTypeID.War:
					toolTip = "(Minion Damage is reduced by your spawn rate multiplier, from enchantments, unless they are your minion attack target)\n(minion attack target set from hitting enemies with whips or a weapon that is converted to summon damage from an enchantment)\n(Prevents consuming boss summoning items if spawn rate multiplier, from enchantments, is > 1)";
					break;
			}//ToolTips
			Tooltip.SetDefault(GenerateFullTooltip(toolTip));
			if (EnchantmentSize > 2 && ToggleRarityNames)
				DisplayName.SetDefault(UtilityMethods.AddSpaces(MyDisplayName + "Enchantment" + displayRarity[EnchantmentSize]));
			else
				DisplayName.SetDefault(UtilityMethods.AddSpaces(MyDisplayName + Name.Substring(Name.IndexOf("Enchantment"))));
			//temp += $"{Name}\n{Tooltip.GetDefault()}\n\n";
		}
		private void GetDefaults()
        {
			EnchantmentTypeName = Name.Substring(0, Name.IndexOf("Enchantment"));
			EnchantmentSize = GetEnchantmentSize(Name);
			for (int i = 0; i < Enum.GetNames(typeof(EnchantmentTypeID)).Length; i++)
			{
				if (EnchantmentTypeName == ((EnchantmentTypeID)i).ToString())
				{
					EnchantmentType = i;
					break;
				}
			}//Check EnchantmentType
			switch (EnchantmentSize)
			{
				case 3:
					Item.width = 44;
					Item.height = 40;
					break;
				case 4:
					Item.width = 40;
					Item.height = 40;
					break;
				default:
					Item.width = 28 + 4 * (EnchantmentSize);
					Item.height = 28 + 4 * (EnchantmentSize);
					break;
			}//Width/Height
			int endSize;
			switch ((EnchantmentTypeID)EnchantmentType)
			{
				case EnchantmentTypeID.Spelunker:
				case EnchantmentTypeID.DangerSense:
				case EnchantmentTypeID.Hunter:
				case EnchantmentTypeID.ObsidianSkin:
					endSize = EnchantmentSize - 2;
					break;
				default:
					endSize = EnchantmentSize;
					break;
			}//Base Value
			for (int i = 0; i < endSize; i++)
			{
				Item.value += (int)EnchantmentEssenceBasic.values[i] * (Utility ? 5 : 10);
			}//Essence Value
			switch (EnchantmentSize)
			{
				case 3:
					Item.value += Containment.Values[2];
					break;
				case 4:
					Item.value += ContentSamples.ItemsByType[999].value;
					break;
				default:
					Item.value += Containment.Values[EnchantmentSize];
					break;
			}//Value - Containment/SuperiorStaibalizers
			for (int i = 0; i < Enum.GetNames(typeof(UtilityEnchantmentNames)).Length; i++)
			{
				if (EnchantmentTypeName == ((UtilityEnchantmentNames)i).ToString())
				{
					Utility = true;
					break;
				}
			}//Check Utility
			for (int i = 0; i < ItemID.Count; i++)
			{
				if (ContentSamples.ItemsByType[i].Name == EnchantmentTypeName)
				{
					Unique = true;
					break;
				}
			}//Check Unique (Vanilla Items)
			switch ((EnchantmentTypeID)EnchantmentType)
			{
				case EnchantmentTypeID.Scale:
				case EnchantmentTypeID.War:
				case EnchantmentTypeID.OneForAll:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.1f;
							break;
						case 1:
							EnchantmentStrength = 0.2f;
							break;
						case 2:
							EnchantmentStrength = 0.5f;
							break;
						case 3:
							EnchantmentStrength = 0.8f;
							break;
						case 4:
							EnchantmentStrength = 1f;
							break;
					}// 0.1, 0.2, 0.5, 0.8, 1
					break;
				case EnchantmentTypeID.Peace:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 1f / 1.1f - 1f;
							break;
						case 1:
							EnchantmentStrength = 1f / 1.2f - 1f;
							break;
						case 2:
							EnchantmentStrength = 1f / 1.5f - 1f;
							break;
						case 3:
							EnchantmentStrength = 1f / 1.8f - 1f;
							break;
						case 4:
							EnchantmentStrength = 1f / 2f - 1f;
							break;
					}// 1f / 1.1f - 1f, 1f / 1.2f - 1f, 1f / 1.5f - 1f, 1f / 1.8f - 1f, 1f / 2f - 1f
					break;
				case EnchantmentTypeID.StatDefense:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 1f;
							break;
						case 1:
							EnchantmentStrength = 2f;
							break;
						case 2:
							EnchantmentStrength = 3f;
							break;
						case 3:
							EnchantmentStrength = 5f;
							break;
						case 4:
							EnchantmentStrength = 10f;
							break;
					}// 1, 2, 3, 5, 10
					break;
				case EnchantmentTypeID.ArmorPenetration:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 2f;
							break;
						case 1:
							EnchantmentStrength = 4f;
							break;
						case 2:
							EnchantmentStrength = 6f;
							break;
						case 3:
							EnchantmentStrength = 10f;
							break;
						case 4:
							EnchantmentStrength = 20f;
							break;
					}// 2, 4, 6, 10, 20
					break;
				case EnchantmentTypeID.LifeSteal:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.005f;
							break;
						case 1:
							EnchantmentStrength = 0.01f;
							break;
						case 2:
							EnchantmentStrength = 0.02f;
							break;
						case 3:
							EnchantmentStrength = 0.03f;
							break;
						case 4:
							EnchantmentStrength = 0.04f;
							break;
					}// 0.005, 0.01, 0.02, 0.03, 0.04
					break;
				case EnchantmentTypeID.AllForOne:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 2f;
							break;
						case 1:
							EnchantmentStrength = 3f;
							break;
						case 2:
							EnchantmentStrength = 5f;
							break;
						case 3:
							EnchantmentStrength = 8f;
							break;
						case 4:
							EnchantmentStrength = 10f;
							break;
					}// 2, 3, 5, 8, 10
					break;
				case EnchantmentTypeID.GodSlayer:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.02f;
							break;
						case 1:
							EnchantmentStrength = 0.04f;
							break;
						case 2:
							EnchantmentStrength = 0.06f;
							break;
						case 3:
							EnchantmentStrength = 0.08f;
							break;
						case 4:
							EnchantmentStrength = 0.10f;
							break;
					}// 0.02, 0.04, 0.06, 0.08, 0.10
					break;
				case EnchantmentTypeID.Splitting:
				case EnchantmentTypeID.CatastrophicRelease:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.5f;
							break;
						case 1:
							EnchantmentStrength = 0.6f;
							break;
						case 2:
							EnchantmentStrength = 0.75f;
							break;
						case 3:
							EnchantmentStrength = 0.85f;
							break;
						case 4:
							EnchantmentStrength = 1f;
							break;
					}//0.5, 0.6, 0.75, 0.85, 1
					break;
				case EnchantmentTypeID.ColdSteel:
				case EnchantmentTypeID.HellsWrath:
				case EnchantmentTypeID.JunglesFury:
				case EnchantmentTypeID.Moonlight:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.25f;
							break;
						case 1:
							EnchantmentStrength = 0.30f;
							break;
						case 2:
							EnchantmentStrength = 0.35f;
							break;
						case 3:
							EnchantmentStrength = 0.4f;
							break;
						case 4:
							EnchantmentStrength = 0.5f;
							break;
					}// 0.25, 0.30, 0.35, 0.4, 0.5
					break;
				default:
					switch (EnchantmentSize)
					{
						case 0:
							EnchantmentStrength = 0.03f;
							break;
						case 1:
							EnchantmentStrength = 0.08f;
							break;
						case 2:
							EnchantmentStrength = 0.16f;
							break;
						case 3:
							EnchantmentStrength = 0.25f;
							break;
						case 4:
							EnchantmentStrength = 0.40f;
							break;
					}//0.03, 0.08, 0.16, 0.25, 0.40
					break;
			}//EnchantmentStrength
			switch ((EnchantmentTypeID)EnchantmentType)
			{
				case EnchantmentTypeID.GodSlayer:
					DamageClassSpecific = (int)DamageTypeSpecificID.Melee;
					break;
				case EnchantmentTypeID.Splitting:
					DamageClassSpecific = (int)DamageTypeSpecificID.Ranged;
					break;
				case EnchantmentTypeID.CatastrophicRelease:
					DamageClassSpecific = (int)DamageTypeSpecificID.Magic;
					break;
				case EnchantmentTypeID.ColdSteel:
				case EnchantmentTypeID.HellsWrath:
				case EnchantmentTypeID.JunglesFury:
				case EnchantmentTypeID.Moonlight:
					RestrictedClass = (int)DamageTypeSpecificID.Summon;
					break;
				case EnchantmentTypeID.AllForOne:
				case EnchantmentTypeID.OneForAll:
					Max1 = true;
					break;
				default:
					DamageClassSpecific = 0;
					break;
			}//DamageTypeSpecific, Max1, RestrictedClass
			switch ((EnchantmentTypeID)EnchantmentType)
			{
				case EnchantmentTypeID.Scale:
					MyDisplayName = "Size";
					break;
				case EnchantmentTypeID.StatDefense:
					MyDisplayName = "Defence";
					break;
				default:
					MyDisplayName = EnchantmentTypeName;
					break;
			}//New Display Name
			if (!checkedStats)
			{
				switch ((EnchantmentTypeID)EnchantmentType)
				{
					case EnchantmentTypeID.StatDefense:
						AllowedList.Add("Weapon", 0.5f);
						AllowedList.Add("Armor", 1f);
						AllowedList.Add("Accessory", 1f);
						break;
					case EnchantmentTypeID.Peace:
					case EnchantmentTypeID.War:
						AllowedList.Add("Weapon", 1f);
						AllowedList.Add("Armor", 1f);
						AllowedList.Add("Accessory", 1f);
						break;
					case EnchantmentTypeID.AllForOne:
					case EnchantmentTypeID.OneForAll:
					case EnchantmentTypeID.LifeSteal:
					case EnchantmentTypeID.ColdSteel:
					case EnchantmentTypeID.HellsWrath:
					case EnchantmentTypeID.JunglesFury:
					case EnchantmentTypeID.Moonlight:
					case EnchantmentTypeID.CatastrophicRelease:
						AllowedList.Add("Weapon", 1f);
						break;
					default:
						AllowedList.Add("Weapon", 1f);
						AllowedList.Add("Armor", 0.25f);
						AllowedList.Add("Accessory", 0.25f);
						break;
				}//AllowedList
				int buffDuration = GetBuffDuration();
				switch ((EnchantmentTypeID)EnchantmentType)
				{
					case EnchantmentTypeID.AllForOne:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, EnchantmentStrength));
						EStats.Add(new EStat("Damage", 0f, EnchantmentStrength));
						EStats.Add(new EStat("NPCHitCooldown", 0f, EnchantmentStrength * 0.8f));
						AddStaticStat("useTime", 0f, EnchantmentStrength * 0.2f);
						AddStaticStat("useAnimation", 0f, EnchantmentStrength * 0.2f);
						AddStaticStat("mana", EnchantmentStrength * 0.3f);
						StaticStat = AddStaticStat("P_autoReuse", EnchantmentStrength);
						break;
					case EnchantmentTypeID.AmmoCost:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, -EnchantmentStrength));
						break;
					case EnchantmentTypeID.ArmorPenetration:
					case EnchantmentTypeID.CriticalStrikeChance:
					case EnchantmentTypeID.Scale:
					case EnchantmentTypeID.StatDefense:
						StaticStat = CheckStaticStatByName();
						break;
					case EnchantmentTypeID.CatastrophicRelease:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, EnchantmentStrength));
						EStats.Add(new EStat("InfinitePenetration", 0f, 1f, EnchantmentStrength));
						AddStaticStat("scale", 0f, EnchantmentStrength * 10f);
						AddStaticStat("shootSpeed", 0f, -0.8f * EnchantmentStrength);
						StaticStat = AddStaticStat("P_autoReuse", EnchantmentStrength);
						break;
					case EnchantmentTypeID.ColdSteel:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						NewDamageType = (int)DamageTypeSpecificID.SummonMeleeSpeed;
						if (EnchantmentSize == 4) OnHitBuff.Add(BuffID.CoolWhipPlayerBuff, buffDuration);
						Debuff.Add(BuffID.RainbowWhipNPCDebuff, buffDuration);
						Debuff.Add(BuffID.Frostburn, buffDuration);
						EStats.Add(new EStat("Damage", 0f, EnchantmentStrength));
						break;
					case EnchantmentTypeID.HellsWrath:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						NewDamageType = (int)DamageTypeSpecificID.SummonMeleeSpeed;
						Debuff.Add(BuffID.FlameWhipEnemyDebuff, buffDuration);
						Debuff.Add(BuffID.RainbowWhipNPCDebuff, buffDuration);
						Debuff.Add(EnchantmentSize == 4 ? BuffID.OnFire3 : BuffID.OnFire, buffDuration);
						EStats.Add(new EStat("Damage", 0f, EnchantmentStrength));
						break;
					case EnchantmentTypeID.JunglesFury:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						NewDamageType = (int)DamageTypeSpecificID.SummonMeleeSpeed;
						OnHitBuff.Add(BuffID.SwordWhipPlayerBuff, buffDuration);
						Debuff.Add(BuffID.SwordWhipNPCDebuff, buffDuration);
						Debuff.Add(BuffID.RainbowWhipNPCDebuff, buffDuration);
						Debuff.Add(EnchantmentSize == 4 ? BuffID.Venom : BuffID.Poisoned, buffDuration);
						EStats.Add(new EStat("Damage", 0f, EnchantmentStrength));
						break;
					case EnchantmentTypeID.Moonlight:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						NewDamageType = (int)DamageTypeSpecificID.SummonMeleeSpeed;
						OnHitBuff.Add(BuffID.ScytheWhipPlayerBuff, buffDuration);
						if (EnchantmentSize == 4) Debuff.Add(BuffID.ScytheWhipEnemyDebuff, buffDuration);
						Debuff.Add(BuffID.RainbowWhipNPCDebuff, buffDuration);
						EStats.Add(new EStat("Damage", 0f, EnchantmentStrength));
						break;
					case EnchantmentTypeID.Damage:
						EStats.Add(new EStat(EnchantmentTypeName, EnchantmentStrength));
						break;
					case EnchantmentTypeID.DangerSense:
					case EnchantmentTypeID.Hunter:
					case EnchantmentTypeID.Spelunker:
					case EnchantmentTypeID.ObsidianSkin:
						CheckBuffByName();
						break;
					case EnchantmentTypeID.Mana:
						AddStaticStat(EnchantmentTypeName.ToFieldName(), -EnchantmentStrength);
						break;
					case EnchantmentTypeID.OneForAll:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						EStats.Add(new EStat("NPCHitCooldown", 0f, 1f + EnchantmentStrength * 0.3f));
						AddStaticStat("useTime", 0f, 1f + EnchantmentStrength * 0.3f);
						AddStaticStat("useAnimation", 0f, 1f + EnchantmentStrength * 0.3f);
						break;
					case EnchantmentTypeID.Peace:
					case EnchantmentTypeID.War:
						EStats.Add(new EStat("spawnRate", 0f, 1f + EnchantmentStrength));
						EStats.Add(new EStat("maxSpawns", 0f, 1f + EnchantmentStrength));
						break;
					case EnchantmentTypeID.Speed:
						EStats.Add(new EStat("I_NPCHitCooldown", EnchantmentStrength));
						AddStaticStat("I_useTime", EnchantmentStrength);
						AddStaticStat("I_useAnimation", EnchantmentStrength);
						StaticStat = AddStaticStat("autoReuse", EnchantmentStrength);
						break;
					default:
						EStats.Add(new EStat(EnchantmentTypeName, 0f, 1f, 0f, EnchantmentStrength));
						break;
				}//Set Stats
				StaticStat = StaticStats.Count > 0;
				ShortToolTip = GenerateShortTooltip();
				checkedStats = true;
			}//SetStats and AllowedList
		}
		public override void SetDefaults()
		{
			Item.maxStack = 99;
			GetDefaults();
		}
		private void GetPercentageMult100(string s, out bool percentage, out bool multiply100, out bool plus, bool staticStat = false)
		{
			percentage = true;
			multiply100 = true;
			plus = staticStat;
			switch (s)
			{
				case "ArmorPenetration":
				case "statDefense":
					percentage = false;
					multiply100 = false;
					plus = true;
					break;
				case "crit":
					multiply100 = false;
					plus = true;
					break;
				case "Damage":
				case "NPCHitCooldown":
					plus = true;
					break;
			}//percentage, multiply100
		}
		public string CheckStatAlteredName(string name)
		{
			switch (name)
			{
				case "crit":
				case "statDefense":
				case "scale":
					return MyDisplayName.AddSpaces();
				case "Damage":
					return "Damage dealt(Not visible in weapon stats applied at damage calculation)";
				default:
					return name.CapitalizeFirst().AddSpaces();
			}
		}
		private bool CheckStaticStatByName(string checkName = "", bool checkBoolOnly = false)
		{
			if (checkName == "")
				checkName = Name;
			foreach (FieldInfo field in Item.GetType().GetFields())
			{
				string fieldName = field.Name;
				if(fieldName.Length <= checkName.Length)
                {
					string name = UtilityMethods.ToFieldName(checkName.Substring(0, fieldName.Length));
					if (fieldName == name)
					{
						if (checkBoolOnly)
							return field.FieldType == typeof(bool);
						else
							switch (name)
							{
								case "crit":
									StaticStats.Add(new EnchantmentStaticStat(fieldName, 0f, 1f, 0f, EnchantmentStrength * 100));
									break;
								default:
									StaticStats.Add(new EnchantmentStaticStat(fieldName, EnchantmentStrength));
									break;
							}
						return true;
					}
				}
			}
			foreach (PropertyInfo property in Item.GetType().GetProperties())
			{
				string name = property.Name;
				if (name.Length <= checkName.Length)
                {
					if (name == checkName.Substring(0, name.Length))
					{
						if (checkBoolOnly)
							return property.PropertyType == typeof(bool);
						else
							StaticStats.Add(new EnchantmentStaticStat(name, EnchantmentStrength));
						return true;
					}
				}
			}
			Player player = new();
			foreach (FieldInfo field in player.GetType().GetFields())
			{
				string fieldName = field.Name;
				if (fieldName.Length <= checkName.Length)
				{
					string name = UtilityMethods.ToFieldName(checkName.Substring(0, fieldName.Length));
					if (fieldName == name)
					{
						if (checkBoolOnly)
							return field.FieldType == typeof(bool);
						else
							switch (name)
							{
								case "statDefense":
									StaticStats.Add(new EnchantmentStaticStat(fieldName, 0f, 1f, 0f, EnchantmentStrength));
									break;
								default:
									StaticStats.Add(new EnchantmentStaticStat(fieldName, EnchantmentStrength));
									break;
							}
						return true;
					}
				}
			}
			foreach (PropertyInfo property in player.GetType().GetProperties())
			{
				string name = property.Name;
				if (name.Length <= checkName.Length)
				{
					if (name == checkName.Substring(0, name.Length))
					{
						if (checkBoolOnly)
							return property.PropertyType == typeof(bool);
						else
							StaticStats.Add(new EnchantmentStaticStat(name, EnchantmentStrength));
						return true;
					}
				}
			}
			return false;
		}
		private bool AddStaticStat(string name, float additive = 0f, float multiplicative = 1f, float flat = 0f, float @base = 0f)
        {
			StaticStats.Add(new EnchantmentStaticStat(name, additive, multiplicative, flat, @base));
			return true;
		}
		private bool CheckBuffByName(bool debuff = false, string baseName = "")
        {
			if (baseName == "")
				baseName = Name;
			BuffID buffID = new();
			foreach(FieldInfo field in buffID.GetType().GetFields())
            {
				string fieldName = field.Name;
				if (fieldName.Length <= baseName.Length)
				{
					string name = baseName.Substring(0, fieldName.Length);
					if (fieldName.ToLower() == name.ToLower())
					{
						if(debuff)
							Debuff.Add((int)buffID.GetType().GetField(fieldName).GetValue(buffID), GetBuffDuration());
						else
							Buff.Add((int)buffID.GetType().GetField(fieldName).GetValue(buffID));
						//StaticStats.Add(new StaticStatStruct(fieldName, EnchantmentStrength));
						return true;
					}
				}
			}
			return false;
        }
		private string GetBuffName(int id)
		{
			BuffID buffID = new();
			foreach (FieldInfo field in buffID.GetType().GetFields())
			{
				if(field.FieldType == typeof(int) && (int)field.GetValue(buffID) == id)
                {
					return field.Name;
                }
			}
			return "";
		}
		private string GenerateShortTooltip(bool forFullToolTip = false, bool firstToolTip = false)
        {
			if(EStats.Count > 0)
            {
				EStat baseNameEStat = EStats[0];
				return GetEStatToolTip(baseNameEStat, forFullToolTip, firstToolTip);
			}
			else if(Buff.Count > 0)
            {
				return $"Grants {MyDisplayName.AddSpaces()} Buff (tier {EnchantmentSize})";
            }
			else if(StaticStats.Count > 0)
            {
				EnchantmentStaticStat baseNameStaticStat = StaticStats[0];
				return GetStaticStatToolTip(baseNameStaticStat, forFullToolTip, firstToolTip);
			};
			return "";
        }
		private string GenerateFullTooltip(string uniqueTooltip)
        {
			string shortTooltip = GenerateShortTooltip(true, true);
			string toolTip = $"{shortTooltip}{(uniqueTooltip != "" ? "\n" : "")}{uniqueTooltip}";
			if (NewDamageType > -1)
				toolTip += $"\nConverts weapon damage type to {((DamageTypeSpecificID)GetDamageClass(NewDamageType)).ToString().AddSpaces()}";
			if(EStats.Count > 0)
            {
				foreach (EStat eStat in EStats)
				{
					string eStatToolTip = GetEStatToolTip(eStat, true);
					if (eStatToolTip != shortTooltip)
						toolTip += $"\n{eStatToolTip}";
				}
			}//Estats
			if (StaticStats.Count > 0)
            {
				foreach (EnchantmentStaticStat staticStat in StaticStats)
				{
					string staticStatToolTip = GetStaticStatToolTip(staticStat, true);
					if (staticStatToolTip != shortTooltip)
						toolTip += $"\n{staticStatToolTip}";
				}
			}//StaticStats
			if (OnHitBuff.Count > 0)
            {
				int i = 0;
				bool first = true;
				foreach (int onHitBuff in OnHitBuff.Keys)
				{
					string buffName = GetBuffName(onHitBuff).AddSpaces();
					if (first)
					{
						toolTip += $"\nOn Hit Buffs: {buffName}";
						first = false;
					}
					else if (i == OnHitBuff.Count - 1)
						toolTip += $" and {buffName}";
					else
						toolTip += $", {buffName}";
					i++;
				}
			}//OnHitBuffs
			if(Debuff.Count > 0)
            {
				int i = 0;
				bool first = true;
				foreach (int debuff in Debuff.Keys)
				{
					string buffName = GetBuffName(debuff).AddSpaces();
					if (first)
					{
						toolTip += $"\nOn Hit Debuffs: {buffName}";
						first = false;
					}
					else if (i == Debuff.Count - 1)
						toolTip += $" and {buffName}";
					else
						toolTip += $", {buffName}";
					i++;
				}
			}//Debuffs
			toolTip += $"\nLevel cost: { GetLevelCost()}";
			if (DamageClassSpecific > 0 || Unique || RestrictedClass > 0)
			{
				string limmitationToolTip = "";
				if (Unique)
					limmitationToolTip = "\n   *" + UtilityMethods.AddSpaces(Item.ModItem.Name) + " Only*";
				else if (DamageClassSpecific > 0)
					limmitationToolTip = $"\n   *{((DamageTypeSpecificID)GetDamageClass(DamageClassSpecific)).ToString().AddSpaces()} Only*";
				else if (RestrictedClass > 0)
					limmitationToolTip = $"\n   *Not allowed on {((DamageTypeSpecificID)GetDamageClass(RestrictedClass)).ToString().AddSpaces()} weapons*";
				limmitationToolTip += "\n   *Unique* (Limmited to 1 Unique Enchantment)";
				toolTip += limmitationToolTip;
			}//Unique, DamageClassSpecific, RestrictedClass
			if (AllowedList.Count < 3)
            {
				int i = 0;
				bool first = true;
				foreach (string key in AllowedList.Keys)
                {
					if (first)
					{
						toolTip += $"\n   *{key}{(AllowedList.Count == 1 ? " Only*" : "")}";
						first = false;
					}
					else if (i == AllowedList.Count - 1)
						toolTip += $" and {key} Only*";
					else
						toolTip += $", {key}";
					i++;
				}
            }//AllowedList
			if (Max1)
				toolTip += "\n   *Max of 1 per weapon*";
			toolTip += Utility ? "\n   *Utility*" : "";
			return toolTip;
		}
		public string GetEStatToolTip(EStat eStat, bool forFullToolTip = false, bool firstToolTip = false)
        {
			string toolTip = "";
			bool percentage, multiply100, plus;
			GetPercentageMult100(eStat.StatName, out percentage, out multiply100, out plus);
			string statName;
			bool invert = forFullToolTip && !firstToolTip && eStat.StatName.Substring(0, 2) == "I_";
			if (invert)
				statName = eStat.StatName.Substring(2);
			else
				statName = eStat.StatName;
			EStat enchantmentStat = new EStat(statName, eStat.Additive * (invert ? -1f : 1f), invert ? 1f / eStat.Multiplicative : eStat.Multiplicative, eStat.Flat * (invert ? -1f : 1f), eStat.Base * (invert ? -1f : 1f));
			if (enchantmentStat.Additive != 0f || enchantmentStat.Multiplicative != 1f)
			{
				if (enchantmentStat.Additive != 0f)
					toolTip += (plus ? (enchantmentStat.Additive > 0f ? "+" : "") : "") + $"{enchantmentStat.Additive * (multiply100 ? 100 : 1)}{(percentage ? "%" : "")}";
				else if (enchantmentStat.Multiplicative != 1f)
					toolTip += $"{enchantmentStat.Multiplicative}x";
			}
			else
			{
				float num = enchantmentStat.Base != 0f ? enchantmentStat.Base : enchantmentStat.Flat;
				toolTip += (plus ? (num > 0f ? "+" : "") : "") + $"{num * (multiply100 ? 100 : 1)}{(percentage ? "%" : "")}";// " + (enchantmenteStat.Base != 0f ? "base" : "");
			}
			toolTip += $" {(forFullToolTip ? CheckStatAlteredName(firstToolTip ? MyDisplayName : enchantmentStat.StatName) : MyDisplayName)}";
			return toolTip;
		}
		public string GetStaticStatToolTip(EnchantmentStaticStat staticStat, bool forFullToolTip = false, bool firstToolTip = false)
        {
			string toolTip = "";
			string statName;
			bool invert = staticStat.Name.Substring(0, 2) == "I_";
			bool prevent = staticStat.Name.Substring(0, 2) == "P_";
			if (invert || prevent)
				statName = staticStat.Name.Substring(2);
			else
				statName = staticStat.Name;
			bool statIsBool = CheckStaticStatByName(statName, true);
			if(statIsBool)
            {
				statName = statName.CapitalizeFirst().AddSpaces();
				if (prevent)
					statName = $"Prevent {statName}";
				toolTip = statName;
            }
            else
            {
				EnchantmentStaticStat enchantmentStaticStat = new EnchantmentStaticStat(statName, staticStat.Additive * (invert ? -1f : 1f), invert ? 1f / staticStat.Multiplicative : staticStat.Multiplicative, staticStat.Flat * (invert ? -1f : 1f), staticStat.Base * (invert ? -1f : 1f));
				bool percentage, multiply100, plus;
				GetPercentageMult100(enchantmentStaticStat.Name, out percentage, out multiply100, out plus, true);
				if (enchantmentStaticStat.Additive != 0f || enchantmentStaticStat.Multiplicative != 1f)
				{
					if (enchantmentStaticStat.Additive != 0f)
						toolTip += (plus ? (enchantmentStaticStat.Additive > 0f ? "+" : "") : "") + $"{enchantmentStaticStat.Additive * (multiply100 ? 100 : 1)}{(percentage ? "%" : "")}";
					else if (enchantmentStaticStat.Multiplicative != 1f)
						toolTip += $"{enchantmentStaticStat.Multiplicative}x";
				}
				else
				{
					float num = enchantmentStaticStat.Base != 0f ? enchantmentStaticStat.Base : enchantmentStaticStat.Flat;
					toolTip += (plus ? (num > 0f ? "+" : "") : "") + $"{num * (multiply100 ? 100 : 1)}{(percentage ? "%" : "")}";// " + (enchantmentStaticStat.Base != 0f ? "base" : "");
				}
				toolTip += $" {(forFullToolTip ? CheckStatAlteredName(firstToolTip ? MyDisplayName : enchantmentStaticStat.Name) : MyDisplayName)}";
			}
			return toolTip;
		}
		public static int GetDamageClass(int damageType)
        {
			switch ((DamageTypeSpecificID)damageType)
			{
				case DamageTypeSpecificID.Melee:
				case DamageTypeSpecificID.MeleeNoSpeed:
					return (int)DamageTypeSpecificID.Melee;
				case DamageTypeSpecificID.Ranged:
					return (int)DamageTypeSpecificID.Ranged;
				case DamageTypeSpecificID.Magic:
					return (int)DamageTypeSpecificID.Magic;
				case DamageTypeSpecificID.Summon:
				case DamageTypeSpecificID.MagicSummonHybrid:
				case DamageTypeSpecificID.SummonMeleeSpeed:
					return (int)DamageTypeSpecificID.Summon;
				case DamageTypeSpecificID.Throwing:
					return (int)DamageTypeSpecificID.Throwing;
				default:
					return (int)DamageTypeSpecificID.Generic;
			}
		}
		private int GetBuffDuration()
        {
			return defaultBuffDuration * (EnchantmentSize + 1);
		}
		public static int GetEnchantmentSize(string name)
        {
			for (int i = 0; i < rarity.Length; i++)
			{
				if (rarity[i] == name.Substring(name.IndexOf("Enchantment") + 11))
				{
					return i;
				}
			}//Get EnchantmentSize
			return -1;
		}
		public override void AddRecipes()
		{
			if (EnchantmentSize > -1)
			{
				for (int i = EnchantmentSize; i < rarity.Length; i++)
				{
					Recipe recipe;
					int skipIfLessOrEqualToSize;
					switch ((EnchantmentTypeID)EnchantmentType)
					{
						case EnchantmentTypeID.Spelunker:
						case EnchantmentTypeID.DangerSense:
						case EnchantmentTypeID.Hunter:
						case EnchantmentTypeID.ObsidianSkin:
							skipIfLessOrEqualToSize = 4;
							break;
						case EnchantmentTypeID.Damage:
						case EnchantmentTypeID.StatDefense:
							skipIfLessOrEqualToSize = -1;
							break;
						default:
							skipIfLessOrEqualToSize = 0;
							break;
					}
					if (EnchantmentSize > skipIfLessOrEqualToSize)
					{
						for (int j = EnchantmentSize; j >= skipIfLessOrEqualToSize + 1; j--)
						{
							recipe = CreateRecipe();
							for (int k = EnchantmentSize; k >= j; k--)
							{
								int essenceNumber = Utility ? 5 : 10;
								recipe.AddIngredient(Mod, "EnchantmentEssence" + EnchantmentEssenceBasic.rarity[k], essenceNumber);
							}
							if (j > 0)
							{
								recipe.AddIngredient(Mod, EnchantmentTypeName + "Enchantment" + rarity[j - 1], 1);
							}
							if (EnchantmentSize < 3)
							{
								recipe.AddIngredient(Mod, Containment.sizes[EnchantmentSize] + "Containment", 1);
							}
							else if (j < 3)
							{
								recipe.AddIngredient(Mod, Containment.sizes[2] + "Containment", 1);
							}
							if(EnchantmentSize == 3)
                            {
								recipe.AddRecipeGroup("WeaponEnchantments:CommonGems",  2);
							}
							if (EnchantmentSize == 4)
							{
								recipe.AddRecipeGroup("WeaponEnchantments:RareGems");
							}
							recipe.AddTile(Mod, WoodEnchantingTable.enchantingTableNames[i] + "EnchantingTable");
							recipe.Register();
						}
					}
				}
			}
		}
		public int GetLevelCost()
        {
            switch ((EnchantmentTypeID)EnchantmentType)
            {
				case EnchantmentTypeID.AllForOne:
				case EnchantmentTypeID.OneForAll:
				case EnchantmentTypeID.Splitting:
				case EnchantmentTypeID.ColdSteel:
				case EnchantmentTypeID.HellsWrath:
				case EnchantmentTypeID.JunglesFury:
				case EnchantmentTypeID.Moonlight:
					return (1 + EnchantmentSize) * 3;
				default:
					return Utility ? (1 + EnchantmentSize) * 1 : (1 + EnchantmentSize) * 2;
			}
        }
	}
	public class AllForOneEnchantmentCommon : AllForOneEnchantmentBasic { }public class AllForOneEnchantmentRare : AllForOneEnchantmentBasic { }public class AllForOneEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class AllForOneEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class AmmoCostEnchantmentBasic : AllForOneEnchantmentBasic { }public class AmmoCostEnchantmentCommon : AllForOneEnchantmentBasic { }public class AmmoCostEnchantmentRare : AllForOneEnchantmentBasic { }public class AmmoCostEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class AmmoCostEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class ArmorPenetrationEnchantmentBasic : AllForOneEnchantmentBasic { }public class ArmorPenetrationEnchantmentCommon : AllForOneEnchantmentBasic { }public class ArmorPenetrationEnchantmentRare : AllForOneEnchantmentBasic { }public class ArmorPenetrationEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class ArmorPenetrationEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class ColdSteelEnchantmentBasic : AllForOneEnchantmentBasic { }public class ColdSteelEnchantmentCommon : AllForOneEnchantmentBasic { }public class ColdSteelEnchantmentRare : AllForOneEnchantmentBasic { }public class ColdSteelEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class ColdSteelEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class CriticalStrikeChanceEnchantmentBasic : AllForOneEnchantmentBasic { }public class CriticalStrikeChanceEnchantmentCommon : AllForOneEnchantmentBasic { }public class CriticalStrikeChanceEnchantmentRare : AllForOneEnchantmentBasic { }public class CriticalStrikeChanceEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class CriticalStrikeChanceEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class DamageEnchantmentBasic : AllForOneEnchantmentBasic { }public class DamageEnchantmentCommon : AllForOneEnchantmentBasic { }public class DamageEnchantmentRare : AllForOneEnchantmentBasic { }public class DamageEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class DamageEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class DangerSenseEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class GodSlayerEnchantmentBasic : AllForOneEnchantmentBasic { }public class GodSlayerEnchantmentCommon : AllForOneEnchantmentBasic { }public class GodSlayerEnchantmentRare : AllForOneEnchantmentBasic { }public class GodSlayerEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class GodSlayerEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class HellsWrathEnchantmentBasic : AllForOneEnchantmentBasic { }public class HellsWrathEnchantmentCommon : AllForOneEnchantmentBasic { }public class HellsWrathEnchantmentRare : AllForOneEnchantmentBasic { }public class HellsWrathEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class HellsWrathEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class HunterEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class JunglesFuryEnchantmentBasic : AllForOneEnchantmentBasic { }public class JunglesFuryEnchantmentCommon : AllForOneEnchantmentBasic { }public class JunglesFuryEnchantmentRare : AllForOneEnchantmentBasic { }public class JunglesFuryEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class JunglesFuryEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class LifeStealEnchantmentBasic : AllForOneEnchantmentBasic { }public class LifeStealEnchantmentCommon : AllForOneEnchantmentBasic { }public class LifeStealEnchantmentRare : AllForOneEnchantmentBasic { }public class LifeStealEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class LifeStealEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class ManaEnchantmentBasic : AllForOneEnchantmentBasic { }public class ManaEnchantmentCommon : AllForOneEnchantmentBasic { }public class ManaEnchantmentRare : AllForOneEnchantmentBasic { }public class ManaEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class ManaEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class MoonlightEnchantmentBasic : AllForOneEnchantmentBasic { }public class MoonlightEnchantmentCommon : AllForOneEnchantmentBasic { }public class MoonlightEnchantmentRare : AllForOneEnchantmentBasic { }public class MoonlightEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class MoonlightEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class ObsidianSkinEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class OneForAllEnchantmentBasic : AllForOneEnchantmentBasic { }public class OneForAllEnchantmentCommon : AllForOneEnchantmentBasic { }public class OneForAllEnchantmentRare : AllForOneEnchantmentBasic { }public class OneForAllEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class OneForAllEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class PeaceEnchantmentBasic : AllForOneEnchantmentBasic { }public class PeaceEnchantmentCommon : AllForOneEnchantmentBasic { }public class PeaceEnchantmentRare : AllForOneEnchantmentBasic { }public class PeaceEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class PeaceEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class ScaleEnchantmentBasic : AllForOneEnchantmentBasic { }public class ScaleEnchantmentCommon : AllForOneEnchantmentBasic { }public class ScaleEnchantmentRare : AllForOneEnchantmentBasic { }public class ScaleEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class ScaleEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class SpeedEnchantmentBasic : AllForOneEnchantmentBasic { }public class SpeedEnchantmentCommon : AllForOneEnchantmentBasic { }public class SpeedEnchantmentRare : AllForOneEnchantmentBasic { }public class SpeedEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class SpeedEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class SpelunkerEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class SplittingEnchantmentBasic : AllForOneEnchantmentBasic { }public class SplittingEnchantmentCommon : AllForOneEnchantmentBasic { }public class SplittingEnchantmentRare : AllForOneEnchantmentBasic { }public class SplittingEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class SplittingEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class StatDefenseEnchantmentBasic : AllForOneEnchantmentBasic { }public class StatDefenseEnchantmentCommon : AllForOneEnchantmentBasic { }public class StatDefenseEnchantmentRare : AllForOneEnchantmentBasic { }public class StatDefenseEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class StatDefenseEnchantmentUltraRare : AllForOneEnchantmentBasic { }
	public class WarEnchantmentBasic : AllForOneEnchantmentBasic { }public class WarEnchantmentCommon : AllForOneEnchantmentBasic { }public class WarEnchantmentRare : AllForOneEnchantmentBasic { }public class WarEnchantmentSuperRare : AllForOneEnchantmentBasic { }public class WarEnchantmentUltraRare : AllForOneEnchantmentBasic { }
}
