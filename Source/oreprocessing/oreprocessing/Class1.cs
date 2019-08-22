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

   

    public class PlaceWorker_MiningNode : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            Thing thing = map.thingGrid.ThingAt(loc, ThingDef.Named("ResourceHole"));
            if (thing == null || thing.Position != loc)
            {
                return "Must be placed on resource hole";
            }
            return true;
        }

    }

    public class DryingSpot : Building
    {
        private CompDryingSpot DryingSpotComp;
        private int dryingTicks;
        private int TargetTicks
        {
            get
            {
                return this.DryingSpotComp.Props.dryingTicks;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.dryingTicks, "dryingTicks", 0, false);
        }
        public override void SpawnSetup(Map currentGame, bool respawningAfterLoad)
        {
            base.SpawnSetup(currentGame, respawningAfterLoad);
            this.DryingSpotComp = base.GetComp<CompDryingSpot>();
        }
        public override void TickRare()
        {
            base.TickRare();
            if (this.dryingTicks < this.TargetTicks)
            {
                this.dryingTicks++;
            }
            if (this.dryingTicks >= this.TargetTicks)
            {
                this.PlaceProduct();
            }
        }
        private void PlaceProduct()
        {
            IntVec3 position = base.Position;
            Map map = base.Map;
            Thing product = ThingMaker.MakeThing(ThingDef.Named(this.DryingSpotComp.Props.efekt), null);
            product.stackCount = 9;
            GenPlace.TryPlaceThing(product, position, map, ThingPlaceMode.Near, null);
            GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDef.Named("WoodLog"), null), position, map, ThingPlaceMode.Near, null);
            if (!this.Destroyed)
                this.Destroy();


        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine("Drying progress is" + " " + ((float)this.dryingTicks / (float)this.TargetTicks * 100f).ToString("#0.00") + "%");
            return stringBuilder.ToString().TrimEndNewlines();
        }



    }



    public class CompDryingSpot : ThingComp
    {
        public CompProperties_DryingSpot Props
        {
            get
            {
                return (CompProperties_DryingSpot)this.props;
            }
        }
    }

    public class CompProperties_DryingSpot : CompProperties
    {
        public string efekt;
        public int dryingTicks;
        public CompProperties_DryingSpot()
        {
            this.compClass = typeof(CompDryingSpot);
        }
    }







}