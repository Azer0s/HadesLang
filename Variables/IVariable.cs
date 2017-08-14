namespace Variables
{
    public abstract class IVariable
    {
        public AccessTypes Access { get; set; }
        public DataTypes DataType { get; set; }
    }
}
