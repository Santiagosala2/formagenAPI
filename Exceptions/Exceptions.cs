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

    public class UnexpectedCosmosException : Exception
    {

        public UnexpectedCosmosException(string message, CosmosException inner) : base(message, inner)
        {
        }
    }

}
