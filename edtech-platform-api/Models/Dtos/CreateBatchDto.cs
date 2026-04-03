using System;
using System.ComponentModel.DataAnnotations;

namespace edtech_platform_api.Models.Dtos
{
    public class CreateBatchDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? MentorName { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
        public int Capacity { get; set; }
    }
}
