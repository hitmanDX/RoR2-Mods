using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXsRoR2Mods {
    partial class Patches {

        static void RunStart_PostFix(Run __instance) {
            run = __instance;
        }

        static void RunDestroy_PreFix() {
            run = null;
        }

    }
}
