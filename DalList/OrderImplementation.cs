namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides in-memory CRUD operations (Create, Read, Update, Delete)
/// for <see cref="Order"/> entities in the <see cref="DataSource"/>.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IOrder"/> interface and handles 
/// all data access operations related to orders.
/// It works directly with the static in-memory data source.
/// </remarks>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new <see cref="Order"/> and adds it to the data source.
    /// </summary>
    /// <param name="item">
    /// A reference to an existing <see cref="Order"/> object whose properties 
    /// are already populated with valid values, except for the ID.
    /// </param>
    /// <remarks>
    /// A new unique ID is generated using <see cref="Config.NextOrderId"/>, 
    /// and a copy of the provided order is created with that ID.
    /// The copy is then added to the <see cref="DataSource.Orders"/> collection.
    /// </remarks>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order copy = item with { Id = id };
        DataSource.Orders.Add(copy);
    }

    /// <summary>
    /// Deletes an existing <see cref="Order"/> from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <exception cref="Exception">
    /// Thrown when no order with the specified ID exists in the data source.
    /// </exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        Order? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($" Order with ID={id} doesn't  exist.");
        }
        else
        {
            DataSource.Orders.Remove(existing);
        }
    }

    /// <summary>
    /// Deletes all <see cref="Order"/> objects from the data source.
    /// </summary>
    /// <remarks>
    /// This operation clears the entire orders collection permanently.
    /// </remarks>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// Reads and returns a single <see cref="Order"/> by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>
    /// The <see cref="Order"/> object with the specified ID, 
    /// or <c>null</c> if no such order exists.
    /// </returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(int id)
    {
        return DataSource.Orders.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// Reads and returns a single <see cref="Order"/>
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(Func<Order, bool> filter)
    {
        return DataSource.Orders.FirstOrDefault(item => filter(item));
    }

    /// <summary>
    /// Returns a new list containing all <see cref="Order"/> objects 
    /// currently stored in the data source.
    /// </summary>
    /// <returns>
    /// A new <see cref="List{Order}"/> containing all existing orders.
    /// </returns>
    /// <remarks>
    /// The returned list is a copy; modifying it does not affect the original data.
    /// </remarks>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
         => filter != null
             ? from item in DataSource.Orders
               where filter(item)
               select item :
              from item in DataSource.Orders
              select item;

    /// <summary>
    /// Updates an existing <see cref="Order"/> in the data source.
    /// </summary>
    /// <param name="item">
    /// The <see cref="Order"/> object containing updated values. 
    /// Its ID must match an existing order in the data source.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when no order with the specified ID exists in the data source.
    /// </exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Order item)
    {
        int id = item.Id;
        Order? existing = Read(id);
        if (existing == null)
        {
            throw new DalDoesNotExistException($" Order with ID={id} isn't  existing.");
        }
        else
        {
            DataSource.Orders.Remove(existing);
            DataSource.Orders.Add(item);
        }
    }
}
