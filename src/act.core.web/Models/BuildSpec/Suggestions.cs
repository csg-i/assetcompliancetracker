using System.Collections.Generic;
using act.core.data;

namespace act.core.web.Models.BuildSpec
{
    public class Suggestion
    {
        public Suggestion(BuildSpecificationTypeConstant specType, long specId, string specName, string specOwner, string specOwnerSam, JustificationTypeConstant suggestionType, PortTypeConstant? portType, string value, bool clone)
        {
            SpecType = specType;
            SpecId = specId;
            SpecName = specName;
            SpecOwner = specOwner;
            SpecOwnerSam = specOwnerSam;
            SuggestionType = suggestionType;
            PortType = portType;
            Value = value;
            Clone = clone;
        }
        public Suggestion(BuildSpecificationTypeConstant specType, long specId, string specName, string specOwner, string specOwnerSam, JustificationTypeConstant suggestionType, PortTypeConstant? portType, string value, bool clone, BuildSpecReference remove)
        {
            SpecType = specType;
            SpecId = specId;
            SpecName = specName;
            SpecOwner = specOwner;
            SpecOwnerSam = specOwnerSam;
            SuggestionType = suggestionType;
            PortType = portType;
            Value = value;
            Remove = true;
            ToRemove = remove;
            Clone = clone;
        }

        public BuildSpecificationTypeConstant SpecType { get; }
        public long SpecId { get; }
        public string SpecName { get; }
        public string SpecOwner { get; }
        public string SpecOwnerSam { get; }
        public JustificationTypeConstant SuggestionType { get; }
        public PortTypeConstant? PortType { get; }
        public string Value { get; }
        public bool Remove { get; }
        public BuildSpecReference ToRemove { get; }
        public bool Clone { get; }
    }
    public class Suggestions:List<Suggestion>
    {
        public BuildSpecificationTypeConstant Type { get; }
        public long SpecId { get; }
        public string Name { get; }
        public bool IsEmpty => Count == 0;
        public string Text { get; }

        public Suggestions(BuildSpecificationTypeConstant type, long specId, string name,
            IEnumerable<Suggestion> suggestions) : base(suggestions)
        {
            Type = type;
            SpecId = specId;
            Name = name;
        }

        internal static Suggestions Empty => new Suggestions("No Suggestions could be found.");
        internal static Suggestions NodeMismatch => new Suggestions("A node is assigned with a different platform type.");
        
        private Suggestions(string text) : base()
        {
            Text = text;
        }

    }
}