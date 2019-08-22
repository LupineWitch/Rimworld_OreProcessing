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
    public class JobDriverOreMine : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            LocalTargetInfo targetB = this.pawn;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Deconstruct);
            this.FailOn(delegate ()
            {
                CompMineShaft compMiningPlatform = this.job.targetA.Thing.TryGetComp<CompMineShaft>();
                return !compMiningPlatform.CanMine();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate ()
            {
                Pawn actor = work.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                CompMineShaft comp = building.GetComp<CompMineShaft>();
                if (comp.ProspectMode)
                {
                    comp.Prospecting(actor);
                }
                else
                {
                    comp.MiningWorkDone(actor);
                }
                actor.skills.Learn(SkillDefOf.Mining, 0.065f, false);
            };
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);
            work.WithEffect(EffecterDefOf.Mine, TargetIndex.B);
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.activeSkill = (() => SkillDefOf.Mining);
            yield return work;
            yield break;
        }
    }
}
