namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides an implementation of the <see cref="IOrder"/> interface, handling Order data persistence
/// using XML serialization (<see cref="XMLTools.LoadListFromXMLSerializer{T}(string)"/>).
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Adds a new Order entity to the XML file.
    /// A unique ID is automatically generated and assigned to the new order.
    /// </summary>
    /// <param name="item">The Order object to add (its ID is ignored and overwritten).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Order item)
    {
        try
        {
            // Load the current list of orders
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

            // Get the next available ID and auto-increment it in the config file
            int id = Config.NextOrderId;

            // Create a copy of the item with the new ID assigned using C# 9.0 record 'with' syntax
            Order copy = item with { Id = id };
            Orders.Add(copy);

            // Save the updated list back to the XML file
            XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to creat new order: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes an Order entity by its unique ID from the XML file.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no order with the specified ID exists.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.
    /// Note: The original implementation saves to <c>Config.s_deliverys_xml</c> which is likely an error and should be <c>Config.s_orders_xml</c>.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        try
        {
            // Load the current list
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

            // Remove the order by ID and check if any were removed
            if (Orders.RemoveAll(it => it.Id == id) == 0)
                throw new DalDoesNotExistException($"Order with ID={id} does Not exist");

            
            XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete exist order: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes all Order entities by saving an empty list to the XML file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if saving the empty list fails.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        try
        {
            // Save an empty list, effectively deleting all orders
            XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete all the orders: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns an Order entity by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the order to read.</param>
    /// <returns>The Order object if found, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(int id)
    {
        try
        {
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
            // Use List<T>.Find() for efficient search
            Order? order = Orders.Find(it => it.Id == id);
            return order;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read order: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns the first Order entity that satisfies the given predicate filter.
    /// </summary>
    /// <param name="filter">The predicate function to filter orders.</param>
    /// <returns>The first Order object matching the filter, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(Func<Order, bool> filter)
    {
        try
        {
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
            // Use List<T>.Find() which accepts a predicate
            Order? order = Orders.Find(it => filter(it));
            return order;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read order: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns a list of all Order entities, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">An optional predicate function to filter the orders. If null, all orders are returned.</param>
    /// <returns>An enumerable collection of matching Order objects.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        try
        {
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
            if (filter != null)
                // Use FindAll which accepts a predicate and returns a new List<T>
                Orders = Orders.FindAll(it => filter(it)).ToList();
            return Orders;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read all the orders: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing Order entity in the XML file.
    /// </summary>
    /// <param name="item">The Order object containing the updated data. The ID must match an existing order.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no order with the specified ID exists to update.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Order item)
    {
        try
        {
            List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

            // Remove the old order by ID and check if it was found
            if (Orders.RemoveAll(it => it.Id == item.Id) == 0)
                throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");

            // Add the new (updated) order object
            Orders.Add(item);

            // Save the updated list
            XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to update exist order: {ex.Message}");
        }
    }
}