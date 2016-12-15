using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class EvaluatedOperation
    {
        public EvaluatedOperation(OperationTypes operationType, bool result)
        {
            OperationType = operationType;
            Result = result;
        }

        public OperationTypes OperationType;
        public bool Result;
    }
}
