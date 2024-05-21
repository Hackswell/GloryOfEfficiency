namespace GloryOfEfficiency.Configs
{
    class CustomAnimalTool
    {
        public string Name { get; set; }
        public ToolType ToolType { get; set; }

        public CustomAnimalTool(string name, ToolType toolType)
        {
            Name = name;
            ToolType = toolType;
        }
    }

    public enum ToolType
    {
        None,
        Bucket,
        Shears
    }
}
