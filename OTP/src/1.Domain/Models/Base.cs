using System;
using System.ComponentModel.DataAnnotations;

namespace OTP.Domain.Models
{
    public class BaseClass
    {
        protected BaseClass()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
