namespace FormagenAPI.Exceptions
{

    public class FormNotFoundException : Exception
    {
        public FormNotFoundException(string message) : base(message)
        {
        }

        public FormNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class CreateFormException : Exception
    {
        public CreateFormException(string message) : base(message)
        {
        }

        public CreateFormException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class DeleteFormException : Exception
    {
        public DeleteFormException(string message) : base(message)
        {
        }

        public DeleteFormException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class UpdateFormException : Exception
    {
        public UpdateFormException(string message) : base(message)
        {
        }

        public UpdateFormException(string message, Exception inner) : base(message, inner)
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
}
