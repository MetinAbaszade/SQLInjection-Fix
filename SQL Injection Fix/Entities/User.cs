namespace SQL_Injection_Fix.Entities
{
    public class User
    {
        public User()
        {
            
        }

        public User(string name, string password)
        {
            Name = name;
            Password = password;
        }

        public int Id { get; set; }     
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
