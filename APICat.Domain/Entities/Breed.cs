using APICat.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICat.Domain.Entities
{
    public class Breed : EntityBase<Guid>
    {
        public string Name { get; set; }
        public string Origin { get; set; }
        public string Description { get; set; }
        public string Temperament { get; set; }
        public string Wikipedia_url { get; set; }
    }
}
