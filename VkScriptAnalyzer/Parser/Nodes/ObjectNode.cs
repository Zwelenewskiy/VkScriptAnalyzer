﻿namespace VkScriptAnalyzer.Parser
{
    public class ObjectNode : ExprNode
    {
        public System.Collections.Generic.List<ObjectField> Fields { get; set; }

        public ObjectNode(System.Collections.Generic.List<ObjectField> fields) : base(null)
        {
            Fields = fields;
        }
    }
}
