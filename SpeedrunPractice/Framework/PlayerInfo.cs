using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;

namespace SpeedrunPractice.Framework
{
    public class PlayerInfo
    {
        public static bool Active { get { return Game1.player != null; } }
        
        public static bool CanMove { get { return Active && Game1.player.CanMove; } }
        
        public static bool UsingTool { get { return Active && Game1.player.UsingTool; } }

        public static Tool CurrentTool { get { return Game1.player?.CurrentTool; } }


        public static int FacingDirection { get { return Active ? Game1.player.FacingDirection : -1; } }

        public static FarmerSprite CurrentSprite {  get { return Game1.player?.FarmerSprite; } }

        public static int GetMouseFacingDirection(Vector2 position, bool worldCoordinates=true)
        {
            if (Active)
            {
                if (!worldCoordinates)
                {
                    position.X += Game1.viewport.X;
                    position.Y += Game1.viewport.Y;
                }
                Vector2 standingPosition = Game1.player.getStandingPosition();
                if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, Game1.player) &&
                    (Math.Abs(position.X - standingPosition.X) >= 32f || Math.Abs(position.Y - standingPosition.Y) >= 32f))
                {
                    return Game1.player.getGeneralDirectionTowards(position, 0, false);
                }
            }
            return FacingDirection;
        }

        public static int AnimationType
        {
            get
            {
                if (CurrentSprite == null)
                    return -1;

                return (int)Reflector.GetValue(CurrentSprite, "currentSingleAnimation"); ;
            }
        }
    }
}
