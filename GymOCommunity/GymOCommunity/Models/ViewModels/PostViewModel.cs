﻿



public class PostViewModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; } = "Ẩn danh";
}
