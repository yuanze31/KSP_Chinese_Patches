using HarmonyLib;
using KSP.Localization;
using KSP_Chinese_Patches.PatchesInfo;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KSP_Chinese_Patches.ModPatches
{
    public class SmartStagePatches : AbstractPatchBase
    {
        public override string PatchName => "SmartStage";
        public override string PatchDLLName => "SmartStage";

        private static IEnumerable<CodeInstruction> AscentPlotLocPatch(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher matcher = new CodeMatcher(codeInstructions).Start();
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "acceleration")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_PlotAcceleration"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "surface velocity")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_PlotSurfaceVelocity"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "altitude")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_PlotAltitude"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "throttle")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_PlotThrottle"));
            return matcher.InstructionEnumeration();
        }
        private static IEnumerable<CodeInstruction> MainWindow_OnGUILocPatch(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher matcher = new CodeMatcher(codeInstructions).Start();
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "SmartStage Stages")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_WindowTitle"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "SmartStage Stages")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_WindowTitle"));
            return matcher.InstructionEnumeration();
        }

        private static IEnumerable<CodeInstruction> MainWindow_PlanetDisplayNamePatch(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher matcher = new CodeMatcher(codeInstructions).Start();
            matcher.MatchStartForward(new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(CelestialBody), nameof(CelestialBody.GetName))))
                .SetOperandAndAdvance(AccessTools.Method(typeof(CelestialBody), nameof(CelestialBody.GetDisplayName)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LingoonaGrammarExtensions), nameof(LingoonaGrammarExtensions.LocalizeRemoveGender))));
            return matcher.InstructionEnumeration();
        }
        private static IEnumerable<CodeInstruction> MainWindow_drawStagesWindowLocPatch(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher matcher = new CodeMatcher(codeInstructions).Start();
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Stage: ")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_Stage") + ": ");
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, ", part count: ")).SetOperandAndAdvance(", " + Localizer.Format("#CNPatches_SmartStage_PartCount") + ": ");
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Done")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_Done"));
            return matcher.InstructionEnumeration();
        }

        private static IEnumerable<CodeInstruction> MainWindow_drawWindowLocPatch(IEnumerable<CodeInstruction> codeInstructions)
        {
            CodeMatcher matcher = new CodeMatcher(codeInstructions).Start();
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Compute stages")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_ComputeStages"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Show stages")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_ShowStages"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Automatically recompute staging")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_AutomaticallyRecomputeStaging"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Advanced simulation")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_AdvancedSimulation"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Limit max acceleration to (m/sec): ")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_LimitMaxAcceleration"));
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldstr, "Show icon in flight")).SetOperandAndAdvance(Localizer.Format("#CNPatches_SmartStage_ShowIconInFlight"));
            return matcher.InstructionEnumeration();
        }

        protected override void LoadAllPatchInfo()
        {
            Type SmartStageType = AccessTools.TypeByName("SmartStage.MainWindow");
            Patches = new HashSet<HarPatchInfo>
            {
                new HarPatchInfo(
                    AccessTools.Constructor(
                            AccessTools.TypeByName("SmartStage.AscentPlot"),
                            new[] {
                                typeof(List<>).MakeGenericType(AccessTools.TypeByName("SmartStage.Sample")),
                                typeof(List<>).MakeGenericType(AccessTools.TypeByName("SmartStage.StageDescription")),
                                typeof(int),
                                typeof(int)
                            }),
                    new HarmonyMethod(typeof(SmartStagePatches), nameof(SmartStagePatches.AscentPlotLocPatch)),
                    HarmonyPatchType.Transpiler
                 ),
                new HarPatchInfo(
                    AccessTools.Method(SmartStageType, "OnGUI"),
                    new HarmonyMethod(typeof(SmartStagePatches), nameof(SmartStagePatches.MainWindow_OnGUILocPatch)),
                    HarmonyPatchType.Transpiler
                ),
                new HarPatchInfo(
                    AccessTools.Method(SmartStageType, "drawStagesWindow", new[] { typeof(int) }),
                    new HarmonyMethod(typeof(SmartStagePatches), nameof(SmartStagePatches.MainWindow_drawStagesWindowLocPatch)),
                    HarmonyPatchType.Transpiler
                ),
                new HarPatchInfo(
                    AccessTools.Method(SmartStageType, "drawWindow", new[] { typeof(int) }),
                    new HarmonyMethod(typeof(SmartStagePatches), nameof(SmartStagePatches.MainWindow_drawWindowLocPatch)),
                    HarmonyPatchType.Transpiler
                ),
                new HarPatchInfo(
                    AccessTools.Method(SmartStageType, "<planets>m__0", new[] { typeof(CelestialBody) }),
                    new HarmonyMethod(typeof(SmartStagePatches), nameof(SmartStagePatches.MainWindow_PlanetDisplayNamePatch)),
                    HarmonyPatchType.Transpiler
                )
            };
        }
    }
}
