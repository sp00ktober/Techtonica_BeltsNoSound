using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Techtonica_BeltsNoSound.Patches.Transpiler
{
    [HarmonyPatch()]
    internal class MachineInstanceList_Transplier
    {
        delegate void modDelegate(object instance, int index);

        [HarmonyTargetMethod]
        public static MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(MachineInstanceList<,>).MakeGenericType(new Type[]
                {
                    typeof(ConveyorInstance),
                    typeof(ConveyorDefinition)
                }),
                "RunVisualUpdates");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RunVisualUpdates_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MachineInstanceList<ConveyorInstance, ConveyorDefinition>), "updateGpuiFunction")),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ConveyorInstance[]), "myArray")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StreamedMachineData<ConveyorInstance, ConveyorDefinition>[]), "visualsList")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    Transpilers.EmitDelegate<modDelegate>((object instance, int index) =>
                    {
                        if(instance is MachineInstanceList<ConveyorInstance, ConveyorDefinition>)
                        {
                            AccessTools.Field(typeof(MachineVisuals<ConveyorInstance, ConveyorDefinition>), "hasActiveAudio").SetValue(((MachineInstanceList<ConveyorInstance, ConveyorDefinition>)instance).visualsList[index].machineVisuals, false);
                        }
                    }));

            return matcher.InstructionEnumeration();
        }
    }
}
