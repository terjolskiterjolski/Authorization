const form = document.getElementById("verificationForm");
const inputs = [...document.querySelectorAll(".code-input")];
const message = document.getElementById("verificationMessage");
const emailInput = document.getElementById("verificationEmail");
const sendCodeButton = document.getElementById("sendCodeButton");
const changeEmailButton = document.getElementById("changeEmailButton");
const codeSection = document.getElementById("codeSection");
const verifyCodeButton = document.getElementById("verifyCodeButton");
const queryEmail = new URLSearchParams(window.location.search).get("email")?.trim();
let sentToEmail = "";

if (queryEmail) emailInput.value = queryEmail;

sendCodeButton.addEventListener("click", async () => {
    const email = emailInput.value.trim();
    if (!emailInput.checkValidity()) {
        showMessage("Please enter a valid email address.", false);
        emailInput.focus();
        return;
    }

    setSending(true);
    showMessage("", false);

    try {
        const response = await fetch(`/api/password-reset/code?email=${encodeURIComponent(email)}`, {
            method: "POST"
        });
        const result = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(result.message || "We could not send the code. Please try again.");

        sentToEmail = email;
        emailInput.readOnly = true;
        codeSection.hidden = false;
        verifyCodeButton.hidden = false;
        document.getElementById("verificationDescription").textContent =
            `Enter the six-digit code sent to ${maskEmail(email)}.`;
        showMessage(result.message || "Verification code sent.", true);
        inputs[0].focus();
    } catch (error) {
        showMessage(error.message, false);
    } finally {
        setSending(false);
    }
});

changeEmailButton.addEventListener("click", () => {
    sentToEmail = "";
    emailInput.readOnly = false;
    codeSection.hidden = true;
    verifyCodeButton.hidden = true;
    inputs.forEach(input => input.value = "");
    document.getElementById("verificationDescription").textContent =
        "Enter your email address and we’ll send you a six-digit code.";
    showMessage("", false);
    emailInput.focus();
});

inputs.forEach((input, index) => {
    input.addEventListener("input", () => {
        input.value = input.value.replace(/\D/g, "").slice(-1);
        if (input.value && index < inputs.length - 1) inputs[index + 1].focus();
    });

    input.addEventListener("keydown", event => {
        if (event.key === "Backspace" && !input.value && index > 0) inputs[index - 1].focus();
        if (event.key === "ArrowLeft" && index > 0) inputs[index - 1].focus();
        if (event.key === "ArrowRight" && index < inputs.length - 1) inputs[index + 1].focus();
    });

    input.addEventListener("paste", event => {
        const digits = event.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
        if (!digits) return;
        event.preventDefault();
        digits.split("").forEach((digit, digitIndex) => {
            if (inputs[digitIndex]) inputs[digitIndex].value = digit;
        });
        inputs[Math.min(digits.length, 6) - 1].focus();
    });
});

form.addEventListener("submit", async event => {
    event.preventDefault();
    if (!sentToEmail) {
        showMessage("Send a code to your email first.", false);
        emailInput.focus();
        return;
    }
    const code = inputs.map(input => input.value).join("");

    if (!/^\d{6}$/.test(code)) {
        showMessage("Please enter all six digits.", false);
        (inputs.find(input => !input.value) ?? inputs[0]).focus();
        return;
    }

    verifyCodeButton.disabled = true;
    try {
        const response = await fetch("/api/password-reset/verify", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email: sentToEmail, code: Number(code) })
        });
        const result = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(result.message || "The code is invalid or has expired.");

        sessionStorage.setItem("passwordResetCode", code);
        sessionStorage.setItem("passwordResetEmail", sentToEmail);
        window.location.assign("/reset-password.html");
    } catch (error) {
        showMessage(error.message, false);
    } finally {
        verifyCodeButton.disabled = false;
    }
});

function setSending(sending) {
    sendCodeButton.disabled = sending;
    sendCodeButton.textContent = sending ? "Sending…" : (sentToEmail ? "Resend" : "Send code");
}

function maskEmail(value) {
    const [name, domain] = value.split("@");
    if (!domain) return value;
    const visible = name.slice(0, Math.min(2, name.length));
    return `${visible}${"*".repeat(Math.max(1, name.length - visible.length))}@${domain}`;
}

function showMessage(text, successful) {
    message.textContent = text;
    message.className = `form-message ${successful ? "success" : "error"}`;
}
