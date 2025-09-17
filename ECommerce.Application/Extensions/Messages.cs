namespace ECommerce.Application.Extensions
{
    public static class Messages
    {
        public static string GetErrorOccurredMessage()
        {
            return $"An unexpected error occurred while processing the request.";
        }

        public static string GetNotFoundMessage(string className)
        {
            return $"{className} not found.";
        }

        public static string GetCreatedMessage(string className)
        {
            return $"{className} has been successfully created.";
        }

        public static string GetUpdatedMessage(string className)
        {
            return $"{className} has been successfully updated.";
        }

        public static string GetDeletedMessage(string className)
        {
            return $"{className} has been successfully deleted.";
        }

        public static string GetRetrievedMessage(string className)
        {
            return $"{className} has been successfully retrieved.";
        }

        public static string GetRetrievedPluralMessage(string className)
        {
            return $"{className}s have been successfully retrieved.";
        }

        public static string GetNameExistsMessage(string className)
        {
            return $"A {className} with the same name already exists.";
        }

        public static string GetForeignKeyNotFoundMessage(string relatedEntityName, int id)
        {
            return $"{relatedEntityName} with ID {id} was not found.";
        }
    }
}
