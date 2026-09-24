using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  internal class WorksFor {
    public int ID { get; set; }
    public int Hours { get; set; }

    //Nav-Prop
    public virtual Employee Employee { get; set; }
    public virtual Project Project { get; set; }

  }
}
