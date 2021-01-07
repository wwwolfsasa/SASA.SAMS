using System;

namespace SASA.SAMS.PFD {
    internal class AttributeItemAttribute :Attribute {

        public AttributeItemAttribute(string Description) {
            this.Description = Description;
        }
        /// <summary>
        /// 敘述
        /// </summary>
        public string Description { get; set; }
    }
}