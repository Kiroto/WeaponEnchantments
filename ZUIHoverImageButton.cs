﻿/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;

namespace WeaponEnchantments
{
		// This UIHoverImageButton class inherits from UIImageButton. 
		// Inheriting is a great tool for UI design. 
		// By inheriting, we get the Image drawing, MouseOver sound, and fading for free from UIImageButton
		// We've added some code to allow the Button to show a text tooltip while hovered. 
		
	internal class UIHoverImageButton : UIImageButton
	{
		internal string HoverText;

		public UIHoverImageButton(Asset<Texture2D> texture, string hoverText) : base(texture)
		{
			HoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			if (IsMouseHovering)
			{
				Main.hoverItemName = HoverText;
			}
		}
	}
}
*/