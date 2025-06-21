using Microsoft.Azure.Cosmos;

namespace FormagenAPI.Exceptions
{
    public class FormNotFoundException : Exception
    {

        public FormNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class FormNameIsNotUniqueException : Exception
    {

        public FormNameIsNotUniqueException(string message) : base(message)
        {
        }

        public FormNameIsNotUniqueException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class UserEmailNotUniqueException : Exception
    {

        public UserEmailNotUniqueException(string message) : base(message)
        {
        }

        public UserEmailNotUniqueException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class AdminSessionNotFoundException : Exception
    {

        public AdminSessionNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class AdminUserNotFoundException : Exception
    {

        public AdminUserNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class AdminUserOwnerException : Exception
    {

        public AdminUserOwnerException(string message) : base(message)
        {
        }
    }


    public class AdminUserSessionsCouldNotDeleteException : Exception
    {

        public AdminUserSessionsCouldNotDeleteException(string message) : base(message)
        {
        }
    }

    public class UnexpectedCosmosException : Exception
    {

        public UnexpectedCosmosException(string message, CosmosException inner) : base(message, inner)
        {
        }
    }

}
