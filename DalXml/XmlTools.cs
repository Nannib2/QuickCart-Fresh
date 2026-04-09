namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    /// <summary>
    /// Saves a list of objects of type T to an XML file using XmlSerializer.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list, must be a class.</typeparam>
    /// <param name="list">The list of objects to save.</param>
    /// <param name="xmlFileName">The name of the XML file (without the path).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the file creation or serialization fails.</exception>
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;//create the full path

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);//create the file
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);//serialize the list to the file-"write"
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    /// <summary>
    /// Loads a list of objects of type T from an XML file using XmlSerializer.
    /// </summary>
    /// <typeparam name="T">The type of objects to load, must be a class.</typeparam>
    /// <param name="xmlFileName">The name of the XML file (without the path).</param>
    /// <returns>The loaded list of objects, or a new empty list if the file doesn't exist.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the file loading or deserialization fails.</exception>
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;//create the full path

        try
        {
            if (!File.Exists(xmlFilePath)) return new();//if the file not exists- return an empty list
            using FileStream file = new(xmlFilePath, FileMode.Open);//open the file
            XmlSerializer x = new(typeof(List<T>));//create the serializer
            return x.Deserialize(file) as List<T> ?? new();//deserialize the list from the file-"read"
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region SaveLoadWithXElement
    /// <summary>
    /// Saves an XElement (root element) to an XML file.
    /// </summary>
    /// <param name="rootElem">The XElement to save.</param>
    /// <param name="xmlFileName">The name of the XML file (without the path).</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the file creation or saving fails.</exception>
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;//create the full path

        try
        {
            rootElem.Save(xmlFilePath);//save the root element to the file
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    /// <summary>
    /// Loads an XElement (root element) from an XML file. If the file does not exist,
    /// a new XElement with the given file name as its name is created, saved, and returned.
    /// </summary>
    /// <param name="xmlFileName">The name of the XML file (without the path, used as the root element name if created).</param>
    /// <returns>The loaded or newly created XElement.</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if the file loading or creation fails.</exception>
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;//create the full path

        try
        {
            if (File.Exists(xmlFilePath))//if the file exists
                return XElement.Load(xmlFilePath);//load the root element from the file
            XElement rootElem = new(xmlFileName);//create the root element
            rootElem.Save(xmlFilePath);//create the file
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region XmlConfig
    /// <summary>
    /// Retrieves an integer value from a configuration XML file, increases it by 1, 
    /// saves the updated value back to the file, and returns the original value.
    /// This is typically used for generating sequential IDs.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the integer value.</param>
    /// <returns>The original integer value before incrementing.</returns>
    /// <exception cref="FormatException">Thrown if the element value cannot be converted to an integer.</exception>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        // Load the XML configuration file
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        // Retrieve the current integer value
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        // Increment the value and save it back to the XML file
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    /// <summary>
    /// Retrieves an integer value from a configuration XML file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the integer value.</param>
    /// <returns>The integer value.</returns>
    /// <exception cref="FormatException">Thrown if the element value cannot be converted to an integer.</exception>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    /// <summary>
    /// Retrieves a DateTime value from a configuration XML file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the DateTime value.</param>
    /// <returns>The DateTime value.</returns>
    /// <exception cref="FormatException">Thrown if the element value cannot be converted to a DateTime.</exception>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }
    /// <summary>
    /// Sets an integer value in a configuration XML file and saves the file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to set.</param>
    /// <param name="elemVal">The integer value to set.</param>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a DateTime value in a configuration XML file and saves the file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to set.</param>
    /// <param name="elemVal">The DateTime value to set.</param>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a nullable double value in a configuration XML file and saves the file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to set.</param>
    /// <param name="elemVal">The nullable double value to set.</param>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a nullable string value in a configuration XML file and saves the file.
    /// If the value is null, it sets an empty string.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to set.</param>
    /// <param name="elemVal">The nullable string value to set.</param>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal ?? string.Empty).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Retrieves a nullable string value from a configuration XML file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the string value.</param>
    /// <returns>The string value, or null if the element is missing.</returns>
    public static string? GetConfigNullableStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return root.Element(elemName)?.Value;
    }
    /// <summary>
    /// Retrieves a non-nullable string value from a configuration XML file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the string value.</param>
    /// <returns>The string value.</returns>
    /// <exception cref="FormatException">Thrown if the element value is missing or null.</exception>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string str = (string)root.Element(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return str;
    }
    /// <summary>
    /// Retrieves a double value from a configuration XML file.
    /// NOTE: The implementation uses ToIntNullable which suggests a possible type mismatch 
    /// if the value is expected to be a double with decimal places.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the double value.</param>
    /// <returns>The double value.</returns>
    /// <exception cref="FormatException">Thrown if the element value cannot be converted to a number.</exception>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToDoubleNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    /// <summary>
    /// Retrieves a nullable double value from a configuration XML file.
    /// NOTE: The implementation uses ToIntNullable which suggests a possible type mismatch 
    /// if the value is expected to be a double with decimal places.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the double value.</param>
    /// <returns>The nullable double value.</returns>
    public static double? GetConfigNullableDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double? num = root.ToDoubleNullable(elemName);
        return num;
    }
    /// <summary>
    /// Retrieves a TimeSpan value from a configuration XML file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element containing the TimeSpan value.</param>
    /// <returns>The TimeSpan value.</returns>
    /// <exception cref="FormatException">Thrown if the element is missing, empty, or cannot be converted to a TimeSpan.</exception>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string? value = root.Element(elemName)?.Value;

        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException($"Missing or empty element: {xmlFileName}, {elemName}");

        if (TimeSpan.TryParse(value, out TimeSpan ts))
            return ts;

        throw new FormatException($"Can't convert value to TimeSpan: {xmlFileName}, {elemName}");
    }
    /// <summary>
    /// Sets a TimeSpan value in a configuration XML file and saves the file.
    /// </summary>
    /// <param name="xmlFileName">The name of the configuration XML file.</param>
    /// <param name="elemName">The name of the element to set.</param>
    /// <param name="value">The TimeSpan value to set.</param>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan value)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        XElement? elem = root.Element(elemName);

        if (elem != null)
        {
            elem.Value = value.ToString();
            XMLTools.SaveListToXMLElement(root, xmlFileName);
        }
    }
    #endregion

    #region ExtensionFuctions
    /// <summary>
    /// Extension method to safely convert the value of a specified child element of an XElement to a nullable Enum.
    /// </summary>
    /// <typeparam name="T">The target Enum type.</typeparam>
    /// <param name="element">The parent XElement.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The converted Enum value as nullable T, or null if parsing fails.</returns>
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;
    /// <summary>
    /// Extension method to safely convert the value of a specified child element of an XElement to a nullable DateTime.
    /// </summary>
    /// <param name="element">The parent XElement.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The converted DateTime value as nullable DateTime, or null if parsing fails.</returns>
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;
    /// <summary>
    /// Extension method to safely convert the value of a specified child element of an XElement to a nullable double.
    /// </summary>
    /// <param name="element">The parent XElement.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The converted double value as nullable double, or null if parsing fails.</returns>
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;
    /// <summary>
    /// Extension method to safely convert the value of a specified child element of an XElement to a nullable integer.
    /// </summary>
    /// <param name="element">The parent XElement.</param>
    /// <param name="name">The name of the child element.</param>
    /// <returns>The converted integer value as nullable int, or null if parsing fails.</returns>
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;
    #endregion
}