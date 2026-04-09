namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

/// <summary>
/// Provides an implementation of the <see cref="ICourier"/> interface, handling Courier data persistence
/// by reading from and writing to the XML data file defined in <see cref="Config.s_couriers_xml"/>.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Converts an <see cref="XElement"/> representing a Courier from the XML file into a <see cref="Courier"/> object.
    /// </summary>
    /// <param name="s">The XElement to convert.</param>
    /// <returns>A fully populated <see cref="Courier"/> object.</returns>
    /// <exception cref="FormatException">Thrown if any element required for the Courier object is missing or cannot be parsed correctly (e.g., ID, dates, enum).</exception>
    private static Courier getCourier(XElement s)
    {
        try
        {
            int id= s.ToIntNullable("Id") ?? throw new FormatException("can't convert id");
            DateTime employmentStartDateTime = s.ToDateTimeNullable("EmploymentStartDateTime") ?? throw new FormatException("can't convert EmploymentStartDateTime");
            string nameCourier = (string?)s.Element("NameCourier") ?? throw new FormatException("can't convert NameCourier");
            string phoneNumber = (string?)s.Element("PhoneNumber") ?? throw new FormatException("can't convert PhoneNumber");
            string emailCourier = (string?)s.Element("EmailCourier") ?? throw new FormatException("can't convert EmailCourier");
            string passwordCourier = (string?)s.Element("PasswordCourier") ?? throw new FormatException("can't convert PasswordCourier");
            bool active = bool.TryParse(s.Element("IsActive")?.Value, out var b) ? b : true;
            double? personalMaxAirDistance = s.ToDoubleNullable("PersonalMaxAirDistance");
            DeliveryTypeMethods courierDeliveryType = s.ToEnumNullable<DeliveryTypeMethods>("CourierDeliveryType") ?? throw new FormatException("can't convert CourierDeliveryType");


            return new Courier
        (
            id,
            employmentStartDateTime,
            nameCourier,
            phoneNumber,
            emailCourier,
            passwordCourier,
            active,
            personalMaxAirDistance,
            courierDeliveryType
        );
        }

        catch (FormatException ex)
        {
            throw new FormatException($"Failed to parse Courier element: {ex.Message}");

        }
    }

    /// <summary>
    /// Converts a <see cref="Courier"/> object into an <see cref="XElement"/> for saving to the XML file.
    /// </summary>
    /// <param name="c">The Courier object to convert.</param>
    /// <returns>An XElement representing the Courier object.</returns>
    private static XElement setCourier(Courier c)
    {
        return new XElement("Courier",
            new XElement("Id", c.Id),
            new XElement("EmploymentStartDateTime", c.EmploymentStartDateTime),
            new XElement("NameCourier", c.NameCourier),
            new XElement("PhoneNumber", c.PhoneNumber),
            new XElement("EmailCourier", c.EmailCourier),
            new XElement("PasswordCourier", c.PasswordCourier),
            new XElement("IsActive", c.Active),
            new XElement("PersonalMaxAirDistance", c.PersonalMaxAirDistance),
            new XElement("CourierDeliveryType", c.CourierDeliveryType.ToString())
        );
    }

    /// <summary>
    /// Adds a new Courier entity to the list of couriers in the XML file.
    /// </summary>
    /// <param name="item">The Courier object to add.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if a courier with the same ID already exists.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Courier item)
    {
        try
        {
            XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

            if (couriersRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id) != null)
            {
                throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");
            }
            // Adds the new courier element to the root
            couriersRootElem.Add(new XElement(setCourier(item)));

            XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to create a courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a Courier entity by its unique ID from the XML file.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no courier with the specified ID exists.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        try
        {
            XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

            // Find the element and remove it, or throw an exception if not found.
            (couriersRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == id)
            ?? throw new DalDoesNotExistException($"Courier with ID={id} does Not exist"))
                        .Remove();

            XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete a courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes all Courier entities from the XML file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        try
        {
            XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
            // Removes all child elements from the root, effectively emptying the list.
            couriersRootElem.RemoveAll();
            XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to delete a courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads and returns a Courier entity by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the courier to read.</param>
    /// <returns>The Courier object if found, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or there is a parsing error.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(int id)
    {
        try
        {
            XElement? courierElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
            return courierElem is null ? null : getCourier(courierElem);
        }
        catch (FormatException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read courier: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read exist courier: {ex.Message}");
        }


    }

    /// <summary>
    /// Reads and returns the first Courier entity that satisfies the given predicate filter.
    /// </summary>
    /// <param name="filter">The predicate function to filter couriers.</param>
    /// <returns>The first Courier object matching the filter, otherwise <see langword="null"/>.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or there is a parsing error.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(Func<Courier, bool> filter)
    {
        try
        {
            // Loads all couriers, converts them to DO objects, and applies the filter to find the first match.
            return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().Select(s => getCourier(s)).FirstOrDefault(filter);
        }

        catch (FormatException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read courier: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read exist courier: {ex.Message}");
        }

    }

    /// <summary>
    /// Reads and returns a list of all Courier entities, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">An optional predicate function to filter the couriers. If null, all couriers are returned.</param>
    /// <returns>An enumerable collection of matching Courier objects.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or there is a parsing error.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        try
        {
            // Loads all courier elements, converts them, and applies the filter if provided.
            return filter is null ?
                XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().Select(s => getCourier(s)) :
                XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().Select(s => getCourier(s)).Where(filter);
        }
        catch (FormatException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read courier: {ex.Message}");
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read all or selction couriers: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing Courier entity in the XML file.
    /// </summary>
    /// <param name="item">The Courier object containing the updated data. The ID must match an existing courier.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no courier with the specified ID exists to update.</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the XML file cannot be loaded or saved.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Courier item)
    {
        try
        {
            XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

            // Find the existing element, remove it, and throw an exception if not found.
            (couriersRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
            ?? throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist"))
                        .Remove();

            // Add the new updated element to the list.
            couriersRootElem.Add(new XElement(setCourier(item)));

            XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to read all or selction couriers: {ex.Message}");
        }
    }
}