// Function to fetch doctor profile (excluding workSchedule)
async function getDoctorProfile(accId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/Doctor/ViewDoctorProfile?id=${accId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) throw new Error('Failed to fetch doctor profile');
        return await response.json();
    } catch (error) {
        console.error('Error fetching doctor profile:', error);
        return null;
    }
}

// Render doctor profile (excluding workSchedule)
async function renderDoctorProfile(accId, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = "<p>Loading profile...</p>";
    const profile = await getDoctorProfile(accId);
    if (profile) {
        container.innerHTML = `
            <div class="profile-header">
                <h2>${profile.fullname}</h2>
                <p><strong>Email:</strong> ${profile.email}</p>
                <p><strong>Date of Birth:</strong> ${profile.dob}</p>
                <p><strong>Gender:</strong> ${profile.gender ? "Male" : "Female"}</p>
                <p><strong>Degree:</strong> ${profile.degree}</p>
                <p><strong>Bio:</strong> ${profile.bio}</p>
            </div>
        `;
    } else {
        container.innerHTML = "<p>Failed to load profile.</p>";
    }
}

// On page load, render the profile
window.addEventListener('DOMContentLoaded', () => {
    const accId = localStorage.getItem('accId');
    renderDoctorProfile(accId, 'profileSection');
});