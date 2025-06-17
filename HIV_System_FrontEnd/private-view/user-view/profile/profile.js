document.addEventListener("DOMContentLoaded", async () => {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "../../public-view/landingpage.html";
        return;
    }

    try {
        const response = await fetch("https://localhost:7009/api/Account/View-profile", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error("Failed to fetch profile");

        const data = await response.json();

        // Render editable form
        document.getElementById("profile-details").innerHTML = `
            <form id="profileForm">
                <ul class="profile-list">
                    <li><strong>Account ID:</strong> <span>${data.accId}</span></li>
                    <li><strong>Username:</strong> <span>${data.accUsername}</span></li>
                    <li>
                        <strong>Email:</strong>
                        <input type="email" id="email" name="email" value="${data.email}" required />
                    </li>
                    <li>
                        <strong>Full Name:</strong>
                        <input type="text" id="fullname" name="fullname" value="${data.fullname}" required />
                    </li>
                    <li>
                        <strong>Date of Birth:</strong>
                        <input type="date" id="dob" name="dob" value="${data.dob ? data.dob.substring(0,10) : ''}" required />
                    </li>
                    <li>
                        <strong>Gender:</strong>
                        <select id="gender" name="gender">
                            <option value="true" ${data.gender ? "selected" : ""}>Male</option>
                            <option value="false" ${!data.gender ? "selected" : ""}>Female</option>
                        </select>
                    </li>
                    <li>
                        <strong>Password:</strong>
                        <input type="password" id="accPassword" name="accPassword" value="${data.accPassword || ''}" required />
                    </li>
                    <li>
                        <strong>Role:</strong> <span>${data.roles}</span>
                    </li>
                    <li>
                        <strong>Status:</strong> <span>${data.isActive ? "Active" : "Inactive"}</span>
                    </li>
                </ul>
                <button type="submit" class="btn-profile" style="margin-top:1rem;">Save Changes</button>
            </form>
        `;

        // Handle form submission
        document.getElementById("profileForm").addEventListener("submit", async function(e) {
            e.preventDefault();

            // Email validation (simple regex)
            const email = document.getElementById("email").value.trim();
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailPattern.test(email)) {
                showMessage("Please enter a valid email address.", true);
                return;
            }

            const fullname = document.getElementById("fullname").value.trim();
            const dob = document.getElementById("dob").value;
            const gender = document.getElementById("gender").value === "true";
            const accPassword = document.getElementById("accPassword").value;

            const updateData = {
                email,
                fullname,
                dob,
                gender,
                accPassword
            };

            try {
                const updateResponse = await fetch("https://localhost:7009/api/Account/UpdatePatientProfile", {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify(updateData)
                });

                if (!updateResponse.ok) {
                    const errMsg = await updateResponse.text();
                    throw new Error(errMsg || "Failed to update profile");
                }

                showMessage("Profile updated successfully!", false);
            } catch (err) {
                showMessage("Update failed: " + err.message, true);
            }
        });

    } catch (err) {
        document.getElementById("profile-details").innerHTML = `<p style="color:red;">Error loading profile.</p>`;
    }
});

// Helper to show messages
function showMessage(msg, isError) {
    const msgDiv = document.getElementById("profile-message");
    msgDiv.textContent = msg;
    msgDiv.style.color = isError ? "red" : "green";
    setTimeout(() => { msgDiv.textContent = ""; }, 4000);
}