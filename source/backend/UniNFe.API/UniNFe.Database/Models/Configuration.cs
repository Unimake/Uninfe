using System;
using UniNFe.Database.Enums;

namespace UniNFe.Database.Models
{
    public class Configuration
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }

        public string Email { get; set; }
    }
}
