// ReSharper disable InconsistentNaming

namespace Hades.Error
{
    public static class ErrorStrings
    {
        public const string MESSAGE_UNKNOWN_RUNTIME_EXCEPTION = "Unknown runtime exception! Please check the AST! \n{0}";
        public const string MESSAGE_DUPLICATE_VARIABLE_DECLARATION = "Duplicate decleration of variable {0}!";
        
        public const string MESSAGE_EXPECTED_TYPE = "Expected a type!";
        public const string MESSAGE_EXPECTED_ACCESS_MODIFIER = "Expected an access modifier!";
        public const string MESSAGE_ILLEGAL_PROTECTED_IN_STRUCT = "A struct cannot contain a protected variable!";
        public const string MESSAGE_UNEXPECTED_CALL_OR_INLINE_IF = "Unexpected call or inline if!";
        public const string MESSAGE_CANT_USE_COMPLEX_LAMBDA_IN_MATCH_BLOCK = "Can't use a complex lambda in a match block!";
        public const string MESSAGE_UNEXPECTED_NODE = "Unexpected node: {0}!";
        public const string MESSAGE_UNEXPECTED_ACCESS_MODIFIER = "Unexpected access modifier!";
        public const string MESSAGE_UNEXPECTED_STATEMENT = "Unexpected statement!";
        public const string MESSAGE_CANT_HAVE_MULTIPLE_VARARGS = "Can't have multiple 'args' parameters in one function!";
        public const string MESSAGE_EXPECTED_IN = "Expected 'in' keyword!";
        public const string MESSAGE_UNEXPECTED_KEYWORD = "Unexpected keyword: {0}!";
        public const string MESSAGE_EXPECTED_LEFT_PARENTHESIS = "Expected left parenthesis!";
        public const string MESSAGE_OVERRIDE_WITHOUT_DECLARATION = "Function can't override another function or an operator without being marked as an override function!";
        public const string MESSAGE_EXPECTED_RIGHT_PARENTHESIS = "Expected right parenthesis!";
        public const string MESSAGE_EXPECTED_VALUE = "Expected {0}!";
        public const string MESSAGE_INVALID_ARRAY_EXPECTED_COMMA = "Invalid array literal! Expected a comma!";
        public const string MESSAGE_EXPECTED_COMMA = "Expected a comma!";
        public const string MESSAGE_EXPECTED_COLON = "Expected a colon!";
        public const string MESSAGE_EXPECTED_PARAMETERS = "Expected parameters!";
        public const string MESSAGE_IMMUTABLE_CANT_BE_NULLABLE = "An immutable variable can't be nullable!";
        public const string MESSAGE_TYPE_INFERRED_CANT_BE_NULLABLE = "A type infered variable can't be nullable!";
        public const string MESSAGE_IMMUTABLE_CANT_BE_DYNAMIC = "An immutable variable can't be dynamic!";
        public const string MESSAGE_DYNAMIC_NOT_POSSIBLE_WITH_STATIC_TYPES = "A variable with a static type can't also be a dynamic variable!";
        public const string MESSAGE_UNEXPECTED_EOF = "Unexpected end of file!";
        public const string MESSAGE_INVALID_LITERAL = "Invalid literal: {0}!";
        public const string MESSAGE_EXPECTED_TOKEN = "Expected token: {0}!";
        public const string MESSAGE_UNEXPECTED_TOKEN = "Unexpected token: {0}!";
        public const string MESSAGE_EXPECTED_IDENTIFIER = "Expected an identifier!";
    }
}