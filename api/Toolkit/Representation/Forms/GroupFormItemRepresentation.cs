﻿using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "group-form-item")]
    public class GroupFormItemRepresentation : FormItemRepresentation
    {
        public GroupFormItemRepresentation()
        {
            Type = FormType.Group;
        }

        [DataMember(Name = "items", Order = 100)]
        public FormItemRepresentation[] Items { get; set; }
    }
}