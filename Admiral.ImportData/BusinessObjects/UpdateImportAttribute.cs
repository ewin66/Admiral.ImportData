using System;

namespace Admiral.ImportData
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class UpdateImportAttribute : Attribute
    {
        public string KeyMember { get; set; }

        public UpdateImportAttribute(string keyMember)
        {
            this.KeyMember = keyMember;
        }
    }
}