namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides an implementation of the <see cref="IDelivery"/> interface
/// for managing <see cref="Delivery"/> entities in the in-memory data source.
/// </summary>
/// <remarks>
/// This class handles CRUD (Create, Read, Update, Delete) operations for
/// deliveries within the DAL (Data Access Layer). 
/// The data is stored in <see cref="DataSource.Deliverys"/> and is maintained
/// throughout the runtime of the application.
/// </remarks>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new <see cref="Delivery"/> object, assigns it a new unique ID,
    /// and adds it to the data source.
    /// </summary>
    /// <param name="item">The <see cref="Delivery"/> object to add.</param>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Delivery item)
    {
        int id = Config.NextDeliveryId;
        Delivery copy = item with { Id = id };
        DataSource.Deliverys.Add(copy);
    }

    /// <summary>
    /// Deletes the <see cref="Delivery"/> object with the specified ID from the data source.
    /// Throws an exception if the delivery does not exist.
    /// </summary>
    /// <param name="id">The ID of the delivery to delete.</param>
    /// <exception cref="Exception">
    /// Thrown when the delivery with the specified ID does not exist.
    /// </exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        Delivery? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($"Delivery with ID={id} doesn't exist.");
        }
        else
        {
            DataSource.Deliverys.Remove(existing);
        }
    }

    /// <summary>
    /// Deletes all <see cref="Delivery"/> objects from the data source.
    /// </summary>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        DataSource.Deliverys.Clear();
    }

    /// <summary>
    /// Reads and returns the <see cref="Delivery"/> object with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the delivery to read.</param>
    /// <returns>
    /// The <see cref="Delivery"/> object if found; otherwise, <c>null</c>.
    /// </returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Delivery? Read(int id)
    {
        return DataSource.Deliverys.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// Reads and returns the first <see cref="Delivery"/> object
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return DataSource.Deliverys.FirstOrDefault(item => filter(item));
    }

    /// <summary>
    /// Returns a new list containing all <see cref="Delivery"/> objects
    /// from the data source.
    /// </summary>
    /// <returns>A copy of the list of all deliveries.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
         => filter != null
             ? from item in DataSource.Deliverys
               where filter(item)
               select item :
              from item in DataSource.Deliverys
              select item;

    /// <summary>
    /// Updates an existing <see cref="Delivery"/> object in the data source.
    /// Throws an exception if the delivery does not exist.
    /// </summary>
    /// <param name="item">The updated <see cref="Delivery"/> object.</param>
    /// <exception cref="Exception">
    /// Thrown when the delivery with the specified ID does not exist.
    /// </exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Delivery item)
    {
        int id = item.Id;
        Delivery? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($"Delivery with ID={id} doesn't exist.");
        }
        else
        {
            DataSource.Deliverys.Remove(existing);
            DataSource.Deliverys.Add(item);
        }
    }
}
