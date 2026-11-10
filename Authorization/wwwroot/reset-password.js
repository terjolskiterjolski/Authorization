const form = document.getElementById("resetPasswordForm");
const message = document.getElementById("resetPasswordMessage");
const email = sessionStorage.getItem("passwordResetEmail");
const code = sessionStorage.getItem("passwordResetCode");

if (!email || !/^\d{6}$/.test(code ?? "")) {
    window.location.replace("/verify-code.html");
}

form.addEventListener("submit", async event => {
    event.preventDefault();
    const newPassword = document.getElementById("newPassword").value;
    const confirmation = document.getElementById("confirmNewPassword").value;
    if (newPassword !== confirmation) {
        showMessage("Passwords do not match.", false);
        return;
    }

    const button = form.querySelector("button[type='submit']");
    button.disabled = true;
    try {
        const response = await fetch("/api/password-reset", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, code: Number(code), newPassword })
        });
        const result = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(result.message || "The password could not be reset.");

        sessionStorage.removeItem("passwordResetEmail");
        sessionStorage.removeItem("passwordResetCode");
        window.location.assign("/product-search/index.html");
    } catch (error) {
        showMessage(error.message, false);
        button.disabled = false;
    }
});

function showMessage(text, successful) {
    message.textContent = text;
    message.className = `form-message ${successful ? "success" : "error"}`;
}
