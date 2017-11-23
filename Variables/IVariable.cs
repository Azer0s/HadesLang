using System;

namespace Variables
{
    public abstract class IVariable
    {
        public AccessTypes Access { get; set; }
        public DataTypes DataType { get; set; }
        public int Order { get; }

        protected IVariable(int order)
        {
            Order = order;
        }
    }
}
