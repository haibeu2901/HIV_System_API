// Get doctorId from localStorage (stored as accId)
const doctorId = localStorage.getItem('accId');

// Function to fetch doctor's work schedule with JWT from localStorage
async function getWorkSchedule(doctorId) {
    try {
        const token = localStorage.getItem('token'); // Assumes token is stored as 'token'
        const response = await fetch(`https://localhost:7009/api/DoctorWorkSchedule/GetDoctorWorkSchedules?doctorId=${doctorId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            throw new Error('Failed to fetch work schedule');
        }
        const data = await response.json();
        return data; // Expecting an array of schedules
    } catch (error) {
        console.error('Error fetching work schedule:', error);
        return null;
    }
    console.log(data);
}

// Helper to convert dayOfWeek to day name
function getDayName(dayOfWeek) {
    const days = [null, "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
    return days[dayOfWeek] || "Unknown";
}

// Render schedule as a table
async function renderWorkSchedule(doctorId, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = "<p>Loading work schedule...</p>";
    const schedules = await getWorkSchedule(doctorId);

    if (schedules && schedules.length > 0) {
        // Filter for this doctor (should already be filtered by API, but just in case)
        const filtered = schedules.filter(item => item.doctorId == doctorId);
        if (filtered.length === 0) {
            container.innerHTML = "<p>No work schedule found for this doctor.</p>";
            return;
        }
        container.innerHTML = `
            <table style="width:100%;border-collapse:collapse;">
                <thead>
                    <tr>
                        <th style="border-bottom:1px solid #ccc;padding:8px;text-align:left;">Day</th>
                        <th style="border-bottom:1px solid #ccc;padding:8px;text-align:left;">Start Time</th>
                        <th style="border-bottom:1px solid #ccc;padding:8px;text-align:left;">End Time</th>
                    </tr>
                </thead>
                <tbody>
                    ${filtered.map(item => `
                        <tr>
                            <td style="padding:8px;">${getDayName(item.dayOfWeek)}</td>
                            <td style="padding:8px;">${item.startTime.slice(0,5)}</td>
                            <td style="padding:8px;">${item.endTime.slice(0,5)}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        `;
    } else {
        container.innerHTML = "<p>No work schedule found.</p>";
    }
}

// Function to fetch doctor profile (excluding workSchedule)
async function getDoctorProfile(accId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7009/api/Doctor/GetDoctorById?id=${accId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        if (!response.ok) {
            throw new Error('Failed to fetch doctor profile');
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching doctor profile:', error);
        return null;
    }
}

// Render doctor profile (excluding workSchedule)
async function renderDoctorProfile(accId, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = "<p>Loading profile...</p>";
    const data = await getDoctorProfile(accId);
    if (data) {
        const account = data.account || {};
        container.innerHTML = `
            <div class="profile-card">
                <h3>${account.fullname || "Unknown"}</h3>
                <p><strong>Username:</strong> ${account.accUsername || ""}</p>
                <p><strong>Email:</strong> ${account.email || ""}</p>
                <p><strong>Degree:</strong> ${data.degree || ""}</p>
                <p><strong>Bio:</strong> ${data.bio || ""}</p>
                <p><strong>Date of Birth:</strong> ${account.dob || ""}</p>
                <p><strong>Gender:</strong> ${account.gender === false ? "Female" : "Male"}</p>
            </div>
        `;
    } else {
        container.innerHTML = "<p style='color:#e74c3c;'>Failed to load profile.</p>";
    }
}

// Navigation and event logic
function setActive(btn) {
    const btnSchedule = document.getElementById('btnSchedule');
    const btnProfile = document.getElementById('btnProfile');
    [btnSchedule, btnProfile].forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
}

function logout() {
    window.location.href = "../../../public-view/landingpage.html";
}

// Attach event listeners after DOM is loaded
window.addEventListener('DOMContentLoaded', () => {
    const btnSchedule = document.getElementById('btnSchedule');
    const btnProfile = document.getElementById('btnProfile');
    const btnLogout = document.getElementById('btnLogout');
    const logoutBtn = document.getElementById('logoutBtn');
    const dashboardHeader = document.getElementById('dashboardHeader');
    const dashboardSection = document.getElementById('dashboardSection');

    btnSchedule.onclick = function() {
        setActive(btnSchedule);
        dashboardHeader.textContent = "Work Schedule";
        renderWorkSchedule(doctorId, 'dashboardSection');
    };

    btnProfile.onclick = function() {
        setActive(btnProfile);
        dashboardHeader.textContent = "Profile";
        renderDoctorProfile(doctorId, 'dashboardSection');
    };

    btnLogout.onclick = logout;
    if (logoutBtn) logoutBtn.onclick = logout;

    // Load schedule by default
    btnSchedule.click();
});