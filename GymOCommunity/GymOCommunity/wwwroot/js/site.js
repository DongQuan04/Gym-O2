// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const themeToggle = document.getElementById("themeToggle");
    const themeIcon = document.getElementById("themeIcon");

    if (!themeToggle || !themeIcon) return;

    function updateThemeIcon() {
        if (document.body.classList.contains("dark-mode")) {
            themeIcon.textContent = "☀️"; // Biểu tượng mặt trời cho Light Mode
        } else {
            themeIcon.textContent = "🌙"; // Biểu tượng mặt trăng cho Dark Mode
        }
    }

    themeToggle.addEventListener("click", function () {
        document.body.classList.toggle("dark-mode");
        localStorage.setItem("theme", document.body.classList.contains("dark-mode") ? "dark" : "light");
        updateThemeIcon();
    });

    if (localStorage.getItem("theme") === "dark") {
        document.body.classList.add("dark-mode");
    }
    updateThemeIcon();
});







