using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Domain.Entities.Base
{
    public abstract class EntityBase<TId>
    {
        public required TId Id { get; set; }
    }
}
