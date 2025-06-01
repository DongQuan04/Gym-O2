using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymOCommunity.Models
{
    public class WorkoutLog
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string MuscleGroup { get; set; }

        [Required]
        public string ExerciseName { get; set; }

        public int Sets { get; set; }
        public int Reps { get; set; }

        public string Notes { get; set; }
    }
}
