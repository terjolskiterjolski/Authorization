const registerForm = document.getElementById("registerForm");
const loginForm = document.getElementById("loginForm");

registerForm?.addEventListener("submit", async function (event) {
    event.preventDefault(); // Do not reload the page.

    const password = document.getElementById("password").value;
    const confirmation = document.getElementById("confirmPassword").value;
    if (password !== confirmation) {
        showMessage("registerMessage", "Passwords do not match.", false);
        return;
    }

    const customer = {
        name: document.getElementById("name").value,
        email: document.getElementById("email").value,
        phone: document.getElementById("phone").value,
        address: document.getElementById("address").value,
        id: Number(document.getElementById("customerId").value),
        password
    };
    const submitButton = registerForm.querySelector("button[type='submit']");
    submitButton.disabled = true;
    try {
        const response = await fetch("/api/register", {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(customer)
        });
        const data = await response.json();
        showMessage("registerMessage", data.message, response.ok);
        if (response.ok) {
            registerForm.reset();
            setTimeout(() => window.location.href = "login.html", 700);
        }
    } catch {
        showMessage("registerMessage", "Could not connect to the server. Please try again.", false);
    } finally {
        submitButton.disabled = false;
    }
});

loginForm?.addEventListener("submit", async function (event) {
    event.preventDefault();

    const loginData = {
        email: document.getElementById("loginEmail").value,
        password: document.getElementById("loginPassword").value
    };

    const submitButton = loginForm.querySelector("button[type='submit']");
    submitButton.disabled = true;
    try {
        const response = await fetch("/api/login", {
            method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(loginData)
        });
        if (response.ok) {
            const data = await response.json();
            showMessage("loginMessage", data.message, true);
            window.location.assign("/product-search/index.html");
        } else {
            showMessage("loginMessage", "Invalid email or password.", false);
        }
    } catch {
        showMessage("loginMessage", "Could not connect to the server. Please try again.", false);
    } finally {
        submitButton.disabled = false;
    }
});

function showMessage(elementId, message, isSuccessful) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.className = isSuccessful ? "success" : "error";
}

document.querySelectorAll(".eye-toggle").forEach(button => {
    button.addEventListener("click", () => {
        const input = document.getElementById(button.dataset.target);
        const reveal = input.type === "password";
        input.type = reveal ? "text" : "password";
        button.textContent = reveal ? "◎" : "◉";
        button.setAttribute("aria-label", reveal ? "Hide password" : "Show password");
    });
});
