using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubv.common.serialization
{
    public class IntList : serialization.types.List<serialization.types.Int32>
    {
        public IntList(List<serialization.types.Int32> intList) : base(intList)
        { }

        protected override ID.BYTE_TYPE SerializationID()
        {
            return ID.BYTE_TYPE.LIST_INT32;
        }
    }
}
