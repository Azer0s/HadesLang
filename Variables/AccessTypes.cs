using System.IO;

namespace Variables
{
    public enum AccessTypes
    {
        REACHABLE = 1,
        REACHABLE_ALL = 2,
        CLOSED = 3,
        INVALID
    }

    public class AccessTypeParse
    {
        public static AccessTypes ParseTypes(string type)
        {
            switch (type)
            {
                case "reachable":
                    return AccessTypes.REACHABLE;
                case "reachable_all":
                    return AccessTypes.REACHABLE_ALL;
                case "closed":
                    return AccessTypes.CLOSED;
                default:
                    return AccessTypes.INVALID;
            }
        }
    }
}
