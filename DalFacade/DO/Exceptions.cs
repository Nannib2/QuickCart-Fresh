namespace DO;
/// <summary>
/// when an entity is not found in the database
/// </summary>

[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}
/// <summary>
/// generic exception for when an entity already exists in the database
/// </summary>  

[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}




/// <summary>
/// These custom exceptions, DalFailedToGenerate and DalNullReferenceException, 
/// are essential for robust error handling within the Data Access Layer (DAL) 
/// by providing explicit and contextual feedback to the calling layers. 
/// DalFailedToGenerate precisely signals that the DAL failed to successfully create a required unique identifier (e.g., ID, email, or token), 
/// allowing the Business Logic Layer to execute targeted recovery logic like retrying the operation or logging a specific data saturation warning. 
/// While the second class carries a standard name, it serves to encapsulate internal DAL issues, such as unexpected null data dependencies or 
/// configuration problems, ensuring a clear separation of concerns so that errors originating from data access are distinct from generic system exceptions, 
/// which significantly improves debugging speed and application maintainability.
/// <summary>


/// <summary>
///  a generic exception for when the DAL fails to generate an item(email,id, password,phone number etc)
/// </summary>
[Serializable]
public class DalFailedToGenerate : Exception
{
    public DalFailedToGenerate(string? message) : base(message) { }
}

[Serializable]
public class DalNullReferenceException : Exception
{
    public DalNullReferenceException(string? message) : base(message) { }
}


/// <summary>
/// validation exception for XML file load/create issues
/// </summary>
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}



