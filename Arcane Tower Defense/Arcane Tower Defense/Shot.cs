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
    public class Shot
    {
        public Vector2 loc, target;
        public double range, distTraveled;
        public int hits, dmg, spd, lifetime;
        public AnimatedSprite shotTex, hitTex;
        public List<Monster> hitMons;
        //public bool hasHit;
        public Vector2 direction;
        public Color hitColor;
        public Tower shotTow;
        public Monster monTarget;
        public String name;
        public double lastHitBoost;

        

        public Shot(Tower tow, Vector2 start, Vector2 tar)
        {
            //hasHit = false;
            target = tar;
            distTraveled = 0;
            shotTow = tow;
            name = tow.curShot.name;

            loc = start;
            hits = tow.curShot.shotHits;
            dmg = RNG.RandInt((int)Math.Round(4.0 / 5.0 * tow.curShot.shotDmg), (int)Math.Round(6.0 / 5.0 * tow.curShot.shotDmg)) + RNG.RandInt(-1, 1);
            spd = tow.curShot.shotSpd;
            lifetime = tow.curShot.shotLife;
            hitMons = new List<Monster>();

            lastHitBoost = tow.curShot.shotLastHitBoost;

            direction = (target - loc);
            direction.Normalize();
            switch (tow.type)
            {
                case ("Fire"):
                    {
                        switch (tow.curShotNum)
                        {
                            case (1):
                                {
                                    range = 10;
                                    shotTex = new AnimatedSprite("FireShot1", 1, 2, 3);
                                    hitTex = new AnimatedSprite("FireHit1", 1, 5, 5);
                                    hitColor = Color.Red;
                                }
                                break;
                            case (2):
                                {
                                    range = 20;
                                    shotTex = new AnimatedSprite("FireShot2", 1, 1, 1);
                                    hitTex = new AnimatedSprite("FireShot2", 1, 1, 1);
                                    hitColor = Color.Orange;
                                }
                                break;
                        }
                    }
                    break;
                case ("Ice"):
                    {
                        range = 8;
                        shotTex = new AnimatedSprite("IceShot1", 1, 1, 1);
                        hitTex = new AnimatedSprite("IceHit1", 1, 5, 3);
                        hitColor = Color.Blue;
                    }
                    break;
                case ("Earth"):
                    {
                        range = 1;
                        shotTex = new AnimatedSprite("EarthShot1", 1, 1, 1);
                        hitTex = new AnimatedSprite("Hit", 1, 4, 4);
                        hitColor = Color.Brown;
                        distTraveled = 1;
                    }
                    break;
                case ("Thunder"):
                    {
                        switch (tow.curShotNum)
                        {
                            case (1):
                                {
                                    range = 10;
                                    shotTex = new AnimatedSprite("Hit", 1, 4, 4);
                                    hitTex = new AnimatedSprite("ThunderHit1", 1, 10, 4);
                                    hitColor = Color.Cyan;
                                }
                                break;
                            case (2):
                                {
                                    range = 13;
                                    shotTex = new AnimatedSprite("ThunderShot2", 1, 7, 5);

                                    hitTex = new AnimatedSprite("ThunderHit2", 1, 6, 5);
                                    hitColor = Color.Yellow;
                                }
                                break;
                        }

                    }
                    break;
            }



        }

        public Shot(Shot copy) : this(copy.shotTow, copy.shotTow.loc, copy.target)
        {
            
        }

        public void MoveToward(Vector2 dest)
        {
            //make unit vector, then multiply by speed constant
            Vector2 vec = new Vector2(dest.X - loc.X, dest.Y - loc.Y);
            float length = (float)(Math.Sqrt(Math.Pow(vec.X, 2) + Math.Pow(vec.Y, 2)));
            vec.X /= length;
            vec.Y /= length;
            loc.X += vec.X * spd / (float)20.0;
            loc.Y += vec.Y * spd / (float)20.0;
            distTraveled += spd / (float)20.0;
            //loc = new Vector2(drawLoc.X + 20, drawLoc.Y + 20);
        }
        public void SetMonTarget(Monster mon)
        {
            monTarget = mon;
        }
        public void Update()
        {
            if (monTarget != null) target = monTarget.loc;
            if (shotTow.curShot.name.Equals("Flamethrower"))
            {
                spd = (int)Math.Round(spd * (1.0 - Math.Pow(distTraveled / (float)(lifetime + 50), 8)));
                loc = loc + new Vector2(RNG.RandInt(-1, 1), RNG.RandInt(-1, 1));
            }
            
            
            if (shotTow.curShot.name.Equals("Seismic Quake"))
            {
                spd = 2 + (int)Math.Round(Math.Pow(1 - (range / (float)lifetime), 1) * shotTow.towShot1.shotSpd);
                range += (spd / (float)20.0);
                distTraveled += spd / (float)20.0;
                //range += 1+(Math.Pow(1-(range/(float)lifetime),4) * spd / (float)20.0);
                //distTraveled += 1+(Math.Pow(1-(range/(float)lifetime),4) * spd / (float)20.0);
            }
            else if (shotTow.curShot.name.Equals("Static Tracer"))
            {

                double constant = .0000025;

                if (monTarget.HP > 0)
                {
                    if ((target - loc).Length() <= 18)
                    {
                        constant = .00025;
                        hitMons.Clear();
                        dmg = (int)(dmg * lastHitBoost);
                        range = 20;
                    }
                }
                else
                {
                    lifetime -= (int)((lifetime - distTraveled) / 12.0);
                }
                
                //hitMons.Clear();
                for (int i = 0; i < hitMons.Count; i++)
                {
                    Monster mon = hitMons[i];
                    Dictionary<Monster,int> monWaits = (Dictionary<Monster, int>)(shotTow.curShot.param[0]);
                    if (monWaits.ContainsKey(mon))
                    {
                        if (monWaits[mon] <= 0)
                        {
                            hitMons.Remove(mon);
                            i--;
                        }
                        else monWaits[mon]--;
                    }
                    else monWaits.Add(mon, 10);
                }
                spd += RNG.RandInt(0, RNG.RandInt(0, 1));

                /*if (monTarget.HP > 0)
                {*/
                    Vector2 vec = (target - loc);
                    vec.Normalize();
                    float vecAng = (float)Math.Atan2(vec.Y, vec.X);
                    float dirAng = (float)Math.Atan2(direction.Y, direction.X);
                    if (Math.Abs(vecAng - dirAng) <= (float)(Math.PI * constant * Math.Pow(spd, 2)) || Math.Abs(vecAng - dirAng) >= (float)(Math.PI * 2 - (Math.PI * constant * Math.Pow(spd, 2))))
                        direction = vec;
                    else
                    {

                        if ((dirAng > vecAng && dirAng - vecAng <= Math.PI) || (dirAng < vecAng && vecAng - dirAng >= Math.PI))
                            dirAng -= (float)(Math.PI * constant * Math.Pow(spd, 2));
                        else
                            dirAng += (float)(Math.PI * constant * Math.Pow(spd, 2));
                        direction = new Vector2((float)Math.Cos(dirAng), (float)Math.Sin(dirAng));
                    }

                    direction.Normalize();
                /*}
                else
                {
                    target = loc + 10 * direction;
                }*/


                MoveToward(loc + direction);
            }
            else
            {
                MoveToward(target);

                if (!(direction.Equals(new Vector2(0, 0)))) direction = (target - loc);
                direction.Normalize();
            }
        }
    }
}
