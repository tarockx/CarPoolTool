using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarPoolTool.Helpers
{
    public class MathHelper
    {
        public static float ComputeDriveRunsRatio(int totalDrives, int totalRuns)
        {
            return (float)totalDrives / (float)totalRuns;
        }
    }
}