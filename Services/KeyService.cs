namespace ServerAPI.Services
{
    public interface IKeyService
    {
        string GetKey();
    }

    public class KeyService : IKeyService
    {
        private readonly string _key;

        public KeyService(string key)
        {
            _key = key;
        }

        public string GetKey() => _key;
    }

}
