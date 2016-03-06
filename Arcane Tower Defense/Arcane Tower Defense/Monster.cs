using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Arcane_Tower_Defense
{
    public class Monster: IComparable
    {
        public String name;
        public int maxHP, HP, gold, lvl, def,spawnSpeed;
        public float spd, distTraveled;
        public Vector2 loc;

        
        public AnimatedSprite sprite;

        //monsters are always drawn 20 left and 20 above their actual location

        public Monster(String nam, int level)
        {
            lvl = level;
            name = nam;
            distTraveled = 0;
            makeMonster(name);
            HP = maxHP;
        }

        //destination based on actual pixel location
        public void MoveToward(Vector2 dest)
        {
            //make unit vector, then multiply by speed constant
            Vector2 vec = new Vector2(dest.X - loc.X, dest.Y - loc.Y);
            float length =(float)( Math.Sqrt(Math.Pow(vec.X,2) + Math.Pow(vec.Y,2)));
            vec.X /= length;
            vec.Y /= length;
            loc.X += vec.X * spd / (float) 20.0;
            loc.Y += vec.Y * spd / (float)20.0;
            distTraveled += spd / (float)20.0;
            //loc = new Vector2(drawLoc.X + 20, drawLoc.Y + 20);
        }

        private void makeMonster(String type)
        {
            switch (type)
            {
                case ("Slime"):
                    {
                        maxHP = 50;
                        spd = RNG.RandInt(9,11);
                        gold = 3;
                        def = 1;
                        sprite = new AnimatedSprite("Slime", 1, 10, 8, true);
                        sprite.setColor(Color.LightGreen);
                        spawnSpeed = RNG.RandInt(80, 100);
                    }
                    break;
                case ("Red Slime"):
                    {
                        maxHP = 60;
                        spd = RNG.RandInt(8,9);
                        gold = 4;
                        def = 2;
                        sprite = new AnimatedSprite("Slime", 1, 10, 10, true);
                        sprite.setColor(Color.Red);
                        spawnSpeed = RNG.RandInt(90, 110);
                    }
                    break;
                case ("Yellow Slime"):
                    {
                        maxHP = 40;
                        spd = RNG.RandInt(15,17);
                        gold = 4;
                        def = 0;
                        sprite = new AnimatedSprite("Slime", 1, 10, 6, true);
                        sprite.setColor(Color.Yellow);
                        spawnSpeed = RNG.RandInt(20, 30);
                    }
                    break;
                case ("Heart Slime"):
                    {
                        maxHP = 150;
                        spd = RNG.RandInt(7, 8);
                        gold = 5;
                        def = -2;
                        sprite = new AnimatedSprite("Slime", 1, 10, 8, true);
                        sprite.setColor(Color.Pink);
                        spawnSpeed = RNG.RandInt(50, 60);
                    }
                    break;
                case ("Steel Slime"):
                    {
                        maxHP = 30;
                        spd = RNG.RandInt(9, 10);
                        gold = 5;
                        def = 5;
                        sprite = new AnimatedSprite("Slime", 1, 10, 8, true);
                        sprite.setColor(Color.Gray);
                        spawnSpeed = RNG.RandInt(110, 130);
                    }
                    break;
                case ("Group Slime"):
                    {
                        maxHP = 20;
                        spd = 20;
                        gold = RNG.RandInt(0,1);
                        def = -3;
                        sprite = new AnimatedSprite("Slime", 1, 10, 6, true);
                        sprite.setColor(Color.LightBlue);
                        spawnSpeed = RNG.RandInt(2,6);
                    }
                    break;
            }


            maxHP = (int)(maxHP * (1 + lvl / 3.0));
            def += (int)(lvl / 5.0);
            gold = (int)(gold * (1 + lvl / 16.0));
        }

        public int CompareTo(object obj)
        {
            Monster mon = (Monster) obj;
            if (distTraveled > mon.distTraveled) return -1;
            else if (distTraveled < mon.distTraveled) return 1;
            else return 0;
        }

        

        
    }
}
