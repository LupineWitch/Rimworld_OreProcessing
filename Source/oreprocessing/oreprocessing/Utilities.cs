using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;


namespace oreprocessing
{
   public  class Utilities
    {

        public static Texture2D GetIcon()
        {
            Texture2D icon = ContentFinder<Texture2D>.Get("Icons/512", false);
                if (icon == null)
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true);
                    Log.Warning("Ore Processer:: No texture at Icons/512 ");
                }
            
            return icon;
        }

        public static  int RandomNumber(int min, int max)
        {
            System.Random random = new System.Random();
            return random.Next(min, max);
        }


        }
    public class ProportionalWheelSelection
    {
        public static System.Random rnd = new System.Random();

        // Static method for using from anywhere. You can make its overload for accepting not only List, but arrays also: 
        // public static Item SelectItem (Item[] items)...
        public int SelectItem(List<thingMined> things)
        {
            // Calculate the summa of all portions.
            int poolSize = 0;
            for (int i = 0; i < things.Count; i++)
            {
                poolSize += things[i].chance;
            }

            // Get a random integer from 0 to PoolSize.
            int randomNumber = rnd.Next(0, poolSize) + 1;

            // Detect the item, which corresponds to current random number.
            int accumulatedProbability = 0;
            for (int i = 0; i < things.Count; i++)
            {
                accumulatedProbability += things[i].chance;
                if (randomNumber <= accumulatedProbability)
                    return i;
            }
            return 0;    // this code will never come while you use this programm right :)
        }
    }

    }
