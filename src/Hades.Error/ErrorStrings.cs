namespace Hades.Error
{
    public static class ErrorStrings
    {
        public const string MESSAGE_EXPECTED_COMMA = "Expected a comma!";
        public const string MESSAGE_EXPECTED_PARAMETERS = "Expected parameters!";
        public const string MESSAGE_IMMUTABLE_CANT_BE_NULLABLE = "An immutable variable can't be nullable!";
        public const string MESSAGE_TYPE_INFERED_CANT_BE_NULLABLE = "A type infered variable can't be nullable!";
        public const string MESSAGE_IMMUTABLE_CANT_BE_DYNAMIC = "An immutable variable can't be dynamic!";
        public const string MESSAGE_DYNAMIC_NOT_POSSIBLE_WITH_STATIC_TYPES = "A variable with a static type can't also be a dynamic variable!";
        public const string MESSAGE_UNEXPECTED_EOF = "Unexpected end of file!";
        public const string MESSAGE_INVALID_LITERAL = "Invalid literal: {0}!";
        public const string MESSAGE_EXPECTED_TOKEN = "Expected token: {0}!";
        public const string MESSAGE_UNEXPECTED_TOKEN = "Unexpected token: {0}!";
        public const string MESSAGE_EXPECTED_IDENTIFIER = "Expected an identifier!";
    }
}