namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Defines the implementation of the <see cref="ICourier"/> interface for managing courier data in the data source.
/// </summary>
/// <remarks>
/// This class provides CRUD (Create, Read, Update, Delete) operations for the Courier entity,
/// storing data in memory using the <see cref="DataSource"/>.
/// </remarks>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Creates a new courier and adds it to the data source.
    /// </summary>
    /// <param name="item">The courier to add.</param>
    /// <exception cref="Exception">Thrown if a courier with the same ID already exists.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Courier item)
    {
        int id = item.Id;
        Courier? existing = Read(id);
        if (existing != null)
        {
            throw new DalAlreadyExistsException($"There is a Courier with ID={id} already exists.");
        }
        else
        {
            DataSource.Couriers.Add(item);
        }
    }

    /// <summary>
    /// Deletes a courier from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="Exception">Thrown if the courier does not exist.</exception>
    
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        Courier? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($"Courier with ID={id} doesn't exist.");
        }
        else
        {
            DataSource.Couriers.Remove(existing);
        }
    }

    /// <summary>
    /// Deletes all couriers from the data source.
    /// </summary>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    /// <summary>
    /// Reads and returns a courier by its ID.
    /// </summary>
    /// <param name="id">The ID of the courier to read.</param>
    /// <returns>The courier object if found, otherwise <c>null</c>.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(item => item.Id == id);
    }


    /// <summary>
    ///Reads and returns a courier that matches the specified filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(Func<Courier, bool> filter)
    {
        return DataSource.Couriers.FirstOrDefault(item => filter(item));
    }


    /// <summary>
    /// Reads and returns a list of all couriers in the data source.
    /// </summary>
    /// <returns>A new list containing all couriers.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
         => filter != null
             ? from item in DataSource.Couriers
               where filter(item)
               select item :
              from item in DataSource.Couriers
              select item;


    /// <summary>
    /// Updates an existing courier with new information.
    /// </summary>
    /// <param name="item">The updated courier object.</param>
    /// <exception cref="Exception">Thrown if the courier does not exist.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Courier item)
    {
        int id = item.Id;
        Courier? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($"Courier with ID={id} doesn't exists.");
        }
        else
        {
            DataSource.Couriers.Remove(existing);
            DataSource.Couriers.Add(item);
        }
    }
}
