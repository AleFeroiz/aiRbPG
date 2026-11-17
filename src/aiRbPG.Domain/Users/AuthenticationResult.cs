namespace aiRbPG.Domain.Users
{
    public sealed class AuthenticationResult
    {
        private AuthenticationResult(bool succeeded, UserAccount user, string message)
        { Succeeded = succeeded; User = user; Message = message; }
        public bool Succeeded { get; private set; }
        public UserAccount User { get; private set; }
        public string Message { get; private set; }
        public static AuthenticationResult Success(UserAccount user) { return new AuthenticationResult(true, user, "Acesso autorizado."); }
        public static AuthenticationResult Failure(string message) { return new AuthenticationResult(false, null, message); }
    }
}
