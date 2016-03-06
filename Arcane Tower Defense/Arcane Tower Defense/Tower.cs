using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Arcane_Tower_Defense
{
    public class Tower
    {
        public class Range
        {
            public Color[] img;
            public bool[] rng;
            public int length;

            public Range(String type, int outer, int inner)
            {
                length = 2 * outer + 2;
                img = new Color[length * length];
                rng = new bool[length * length];

                for (int i = 0; i < img.Length; i++)
                {
                    Vector2 vec = new Vector2(length / 2 - i % length, length / 2 - (int)(i / length));
                    if (vec.Length() <= outer && vec.Length() >= inner)
                    {
                        if (Math.Abs(vec.Length() - outer) <= 1.5 || Math.Abs(vec.Length()-inner) <= 1.5) img[i] = Color.LightBlue;
                        else img[i] = new Color(40, 40, 120, 60);
                        rng[i] = true;
                    }

                }
            }

            public Range(String type, int ran)
            {
                length = 2 * ran + 2;
                switch (type)
                {
                    
                    case ("Circle"):
                        {
                            img = new Color[length * length];
                            rng = new bool[length * length];

                            for (int i = 0; i < img.Length; i++)
                            {
                                Vector2 vec = new Vector2(length/2-i%length,length/2-(int)(i/length));
                                if (vec.Length() <= ran)
                                {
                                    if (Math.Abs(vec.Length() - ran) <= 1.5) img[i] = Color.LightBlue;
                                    else img[i] = new Color(40, 40, 120, 60);
                                    rng[i] = true;
                                }

                            }

                            

                        }
                        break;
                    case ("Cross"):
                        {
                            img = new Color[length * length];
                            rng = new bool[length * length];

                            for (int i = 0; i < length; i++)
                            {
                                for (int j = 0; j < length; j++)
                                {
                                    if (Math.Abs(j - length / 2.0) == 20 || Math.Abs(i - length / 2.0) == 20)
                                    {

                                        img[i * length + j] = Color.LightBlue;
                                        rng[i * length + j] = true;
                                    }
                                    if (Math.Abs(j - length / 2.0) < 20 || Math.Abs(i - length / 2.0) < 20)
                                    {
                                        if (j == 0 || i == 0 || j == length-1 || i == length-1)
                                        {
                                            img[i * length + j] = Color.LightBlue;
                                            rng[i * length + j] = true;
                                        }
                                        else
                                        {
                                            img[i * length + j] = new Color(40, 40, 120, 60);
                                            rng[i * length + j] = true;
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(j - length / 2.0) == 20 || Math.Abs(i - length / 2.0) == 20)
                                        {

                                            img[i * length + j] = Color.LightBlue;
                                            rng[i * length + j] = true;
                                        }
                                        else
                                        {
                                            img[i * length + j] = Color.Transparent;
                                            rng[i * length + j] = false;
                                        }
                                    }
                                }

                            }



                        }
                        break;
                }
                

                

            }

        }

        

        public struct ShotTemplate
        {
            public int shotHits, shotDmg, shotLife, shotSpd, cooldown, cooldown2;
            public Range ran;
            public String name;
            //for any extra parameters
            public List<Object> param;
            public double shotLastHitBoost;

            public ShotTemplate(int hits,int dmg,int life,int spd, String nam, int cool, int cool2, Range ra)
            {
                shotHits = hits;
                shotDmg = dmg;
                shotLife = life;
                shotSpd = spd;
                name = nam;
                cooldown = cool;
                cooldown2 = cool2;
                ran = ra;
                param = new List<Object>();
                shotLastHitBoost = 1;
            }
        }

        public int cost;
        public Vector2 loc;
        public String type;
        public int delay, delay2;
        public int count;
        
        //public double att;
        //public int cooldown;

        //public Range curShot.ran;

        public ShotTemplate curShot, towShot1,towShot2;
        public int curShotNum;

        public int curUpgr, upgrCost;
        public String upgrInfo;


        public Tower(String ty)
        {
            type = ty;
            //level = 1;
            delay = 0;
            delay2 = 0;
            //att = 1;
            curUpgr = 1;
            curShotNum = 1;
            count = 0;
            
            switch (type)
            {
                case ("Fire"):
                    {
                        cost = 25;     
                        towShot1 = new ShotTemplate(3, 9, 650, 200, "Lava Arrow",50,0,new Range("Cross",100));
                        upgrCost = 20;
                        upgrInfo = "Increases Lava Arrow's number of hits by 2";

                    }
                    break;
                case ("Ice"):
                    {
                        cost = 25;
                        towShot1 = new ShotTemplate(1, 3, 110, 100, "Rapid-Fire Icicles",8,0,new Range("Circle",100));
                        towShot1.shotHits = 1;
                        upgrCost = 25;
                        upgrInfo = "Increases damage per shot of Rapid-Fire Icicles by 1";

                    }
                    break;
                case ("Earth"):
                    {
                        cost = 25;
                        towShot1 = new ShotTemplate(50, 7, 60, 150, "Seismic Quake",75,0,new Range("Circle",60));
                        upgrCost = 15;
                        upgrInfo = "Reduces curShot.cooldown of Seismic Quake by 13%";
                        
                    }
                    break;
                case ("Thunder"):
                    {
                        cost = 25;
                        towShot1 = new ShotTemplate(1, 18, 50, 5, "Energy Bolt", 200,0, new Range("Ring",140,100));
                        upgrCost = 20;
                        upgrInfo = "Increases damage of Energy Bolt by 5";
                    }
                    break;
            }
            curShot = towShot1;
            upgrInfo = upgrInfo + "\nCost: " + upgrCost;
        }

        public void SetShot(int shotNum)
        {
            curShotNum = shotNum;
            if (shotNum == 1) curShot = towShot1;
            else curShot = towShot2;
        }
        //deduct cost first
        public void Upgrade()
        {
            cost += upgrCost;
            switch (type)
            {
                case ("Fire"):
                    {
                        switch (curUpgr)
                        {
                            case (1):
                                {
                                    towShot1.shotHits += 2;
                                    upgrCost = 30;
                                    upgrInfo = "Raises number of Lava Arrow's hits by 1 and damage by 2";
                                }
                                break;
                            case (2):
                                {
                                    towShot1.shotHits++;
                                    towShot1.shotDmg += 2;
                                    upgrInfo = "Increases range of Lava Arrow by 1 block";
                                    upgrCost = 40;
                                }
                                break;
                            case (3):
                                {
                                    towShot1.ran = new Range("Cross", 140);
                                    upgrInfo = "Unlocks second shot, Flamethrower";
                                    upgrCost = 50;
                                }
                                break;
                            case (4):
                                {
                                    towShot2 = new ShotTemplate(3, 10, 100, 75, "Flamethrower", 150,3, new Range("Cross", 100));
                                    //flamethrower length
                                    towShot2.param.Add(15);
                                    towShot2.param.Add(null);
                                }
                                break;
                        }
                    }
                    break;
                case ("Ice"):
                    {
                        switch (curUpgr)
                        {
                            case (1):
                                {
                                    towShot1.shotDmg++;
                                    upgrCost = 30;
                                    upgrInfo = "Reduces cooldown of Rapid-Fire Icicles by 25%";
                                }
                                break;
                            case (2):
                                {
                                    curShot.cooldown = 6;
                                    upgrCost = 35;
                                    upgrInfo = "Increases range of Rapid-Fire Icicles by 1 block";
                                }
                                break;
                            case (3):
                                {
                                    towShot1.ran = new Range("Circle", 140);
                                    towShot1.shotLife = 150;
                                }
                                break;
                        }
                    }
                    break;
                case ("Earth"):
                    {
                        switch (curUpgr)
                        {
                            case (1):
                                {
                                    towShot1.cooldown = 65;
                                    upgrCost = 25;
                                    upgrInfo = "Increases damage of Seismic Quake by 3";
                                }
                                break;
                            case (2):
                                {
                                    towShot1.shotDmg += 3;
                                    upgrCost = 50;
                                    upgrInfo = "Increases damage of Seismic Quake by 1\nIncreases range of Seismic Quake by half a block \nReduces cooldown of Seismic Quake by 10"; 
                                }
                                break;
                            case (3):
                                {
                                    towShot1.shotDmg++;
                                    towShot1.cooldown = 55;
                                    towShot1.ran = new Range("Circle", 80);
                                    towShot1.shotLife = 80;
                                }
                                break;

                        }
                    }
                    break;
                case ("Thunder"):
                    {
                        switch (curUpgr)
                        {
                            case (1):
                                {
                                    towShot1.shotDmg += 5;
                                    upgrCost = 40;
                                    upgrInfo = "Increase range of Energy Bolt by 1 block";
                                }
                                break;
                            case (2):
                                {
                                    towShot1.ran = new Range("Ring", 180, 100);
                                    upgrCost = 30;
                                    upgrInfo = "Reduce cooldown of Energy Bolt by 10%";
                                }
                                break;
                            case (3):
                                {
                                    towShot1.cooldown = 180;
                                    upgrCost = 50;
                                    upgrInfo = "Unlocks second shot, Static Tracer";
                                }
                                break;
                            case (4):
                                {
                                    towShot2 = new ShotTemplate(40, 15, 5000, 0, "Static Tracer", 300, 0, new Range("Ring", 140, 100));
                                    towShot2.shotLastHitBoost = 4;
                                    //amount of time before you can hit a certain monster again
                                    towShot2.param.Add(new Dictionary<Monster, int>());
                                }
                                break;
                        }
                    }
                    break;
            }
            upgrInfo = upgrInfo + "\nCost: " + upgrCost;
            curUpgr++;
            if (curShotNum == 1) curShot = towShot1;
            else curShot = towShot2;
        }

        public void setLoc(Vector2 vec)
        {
            loc = vec;
        }

        public bool RngContains(Vector2 vec)
        {
            double x = vec.X - (loc.X-curShot.ran.length/2.0);
            double y = vec.Y - (loc.Y - curShot.ran.length/2.0);
            if (x < 0 || y < 0 || x >= curShot.ran.length || y >= curShot.ran.length) return false;
            else return (curShot.ran.rng[((int)y * curShot.ran.length) + (int)x]);
        }

    }
}
