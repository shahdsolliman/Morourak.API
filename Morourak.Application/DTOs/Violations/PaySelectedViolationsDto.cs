using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Violations
{
    /// <summary>
    /// Request DTO for paying multiple selected violations at once.
    /// </summary>
    public class PaySelectedViolationsDto
    {
        /// <summary>List of violation identifiers to be paid.</summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one violation ID is required.")]
        public List<int> ViolationIds { get; set; } = new();
    }
}
