document.addEventListener("DOMContentLoaded", function () {
    const themeToggle = document.getElementById("themeToggle");
    const themeIcon = document.getElementById("themeIcon");
    const body = document.body;

    if (!themeToggle || !themeIcon) return;

    function setTheme(theme) {
        if (theme === "dark") {
            body.classList.add("dark-mode");
            themeIcon.textContent = "☀️";
        } else {
            body.classList.remove("dark-mode");
            themeIcon.textContent = "🌙";
        }
    }

    themeToggle.addEventListener("click", () => {
        const currentTheme = body.classList.contains("dark-mode") ? "dark" : "light";
        const newTheme = currentTheme === "dark" ? "light" : "dark";
        localStorage.setItem("theme", newTheme);
        setTheme(newTheme);
    });

    const savedTheme = localStorage.getItem("theme") || "light";
    setTheme(savedTheme);
});
