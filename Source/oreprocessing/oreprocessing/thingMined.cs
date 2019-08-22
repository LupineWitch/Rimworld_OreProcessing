using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace oreprocessing
{
    public class thingMined 
    {
        public float depletionRate = 5f;
        public ThingDef thingDef;
        public int yield;
        public int workcapToMine;
        public string graphSuffix = null;
        public int chance = 100;

    }
}
