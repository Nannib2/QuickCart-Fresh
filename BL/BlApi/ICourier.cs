using BlApi;
using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLApi;
public interface ICourier : IObservable //stage 5
{
    public string EnterSystem(int id, string password);

    /// <summary>
    ///  obtain a list of couriers according to the requested parameters       
    /// </summary>
    /// <param name="id"></param>
    /// <param name="active"></param>
    /// <param name="sortBy"></param>
    /// <param name="filterBy"></param>
    /// <returns></returns>
    public IEnumerable<BO.CourierInList> ReadAll(int requesterId, bool? active, CourierInListProperties? sortBy, CourierInListProperties? filterBy,Object? innerfilterBy);

    public Task< BO.Courier> Read(int requesterId, int courierId);
    public void Update(BO.Courier courier, int requesterId);
    public void Delete(int courierId, int requesterId);
    public void Create(BO.Courier courier, int requesterId);
}
