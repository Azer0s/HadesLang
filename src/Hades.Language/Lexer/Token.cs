namespace Hades.Language.Lexer
{
	public struct Token
	{
		public Type Type;
		public string Value;

		public override string ToString()
		{
			return $"[{Type.ToString().ToUpper()}] {Value}";
		}
	}
}