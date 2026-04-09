using DalApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLApi;

public interface IBl
{
    ICourier Courier { get; }
    IOrder Order { get; }
    IAdmin Admin { get; }

}
