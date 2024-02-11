namespace Models {
    public class Player {
        public string Id {get; set;}
        public string Username {get; set;}
        public Player(string id, string username) {
            Id = id;
            Username = username;
        }   
    }
}