namespace NessWebApi.Helper
{
    public class EmailBody
    {
        public static string EmailStringBody(string email, string emailToken)
        {
            return $@"
                     <h1>Salut, {email}!</h1>
                     <h1>Te rugăm să accesezi următorul link pentru a-ți reseta parola:</h1>
                      <a href=""https://www.exemplu.com/reset-password?token={emailToken}""
                      Dacă nu ai solicitat această acțiune, te rugăm să ignori acest e-mail.

                      Cu respect,
                      Echipa Exemplu";
        }
    }
}
