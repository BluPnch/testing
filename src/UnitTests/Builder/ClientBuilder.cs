using Domain.Models;

namespace UnitTests.Builders
{
    public class ClientBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _companyName = "Default Company";
        private string _phoneNumber = "0000000000";

        public ClientBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ClientBuilder WithCompanyName(string companyName)
        {
            _companyName = companyName;
            return this;
        }

        public ClientBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public Client Build()
        {
            return new Client(_id, _companyName, _phoneNumber);
        }

        public static implicit operator Client(ClientBuilder builder)
        {
            return builder.Build();
        }
    }
}