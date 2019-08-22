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


    public class CompMineShaft : ThingComp
    {
        public float depletionLevel=100f;
        public bool ProspectMode = true;
        private float StoneChunkChance;
        private int lastUsedTick = -99999;
        private float workDone;
        private float prospectingDone=0f;
        private float prospectingNeeded = 5000f;


        private int ResourceListSize
        {
            get
            {
                return PropsMine.mineableList.Count;
            }
        }

        private int OreIndex;
        public string defaultTexPath;
        //Job Related Methods

        public void MiningWorkDone(Pawn Miner)
        {

            float statValue = Miner.GetStatValue(StatDefOf.MiningSpeed, true);
            this.workDone += statValue;
            this.StoneChunkChance = Miner.GetStatValue(StatDefOf.MiningYield, true);

            this.lastUsedTick = Find.TickManager.TicksGame;
            if (this.workDone >= this.PropsMine.mineableList[OreIndex].workcapToMine) 
            {
                TryfinishMining();
                this.workDone = 0f;
                this.StoneChunkChance = 0f;
               this.depletionLevel += PropsMine.mineableList[OreIndex].depletionRate;
                if(depletionLevel>=100f)
                {
                    Messages.Message("Mine depleted!",new TargetInfo( parent.InteractionCell, parent.Map, false), MessageTypeDefOf.NegativeEvent);
                }
            }

        }

        public void Prospecting(Pawn Miner)
        {
            float statValue = Miner.GetStatValue(StatDefOf.MiningSpeed, true)+Miner.GetStatValue(StatDefOf.ResearchSpeed,true);
            this.prospectingDone += statValue;
            this.lastUsedTick = Find.TickManager.TicksGame;
            if (this.prospectingDone >= prospectingNeeded)
            {
                TryFinishProspecting();
                this.prospectingDone = 0f;
                workDone = 0f;
            }

        }

        private  float percentProgress
        {
            get
            {
                return this.workDone / PropsMine.mineableList[OreIndex].workcapToMine;
            }
        }

        private void TryfinishMining()
        {
          
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.Filth_RubbleRock), this.parent.InteractionCell.RandomAdjacentCell8Way(), this.parent.Map, ThingPlaceMode.Near);
            ThingDef thingDef = PropsMine.mineableList[OreIndex].thingDef;
            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = PropsMine.mineableList[OreIndex].yield;
            //Roll if chunk is to be mined
            System.Random rand = new System.Random();
          double  k = rand.NextDouble()+0.30d;
            if(k>this.StoneChunkChance)
            {
                
                List<string> list = new List<string>() { "Sandstone", "Limestone", "Granite", "Marble", "Slate" };
                
                int d = Rand.Range(1,5)-1;

                Thing chunk = ThingMaker.MakeThing(ThingDef.Named("Chunk"+list[d]));
                GenPlace.TryPlaceThing(chunk, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
            }
            else
                GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, null);
            
           
        }

        private void TryFinishProspecting()
        {
            ProportionalWheelSelection RollOreIndex = new ProportionalWheelSelection();
           OreIndex = RollOreIndex.SelectItem(PropsMine.mineableList);
            Messages.Message(String.Format("New ore was found! It is {0}", PropsMine.mineableList[OreIndex].thingDef.label.ToString()), new TargetInfo( parent.InteractionCell ,parent.Map, false), MessageTypeDefOf.PositiveEvent);
           this.depletionLevel = 0f;
            ProspectMode = false;
            prospectingNeeded += Verse.Rand.Range(0, 1000) * 10;
            
        }

        public CompProperties_MineShaft PropsMine
        {
            get
            {
                return (CompProperties_MineShaft)this.props;
            }
        }
       
        
        //Saving
        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref workDone, "WorkDone", 0f);
            Scribe_Values.Look<float>(ref prospectingDone, "prospectingDone", 0f);
            Scribe_Values.Look<bool>(ref ProspectMode, "ProspectModeone", false);
            Scribe_Values.Look<int>(ref lastUsedTick, "lastusedTick",-99999);
            Scribe_Values.Look<int>(ref OreIndex, "Ore_currentResource", 0);
            Scribe_Values.Look<float>(ref this.depletionLevel, "Ore_depletionLevel", 0f);
            Scribe_Values.Look<float>(ref this.prospectingNeeded,"prospect_needed", 5000f);
        }
       
        
        //Resource Switch methods



        public void Changemode()
        {
            ProspectMode = !ProspectMode;
        }



        //BUTTONS
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if(PropsMine.mineableList.Count>1)
            {
                Command_Action ChangeMode = new Command_Action()
                { 
                    defaultLabel =  "Change working mode to prospecting mode",
                    defaultDesc = "Changes  workmode from mining to prospecting.",
                    activateSound = SoundDef.Named("Click"),
                    icon = Utilities.GetIcon(),
                    action = () => { Changemode(); },
                };
                yield return ChangeMode;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder builder = new StringBuilder();
            float k = this.depletionLevel;
            k = k / 100;
            if(ProspectMode)
            {
                builder.AppendLine("Mine is in prospecting mode");
            }
            else
            {
                builder.AppendLine("Mine is in mining mode");
            }
            builder.AppendFormat("Dug ore is {0}\n", PropsMine.mineableList[OreIndex].thingDef.label);
            builder.Append(k.ToStringPercent());
            builder.AppendLine(" of mine is depleted.");
            builder.Append(this.percentProgress.ToStringPercent());
            builder.AppendLine(" excavated material until ore");
            if(depletionLevel >= 100f)
            {
                builder.AppendLine("Mine is empty, change mode to prospect to find new ore!");
            }
            return builder.ToString().TrimEndNewlines();
        }

        public bool CanMine()
        {
            if (this.depletionLevel < 100f || ProspectMode)
                return true;
            else
            { 

                return false;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            
            this.workDone = 0f;
            this.lastUsedTick = -99999;


        }
        public bool UsedLastTick()
        {
            return this.lastUsedTick >= Find.TickManager.TicksGame - 1;
        }
    }

    public class CompProperties_MineShaft : CompProperties
    {
        
        public List<thingMined> mineableList = new List<thingMined>();

        public CompProperties_MineShaft()
        {
            this.compClass = typeof(CompMineShaft);
        }
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
        }

    }



}
