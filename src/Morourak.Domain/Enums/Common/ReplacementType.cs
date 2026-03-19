using System.ComponentModel.DataAnnotations;

namespace Morourak.Domain.Enums.Common
{
    public enum ReplacementType
    {
        [Display(Name = "بدل فاقد")]
        Lost,

        [Display(Name = "بدل تالف")]
        Damaged
    }
}