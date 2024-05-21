﻿using System.Collections.Generic;
using GloryOfEfficiency.Core;
using StardewValley;
using StardewValley.Tools;

namespace GloryOfEfficiency.Configs
{
    class ConfigCustomAnimalTool
    {

        public List<CustomAnimalTool> CustomAnimalTools { get; set; }= new List<CustomAnimalTool>();

        public static ToolType GetToolType(Tool tool)
        {
            switch (tool)
            {
                case MilkPail _:
                    return ToolType.Bucket;
                case Shears _:
                    return ToolType.Shears;
            }

            return ToolType.None;
        }

        public bool AddCustomTool(string name, Tool tool)
        {
            ToolType type = GetToolType(tool);
            if (type == ToolType.None)
            {
                return false;
            }

            if (CustomAnimalTools.Exists(c => c.Name == name))
            {
                return false;
            }
            CustomAnimalTools.Add(new CustomAnimalTool(name, type));
            InstanceHolder.CustomAnimalTool.Save();
            return true;
        }

        public void RemoveCustomTool(string name)
        {
            if (!CustomAnimalTools.Exists(c => c.Name == name))
            {
                return;
            }

            CustomAnimalTools.RemoveAll(c => c.Name == name);
            InstanceHolder.CustomAnimalTool.Save();
        }

        public bool Contains(string name)
        {
            return CustomAnimalTools.Exists(c => c.Name == name);
        }
    }
}
