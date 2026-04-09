namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq; // Added for ToList() in ReadAll (although in the original code it wasn't used)
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides an implementation of the <see cref="IDelivery"/> interface, handling Delivery data persistence
/// using XML serialization (<see cref="XMLTools.LoadListFromXMLSerializer{T}(string)"/>).
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Adds a new Delivery entity to the XML file.
    /// A unique ID is automatically generated and assigned to the new delivery.
    /// </summary>
    /// <param name="item">The Delivery object to add (its ID is ignored and overwritten).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Delivery item)
    {
        try
        {
            // Load the current list of deliveries
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);

            // Get the next available ID and auto-increment it in the config file
            int id = Config.NextDeliveryId;

            // Create a copy of the item with the new ID assigned using C# 9.0 record 'with' syntax
            Delivery copy = item with { Id = id };
            Deliverys.Add(copy);

            // Save the updated list back to the XML file
            XMLTools.SaveListToXMLSerializer(Deliverys, Config.s_deliverys_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to creat new delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a Delivery entity by its unique ID from the XML file.
    /// </summary>
    /// <param name="id">The ID of the delivery to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no delivery with the specified ID exists.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        try
        {
            // Load the current list
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);

            // Remove the delivery by ID and check if any were removed
            if (Deliverys.RemoveAll(it => it.Id == id) == 0)
                throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");

            // Save the updated list
            XMLTools.SaveListToXMLSerializer(Deliverys, Config.s_deliverys_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete exist delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes all Delivery entities by saving an empty list to the XML file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if saving the empty list fails.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        try
        {
            // Save an empty list, effectively deleting all deliveries
            XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliverys_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete all the deliverys: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns a Delivery entity by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the delivery to read.</param>
    /// <returns>The Delivery object if found, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Delivery? Read(int id)
    {
        try
        {
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);
            // Use List<T>.Find() for efficient search
            Delivery? delivery = Deliverys.Find(it => it.Id == id);
            return delivery;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns the first Delivery entity that satisfies the given predicate filter.
    /// </summary>
    /// <param name="filter">The predicate function to filter deliveries.</param>
    /// <returns>The first Delivery object matching the filter, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        try
        {
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);
            // Use List<T>.Find() which accepts a predicate
            Delivery? delivery = Deliverys.Find(it => filter(it));
            return delivery;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns a list of all Delivery entities, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">An optional predicate function to filter the deliveries. If null, all deliveries are returned.</param>
    /// <returns>An enumerable collection of matching Delivery objects.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        try
        {
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);
            if (filter != null)
                // Use FindAll which accepts a predicate and returns a new List<T>
                Deliverys = Deliverys.FindAll(it => filter(it));
            return Deliverys;
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read all the deliverys: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing Delivery entity in the XML file.
    /// </summary>
    /// <param name="item">The Delivery object containing the updated data. The ID must match an existing delivery.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no delivery with the specified ID exists to update.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Delivery item)
    {
        try
        {
            List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);

            // Remove the old delivery by ID and check if it was found
            if (Deliverys.RemoveAll(it => it.Id == item.Id) == 0)
                throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");

            // Add the new (updated) delivery object
            Deliverys.Add(item);

            // Save the updated list
            XMLTools.SaveListToXMLSerializer(Deliverys, Config.s_deliverys_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to update exist delivery: {ex.Message}");
        }
    }
}