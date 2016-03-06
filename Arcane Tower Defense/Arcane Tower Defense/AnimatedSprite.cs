using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Arcane_Tower_Defense
{
    public class AnimatedSprite
    {
        public Texture2D texture { get; set; }
        public int rows { get; set; }
        public int columns { get; set; }
        public int currentFrame;
        public int totalFrames;
        private int frameDelay;
        private int frameControl;
        private int direction;
        private Boolean reverse;
        private List<int> timePerFrame;
        private int timeIterator;
        private int timeControl;
        private Color col;
        private float alpha;
        public String textureString;
        
        //assumes evenly divided spriteframes
        //delay is amount of time each frame is shown
        //reverse indicates whether sprite animation will reverse frames afterward
        public AnimatedSprite(String texture, int ro, int cols, int delay)
        {
            textureString = texture;
            rows = ro;
            columns = cols;
            currentFrame = 0;
            frameControl = 0;
            frameDelay = delay;
            totalFrames = rows * columns;
            reverse = false;
            direction = 1;
            col = Color.White;
            alpha = 1;
        }

        //if total frames is less than rows * columns
        /*
        public AnimatedSprite(Texture2D texture, int rows, int columns, int delay, int total)
        {
            Texture = texture;
            Rows = rows;
            Columns = columns;
            currentFrame = 0;
            frameControl = 0;
            frameDelay = delay;
            totalFrames = total;
            reverse = false;
            direction = 1;
        }*/

        public AnimatedSprite(String texture, int ro, int cols, int delay, Boolean rev)
        {
            textureString = texture;
            rows = ro;
            columns = cols;
            currentFrame = 0;
            frameControl = 0;
            frameDelay = delay;
            totalFrames = rows * columns;
            reverse = rev;
            direction = 1;
            col = Color.White;
            alpha = 1;
        }

        public AnimatedSprite(String texture, int ro, int cols, int delay, Boolean rev, List<int> time)
        {
            textureString = texture;
            rows = ro;
            columns = cols;
            currentFrame = 0;
            frameControl = 0;
            frameDelay = delay;
            totalFrames = rows * columns;
            reverse = rev;
            direction = 1;
            timePerFrame = time;
            timeIterator = 0;
            timeControl = timePerFrame[0];
            col = Color.White;
            alpha = 1;
        }

        public void setColor(Color co)
        {
            col = co;
        }

        public void setTexture(Texture2D text)
        {
            texture = text;
        }

        public void setAlpha(float al)
        {
            alpha = al;
        }

        public void Update()
        {
            
            
            if (frameControl == frameDelay)
            {
                frameControl = 0;
                if (timePerFrame != null)
                {
                    if (timeControl == 0)
                    {
                        currentFrame += direction;

                        if (currentFrame == totalFrames && !(reverse))
                            currentFrame = 0;
                        if (reverse && (currentFrame == totalFrames - 1 || currentFrame == 0))
                            direction = -direction;

                        if (timeIterator < rows * columns - 1) timeIterator++;
                        else timeIterator = 0;
                        timeControl = timePerFrame[timeIterator];

                    }
                    else
                    {
                        timeControl--;
                    }
                }
                else
                {
                    currentFrame += direction;

                    if (currentFrame == totalFrames && !(reverse))
                        currentFrame = 0;
                    if (reverse && (currentFrame == totalFrames - 1 || currentFrame == 0))
                        direction = -direction;
                }


            }
            frameControl++;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location)
        {

            Update();
            int width = texture.Width / columns;
            int height = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();
            
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha);
            spriteBatch.End();
        }

        //origin will change to center
        public void Draw(SpriteBatch spriteBatch, Vector2 location, int width, int height)
        {

            Update();
            int wid = texture.Width / columns;
            int hei = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(wid * column, hei * row, wid, hei);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha,0,new Vector2(wid/(float)2.0,hei/(float)2.0),SpriteEffects.None,0);
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, float angle)
        {

            Update();
            int width = texture.Width / columns;
            int height = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();

            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha, angle, new Vector2(width/2,height/2), SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, bool isReflected)
        {
            Update();
            int width = texture.Width / columns;
            int height = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();
            if (isReflected)
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
            else
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha);
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, bool isReflected, Color col)
        {
            Update();
            int width = texture.Width / columns;
            int height = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();
            if (isReflected)
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
            else
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col*alpha);
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location, bool isReflected, Color col, double al)
        {
            Update();
            int width = texture.Width / columns;
            int height = texture.Height / rows;
            int row = (int)((float)currentFrame / (float)columns);
            int column = currentFrame % columns;

            alpha = (float)al;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();
            if (isReflected)
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col * alpha, 0, new Vector2(0, 0), SpriteEffects.FlipHorizontally, 0);
            else
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, col * alpha);
            spriteBatch.End();
        }
    }
}
