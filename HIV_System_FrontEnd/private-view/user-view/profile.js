document.addEventListener("DOMContentLoaded", async () => {
    // Get patient ID from localStorage (set after login)
    const accId = localStorage.getItem("accId");
    if (!accId) {
        window.location.href = "../../public-view/landingpage.html";
        return;
    }

    try {
        const response = await fetch(`https://localhost:7009/api/Account/GetAccountById/${accId}`);
        if (!response.ok) throw new Error("Failed to fetch profile");

        const data = await response.json();

        document.getElementById("profile-details").innerHTML = `
            <ul class="profile-list">
                <li><strong>Account ID:</strong> ${data.accId}</li>
                <li><strong>Username:</strong> ${data.accUsername}</li>
                <li><strong>Email:</strong> ${data.email}</li>
                <li><strong>Full Name:</strong> ${data.fullname}</li>
                <li><strong>Date of Birth:</strong> ${data.dob}</li>
                <li><strong>Gender:</strong> ${data.gender ? "Male" : "Female"}</li>
                <li><strong>Role:</strong> ${data.roles}</li>
                <li><strong>Status:</strong> ${data.isActive ? "Active" : "Inactive"}</li>
            </ul>
        `;
    } catch (err) {
        document.getElementById("profile-details").innerHTML = `<p style="color:red;">Error loading profile.</p>`;
    }

    
});