// Role-based user profile script
async function getUserProfile(accId, role) {
    const token = localStorage.getItem('token');
    if (role === 'doctor') {
        // Doctor API
        try {
            const response = await fetch(`https://localhost:7009/api/Doctor/ViewDoctorProfile?id=${accId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error('Failed to fetch doctor profile');
            return await response.json();
        } catch (error) {
            console.error('Error fetching doctor profile:', error);
            return null;
        }
    } else if (role === 'staff') {
        // Staff API
        try {
            const response = await fetch(`https://localhost:7009/api/Staff/GetStaffById/${accId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!response.ok) throw new Error('Failed to fetch staff profile');
            return await response.json();
        } catch (error) {
            console.error('Error fetching staff profile:', error);
            return null;
        }
    }
    // Add more roles as needed
    return null;
}

async function renderUserProfile(accId, role, containerId) {
    const container = document.getElementById(containerId);
    container.innerHTML = '<p>Loading profile...</p>';
    const profile = await getUserProfile(accId, role);
    if (profile) {
        if (role === 'doctor') {
            container.innerHTML = `
                <div class="profile-header">
                    <h2>${profile.fullname}</h2>
                    <p><strong>Email:</strong> ${profile.email}</p>
                    <p><strong>Ngày sinh:</strong> ${profile.dob}</p>
                    <p><strong>Giới tính:</strong> ${profile.gender ? "Nam" : "Nữ"}</p>
                    <p><strong>Bằng cấp:</strong> ${profile.degree}</p>
                    <p><strong>Giới thiệu:</strong> ${profile.bio}</p>
                </div>
            `;
        } else if (role === 'staff') {
            // Use account sub-object for staff
            container.innerHTML = `
                <div class="profile-header">
                    <h2>${profile.account.fullname}</h2>
                    <p><strong>Email:</strong> ${profile.account.email}</p>
                    <p><strong>Ngày sinh:</strong> ${profile.account.dob}</p>
                    <p><strong>Giới tính:</strong> ${profile.account.gender ? "Nam" : "Nữ"}</p>
                    <p><strong>Bằng cấp:</strong> ${profile.degree}</p>
                    <p><strong>Giới thiệu:</strong> ${profile.bio}</p>
                </div>
            `;
        }
    } else {
        container.innerHTML = '<p>Failed to load profile.</p>';
    }
}

window.addEventListener('DOMContentLoaded', () => {
    const accId = localStorage.getItem('accId');
    // Get role from role-config.js
    let role = 'doctor';
    if (window.roleUtils && window.roleUtils.getUserRole && window.roleUtils.ROLE_NAMES) {
        const roleId = window.roleUtils.getUserRole();
        role = window.roleUtils.ROLE_NAMES[roleId];
    }
    renderUserProfile(accId, role, 'profileSection');
});
