using DataAccess.Models;

namespace UnitTests.Builders
{
    public class ClientDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _companyName = "Company";
        private string _phoneNumber = "0000000000";

        public ClientDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ClientDbBuilder WithCompanyName(string companyName)
        {
            _companyName = companyName;
            return this;
        }

        public ClientDbBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public ClientDb Build()
        {
            return new ClientDb(_id, _companyName, _phoneNumber);
        }

        public static implicit operator ClientDb(ClientDbBuilder builder)
        {
            return builder.Build();
        }
    }
}