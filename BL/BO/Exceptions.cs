namespace BO;

[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
                : base(message, innerException) { }

}
/// <summary>
/// generic exception for when an entity already exists in the database
/// </summary>  

[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string? message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException)
                : base(message, innerException) { }

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
public class BlFailedToGenerate : Exception
{
    public BlFailedToGenerate(string? message) : base(message) { }
    public BlFailedToGenerate(string message, Exception innerException)
                : base(message, innerException) { }

}

/// <summary>
/// validation exception for XML file load/create issues
/// </summary>

[Serializable]
public class BlXMLFileLoadCreateException : Exception
{
    public BlXMLFileLoadCreateException(string? message) : base(message) { }
    public BlXMLFileLoadCreateException(string message, Exception innerException)
               : base(message, innerException) { }
}

[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
}

[Serializable]
public class BlInvalidInputException : Exception
{
    public BlInvalidInputException(string? message) : base(message) { }
    public BlInvalidInputException(string message, Exception innerException)
               : base(message, innerException) { }
}


[Serializable]
public class BlInvalidOperationException : Exception
{
    public BlInvalidOperationException(string? message)
        : base(message)
    {
    }

    public BlInvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// exception for when an external service (e.g., payment gateway, geolocation API) fails or returns an error
/// </summary>
[Serializable]
public class BlExternalServiceException : Exception
{
    public BlExternalServiceException(string? message) : base(message) { }
    public BlExternalServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}


[Serializable]
public class BLTemporaryNotAvailableException
 : Exception
{
    public BLTemporaryNotAvailableException(string? message)
        : base(message)
    {
    } 
} 


