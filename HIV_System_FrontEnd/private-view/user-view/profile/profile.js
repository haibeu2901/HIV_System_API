document.addEventListener("DOMContentLoaded", () => {
    const accountStr = localStorage.getItem("account");
    if (!accountStr) {
        window.location.href = "../../public-view/landingpage.html";
        return;
    }

    const data = JSON.parse(accountStr);

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
});