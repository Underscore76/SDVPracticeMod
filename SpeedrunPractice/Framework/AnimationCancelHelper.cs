using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
//using Rectangle = xTile.Dimensions.Rectangle;


namespace SpeedrunPractice.Framework
{

    public class AnimationCancelHelper
    {
        private bool isCancellableSwing;
        private const int FadeCounterMax = 60;
        public const int ClickThreshold = 30;
        public List<int> ClickRegions = new List<int>() { 10, 10, -1 };
        private int CurrentTick;
        private int LastCancelTick = -1;
        private int fadeCounter;

        private List<Color> ColorRegions = new List<Color>() {
            Color.Red, Color.Green, Color.Orange, Color.OrangeRed
        };
        private List<int> FrameRegions = new List<int>() {-1, 5, 8, -1 };
        public int GoodFrames
        {
            get { return FrameRegions[1]; }
            set { FrameRegions[1] = value; }
        }
        public int OkayFrames
        {
            get { return FrameRegions[2]; }
            set { FrameRegions[2] = value; }
        }
        private int CurrentFrame;
        private List<int> AnimationFrames;
        private List<Color> AnimationColors;
        private TimeSpan FrameTimeSpan = new TimeSpan(166667);
        private List<int> ValidAnimationTypes;

        public bool IsEarlyCancel(out int diff)
        {
            diff = AnimationFrames[0] - CurrentFrame;
            return diff > 0;
        }
        public bool IsGoodCancel(out int diff)
        {
            diff = CurrentFrame - AnimationFrames[0];
            return 0 <= diff && diff < AnimationFrames[1];
        }
        public bool IsOkayCancel(out int diff)
        {
            diff = CurrentFrame - AnimationFrames[0];
            return AnimationFrames[1] <= diff && diff < AnimationFrames[1] + AnimationFrames[2];
        }
        public bool IsSlowCancel(out int diff)
        {
            diff = CurrentFrame - AnimationFrames[0];
            return AnimationFrames[1] + AnimationFrames[2] <= diff;
        }
        public AnimationCancelHelper()
        {
            ValidAnimationTypes = new List<int>()
            {
                FarmerSprite.toolUp,
                FarmerSprite.toolRight,
                FarmerSprite.toolDown,
                FarmerSprite.toolLeft,
                180, 172, 164, 188 // watering can based
            };
        }

        public void Update(IMonitor monitor, IModHelper helper)
        {
            CurrentTick++;
            if (PlayerInfo.UsingTool && !(PlayerInfo.CurrentTool is MeleeWeapon) && PlayerInfo.CurrentSprite != null)
            {
                if (LastCancelTick != -1 && CurrentTick != LastCancelTick)
                {
                    int tickDiff = CurrentTick - LastCancelTick;
                    if (tickDiff < ClickThreshold)
                    {
                        if (tickDiff < ClickRegions[0])
                        {
                            Alerts.Success(string.Format("Fast: {0}", tickDiff));
                        }
                        else if (ClickRegions[0] <= tickDiff && tickDiff < ClickRegions[0] + ClickRegions[1])
                        {
                            Alerts.Info(string.Format("Okay: {0}", tickDiff));
                        }
                        else
                        {
                            Alerts.Failure(string.Format("Slow: {0}", tickDiff));
                        }
                        LastCancelTick = -1;
                    }
                }
                int animationType = PlayerInfo.AnimationType;
                if (!isCancellableSwing)
                    CurrentFrame = 1;
                else
                    CurrentFrame++;
                isCancellableSwing = ValidAnimationTypes.Contains(animationType);
                fadeCounter = FadeCounterMax;

                GetAnimationCancelDetails(PlayerInfo.CurrentSprite, out int cancelFrame, out int totalFrames);
                FrameRegions[0] = cancelFrame;


                int currFrame = 0;
                AnimationFrames = new List<int>();
                AnimationColors = new List<Color>();
                foreach (var p in FrameRegions.Zip(ColorRegions, Tuple.Create))
                {
                    if (p.Item1 == -1)
                    {
                        AnimationFrames.Add(totalFrames - currFrame);
                    }
                    else
                    {
                        AnimationFrames.Add(p.Item1);
                        currFrame += p.Item1;
                    }
                    AnimationColors.Add(p.Item2);
                }
            }
            else
            {
                if (isCancellableSwing && fadeCounter == FadeCounterMax)
                {
                    LastCancelTick = CurrentTick;
                    //int diff;
                    //if (IsEarlyCancel(out diff))
                    //{
                    //    Alerts.Failure(string.Format("Early (-{0})", diff));
                    //}
                    //else if (IsGoodCancel(out diff))
                    //{
                    //    Alerts.Success(string.Format("Good (+{0})", diff));
                    //}
                    //else if (IsOkayCancel(out diff))
                    //{
                    //    Alerts.Info(string.Format("Okay (+{0})", diff));
                    //}
                    //else if (IsSlowCancel(out diff))
                    //{
                    //    Alerts.Failure(string.Format("Late (+{0})", diff));
                    //}
                }
                isCancellableSwing = false;
                if (fadeCounter > 0)
                {
                    fadeCounter--;
                }
                if (fadeCounter <= 0)
                {
                    AnimationFrames = null;
                    AnimationColors = null;
                    CurrentFrame = -1;
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw centered quarter screen
            // draw 1/3 down from top
            // draw rectangle
            Vector2 playerTile = Game1.player.getTileLocation();
            Rectangle progressRectGlobal = new Rectangle((int)(playerTile.X - 3 + 0.5) * Game1.tileSize, (int)(playerTile.Y - 2) * Game1.tileSize, Game1.tileSize * 6, Game1.tileSize / 2);
            Rectangle progressRect = Game1.GlobalToLocal(Game1.viewport, progressRectGlobal);
            if (fadeCounter > 0)
            {
                DrawHelper.DrawProgressBar(spriteBatch, progressRect, AnimationFrames, AnimationColors, CurrentFrame, Color.LightYellow);
            }
            progressRect.Y += progressRect.Height;
            if (LastCancelTick != -1 && CurrentTick - LastCancelTick <= ClickThreshold)
            {
                DrawHelper.DrawProgressBar(spriteBatch, progressRect, new List<int> { ClickThreshold }, new List<Color> { Color.Black }, CurrentTick-LastCancelTick, Color.LightYellow);
            }
        }

        private void GetAnimationCancelDetails(FarmerSprite sprite, out int cancelFrame, out int totalFrames)
        {
            int currentFrame = 0;
            cancelFrame = 0;
            totalFrames = 0;
            for(int i = 0; i < sprite.CurrentAnimation.Count; i++)
            {
                int frames = Math.Max(1, (int)((sprite.CurrentAnimation[i].milliseconds + FrameTimeSpan.Milliseconds - 1) / FrameTimeSpan.TotalMilliseconds));
                if (sprite.CurrentAnimation[i].frameStartBehavior != null && sprite.CurrentAnimation[i].frameStartBehavior.Method.Name.Equals("useTool"))
                    cancelFrame = currentFrame;
                if (i > 0 && sprite.CurrentAnimation[i - 1].frameEndBehavior != null && sprite.CurrentAnimation[i - 1].frameEndBehavior.Method.Name.Equals("useTool"))
                    cancelFrame = currentFrame;
                currentFrame += frames;
            }
            totalFrames = currentFrame;
        }
    }
}
