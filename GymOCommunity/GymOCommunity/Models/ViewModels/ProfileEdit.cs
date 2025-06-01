using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymOCommunity.ViewModels
{
    public class ProfileEditViewModel
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Tên phải từ {2} đến {1} ký tự.", MinimumLength = 2)]
        public string FullName { get; set; }

        [StringLength(500, ErrorMessage = "Tiểu sử không được quá {1} ký tự.")]
        public string Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public IFormFile AvatarFile { get; set; }

        [StringLength(100, ErrorMessage = "Địa điểm không được quá {1} ký tự.")]
        public string Location { get; set; }

        [Url]
        [StringLength(100, ErrorMessage = "Website không được quá {1} ký tự.")]
        public string Website { get; set; }
    }
}