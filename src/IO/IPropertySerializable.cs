using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuVelocity.IO
{
    public interface IPropertySerializable
    {
        public string Serialize();
        public void Deserialize(string context);
    }
}
